using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;   //para crear la lista de reglas del L-System

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

    void Awake()
    {
        CargarIsla(); // el programa parte con el contexto de la isla
    }
    public void CargarIsla()
    {
        // Ruido: pocas octavas = formas suaves. Nivel de agua alto = archipiélago
        ruido.escala = 45f;
        ruido.octavas = 3;
        ruido.persistencia = 0.45f;
        ruido.lacunaridad = 2f;
        ruido.desplazamiento = Vector2.zero;

        nivelAgua = 0.52f;
        nivelArena = 0.56f;
        nivelPasto = 0.74f;
        nivelRoca = 0.88f;

        // Autómata: lagunas interiores dentro de la selva
        automata.activar = true;
        automata.relleno = 0.42f;
        automata.iteraciones = 5;
        automata.limiteNacimiento = 5;
        automata.limiteSupervivencia = 4;
        automata.alturaMinima = 0.62f;
        automata.alturaMaxima = 0.74f;

        // L-System: palmeras (tronco recto + copa abierta). 
        lsystem.activar = true;
        lsystem.axioma = "TTTTX";
        lsystem.reglas = new List<ReglaLSystem>()
        {
            // Copa grande (se usa la mitad de las veces)
            new ReglaLSystem { simbolo = "X",
                reemplazo = "[+Lf+L][-Lf-L][++L][--L]L",
                probabilidad = 0.5f },
            // Copa chica (el resto de las veces)
            new ReglaLSystem { simbolo = "X",
                reemplazo = "[+Lf+L][-Lf-L]L",
                probabilidad = 1f }
        };
        lsystem.iteraciones = 2;
        lsystem.angulo = 60.18f;
        lsystem.variacionAngulo = 0.0f;
        lsystem.largoPaso = 1.67f;
        lsystem.anguloInicial = 90f;
        lsystem.radioHoja = 0;
        lsystem.cantidad = 22;
        lsystem.distanciaMinima = 9f;
        lsystem.alturaMinima = 0.56f;
        lsystem.alturaMaxima = 0.68f;

        // Colores
        colorAguaProfunda = Hex("1F5F8B");
        colorAgua = Hex("3FA7D6");
        colorArena = Hex("F2E2A8");
        colorPasto = Hex("3DAA4F");
        colorRoca = Hex("7D7A6B");
        colorNieve = Hex("D9D3C0");
        colorHueco = Hex("6FD0E8");   // laguna
        colorTronco = Hex("6B4423");
        colorHoja = Hex("1B6E2B");

        Generar();
    }

    public void CargarVolcan()
    {
        // Ruido: muchas octavas y persistencia alta = terreno rugoso. El "agua" es lava
        ruido.escala = 36f;
        ruido.octavas = 5;
        ruido.persistencia = 0.5f;
        ruido.lacunaridad = 2.2f;
        ruido.desplazamiento = Vector2.zero;

        nivelAgua = 0.32f;
        nivelArena = 0.36f;
        nivelPasto = 0.72f;
        nivelRoca = 0.88f;

        // Autómata: cavernas de magma, más iteraciones y relleno más denso
        automata.activar = true;
        automata.relleno = 0.47f;
        automata.iteraciones = 6;
        automata.limiteNacimiento = 5;
        automata.limiteSupervivencia = 4;
        automata.alturaMinima = 0.68f;
        automata.alturaMaxima = 1f;

        // L-System: árboles secos. Si la regla de X no "sale", esa rama se corta
        lsystem.activar = true;
        lsystem.axioma = "TX";
        lsystem.reglas = new List<ReglaLSystem>()
        {
            new ReglaLSystem { simbolo = "F", reemplazo = "FF", probabilidad = 1f },
            new ReglaLSystem { simbolo = "X", reemplazo = "F[+X][-X]", probabilidad = 0.8f }
        };
        lsystem.iteraciones = 4;
        lsystem.angulo = 42f;
        lsystem.variacionAngulo = 15f;
        lsystem.largoPaso = 0.9f;
        lsystem.anguloInicial = 90f;
        lsystem.radioHoja = 0;
        lsystem.cantidad = 16;
        lsystem.distanciaMinima = 12f;
        lsystem.alturaMinima = 0.38f;
        lsystem.alturaMaxima = 0.70f;

        // Colores
        colorAguaProfunda = Hex("8C1608");  // lava profunda
        colorAgua = Hex("E0401A");          // lava
        colorArena = Hex("3E2B2B");         // costra enfriada
        colorPasto = Hex("34343C");         // roca basáltica
        colorRoca = Hex("5A5A64");
        colorNieve = Hex("8E8884");         // ceniza en la cumbre
        colorHueco = Hex("FFD23F");         // magma
        colorTronco = Hex("CDBB9B");        // árbol seco, color hueso
        colorHoja = Hex("7A3E1D");

        Generar();
    }

    // Convierte un código como "3DAA4F" en un Color            
    Color Hex(string codigo)
    {
        Color c;
        if (!ColorUtility.TryParseHtmlString("#" + codigo, out c))
            c = Color.magenta;   // si el código está mal escrito, se ve magenta
        return c;
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
