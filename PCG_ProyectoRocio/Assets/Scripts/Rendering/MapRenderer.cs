using UnityEngine;
using UnityEngine.UI;

public class MapRenderer : MonoBehaviour
{
    public RawImage pantalla; //donde se mostrará la imagen
    public int ancho = 128; //cantidad de celdas a lo ancho
    public int alto = 128; //cantidad de celdas a lo alto
    public int seed = 1;

    [Header("Ruido")]
    public NoiseSettings ruido;

    [Header("Autómata celular")]                
    public CellularSettings automata;           
    public Color colorHueco = new Color(0.1f, 0.1f, 0.1f);

    [Header("L-System")]                                            
    public LSystemSettings lsystem;                                 
    public Color colorTronco = new Color(0.4f, 0.25f, 0.1f, 1f);    
    public Color colorHoja = new Color(0.05f, 0.4f, 0.1f, 1f);      

    [Header("Umbrales de altura (0 a 1)")]
    [Range(0f, 1f)] public float nivelAgua = 0.40f;
    [Range(0f, 1f)] public float nivelArena = 0.45f;
    [Range(0f, 1f)] public float nivelPasto = 0.70f;
    [Range(0f, 1f)] public float nivelRoca = 0.88f;

    [Header("Colores")]
    public Color colorAguaProfunda = new Color(0.1f, 0.2f, 0.6f);
    public Color colorAgua = new Color(0.2f, 0.4f, 0.8f);
    public Color colorArena = new Color(0.9f, 0.85f, 0.55f);
    public Color colorPasto = new Color(0.2f, 0.6f, 0.2f);
    public Color colorRoca = new Color(0.5f, 0.5f, 0.5f);
    public Color colorNieve = Color.white;

    void Start()
    {
        Generar();
    }

    void Update()
    {
        // Barra espaciadora = nueva seed al azar
        if (Input.GetKeyDown(KeyCode.Space))
        {
            seed = Random.Range(0, 100000);
            Generar();
        }
        // Tecla G = regenerar con los valores actuales
        if (Input.GetKeyDown(KeyCode.G))
        {
            Generar();
        }
    }

    public void Generar()
    {
        //terrenos con ruido
        float[,] alturas = NoiseGenerator.Generar(ancho, alto, seed, ruido);

        //cuevas: el autómata solo cuenta dentro de su rango de altura
        bool[,] cuevas = new bool[ancho, alto];                              
        if (automata.activar)
        {
            bool[,] huecos = CellularAutomata.Generar(ancho, alto, seed, automata);
            for (int x = 0; x < ancho; x++)
            {
                for (int y = 0; y < alto; y++)
                {
                    float h = alturas[x, y];
                    cuevas[x, y] = huecos[x, y]
                        && h >= automata.alturaMinima && h <= automata.alturaMaxima;
                }
            }
        }

        //los árboles necesitan las alturas y saber dónde hay cuevas       
        int[,] arboles = null;                                               
        if (lsystem.activar)                                                 
        {                                                                    
            arboles = LSystemGenerator.Generar(ancho, alto, seed, lsystem, alturas, cuevas); 
        }

        //pintamos todo, de abajo hacia arriba: terreno, cuevas, árboles
        Texture2D tex = new Texture2D(ancho, alto);
        tex.filterMode = FilterMode.Point;

        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                Color c = ColorPorAltura(alturas[x, y]);

                if (cuevas[x, y]) c = colorHueco;

                if (arboles != null)                                         
                {                                                            
                    if (arboles[x, y] == LSystemGenerator.RAMA) c = colorTronco; 
                    if (arboles[x, y] == LSystemGenerator.HOJA) c = colorHoja;   
                }                                                            

                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply();
        pantalla.texture = tex;
    }

    Color ColorPorAltura(float h)
    {
        if (h < nivelAgua * 0.6f) return colorAguaProfunda;
        if (h < nivelAgua) return colorAgua;
        if (h < nivelArena) return colorArena;
        if (h < nivelPasto) return colorPasto;
        if (h < nivelRoca) return colorRoca;
        return colorNieve;
    }
}
