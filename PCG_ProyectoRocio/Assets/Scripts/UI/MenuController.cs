using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public MapRenderer generador;        // el generador del mundo
    public Dropdown dropdownContexto;    // lista desplegable de contextos
    public InputField campoSeed;         // campo donde se escribe la seed
    public Button botonGenerar;
    public Button botonSeedAleatoria;
    public PanelParametros panelParametros;

    void Start()
    {
        //le decimos a cada control qué función ejecutar cuando se usa
        botonGenerar.onClick.AddListener(AlPresionarGenerar);
        botonSeedAleatoria.onClick.AddListener(AlPresionarSeedAleatoria);
        dropdownContexto.onValueChanged.AddListener(AlCambiarContexto);

        campoSeed.text = generador.seed.ToString();
    }

    // botón Generar: usa la seed escrita en el campo
    void AlPresionarGenerar()
    {
        LeerSeed();
        generador.Generar();
    }

    // botón Seed aleatoria: elige una seed nueva y la muestra en el campo
    void AlPresionarSeedAleatoria()
    {
        generador.seed = Random.Range(0, 100000);
        campoSeed.text = generador.seed.ToString();
        generador.Generar();
    }

    // Dropdown: 0 = isla, 1 = volcán
    void AlCambiarContexto(int opcion)
    {
        LeerSeed();

        if (opcion == 0) generador.CargarIsla();
        else generador.CargarVolcan();

        panelParametros.Refrescar();   // los sliders muestran los valores del nuevo contexto
    }

    // Lee el número del campo y lo guarda como seed (si el campo está vacío, no cambia nada)
    void LeerSeed()
    {
        int nuevaSeed;
        if (int.TryParse(campoSeed.text, out nuevaSeed))
        {
            generador.seed = nuevaSeed;
        }
    }
}