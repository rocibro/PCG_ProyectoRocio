using UnityEngine;
using UnityEngine.UI;
using System;                      // para Func y Action
using System.Collections.Generic;

public class PanelParametros : MonoBehaviour
{
    public MapRenderer generador;     // el generador del mundo
    public GameObject filaPrefab;     // la fila modelo (etiqueta + slider)
    public Transform contenedor;      // el objeto "Content" del Scroll View

    // Guarda los datos de cada fila para poder actualizarla después
    class FilaParametro
    {
        public string nombre;
        public Slider slider;
        public Text etiqueta;
        public bool entero;           // true = el slider solo da números enteros
        public bool interruptor;      // true = se muestra como Sí / No
        public Func<float> leer;      // función que lee el valor actual del generador
    }

    List<FilaParametro> filas = new List<FilaParametro>();

    void Start()
    {
        CrearFilas();
        Refrescar();
    }

    // aquí está la lista de todos los parámetros del menú.
    // cada línea dice: nombre, mínimo, máximo, si es entero, cómo leerlo y cómo guardarlo.
    void CrearFilas()
    {
        AgregarTitulo("MAPA");
        AgregarSlider("Tamaño del mapa", 32, 256, true,
            () => generador.ancho,
            v => { generador.ancho = (int)v; generador.alto = (int)v; });

        AgregarTitulo("RUIDO FRACTAL (terreno)");
        AgregarSlider("Escala", 5, 100, false,
            () => generador.ruido.escala, v => generador.ruido.escala = v);
        AgregarSlider("Octavas", 1, 8, true,
            () => generador.ruido.octavas, v => generador.ruido.octavas = (int)v);
        AgregarSlider("Persistencia", 0, 1, false,
            () => generador.ruido.persistencia, v => generador.ruido.persistencia = v);
        AgregarSlider("Lacunaridad", 1, 4, false,
            () => generador.ruido.lacunaridad, v => generador.ruido.lacunaridad = v);
        AgregarSlider("Desplazamiento X", -200, 200, false,
            () => generador.ruido.desplazamiento.x,
            v => generador.ruido.desplazamiento = new Vector2(v, generador.ruido.desplazamiento.y));
        AgregarSlider("Desplazamiento Y", -200, 200, false,
            () => generador.ruido.desplazamiento.y,
            v => generador.ruido.desplazamiento = new Vector2(generador.ruido.desplazamiento.x, v));

        AgregarTitulo("ALTURAS DEL TERRENO");
        AgregarSlider("Nivel agua", 0, 1, false,
            () => generador.nivelAgua, v => generador.nivelAgua = v);
        AgregarSlider("Nivel arena", 0, 1, false,
            () => generador.nivelArena, v => generador.nivelArena = v);
        AgregarSlider("Nivel pasto", 0, 1, false,
            () => generador.nivelPasto, v => generador.nivelPasto = v);
        AgregarSlider("Nivel roca", 0, 1, false,
            () => generador.nivelRoca, v => generador.nivelRoca = v);

        AgregarTitulo("AUTÓMATA CELULAR (cuevas)");
        AgregarInterruptor("Activar autómata",
            () => generador.automata.activar, v => generador.automata.activar = v);
        AgregarSlider("Relleno", 0, 1, false,
            () => generador.automata.relleno, v => generador.automata.relleno = v);
        AgregarSlider("Iteraciones", 0, 10, true,
            () => generador.automata.iteraciones, v => generador.automata.iteraciones = (int)v);
        AgregarSlider("Límite nacimiento", 0, 8, true,
            () => generador.automata.limiteNacimiento, v => generador.automata.limiteNacimiento = (int)v);
        AgregarSlider("Límite supervivencia", 0, 8, true,
            () => generador.automata.limiteSupervivencia, v => generador.automata.limiteSupervivencia = (int)v);
        AgregarSlider("Altura mínima", 0, 1, false,
            () => generador.automata.alturaMinima, v => generador.automata.alturaMinima = v);
        AgregarSlider("Altura máxima", 0, 1, false,
            () => generador.automata.alturaMaxima, v => generador.automata.alturaMaxima = v);

        AgregarTitulo("L-SYSTEM (árboles)");
        AgregarInterruptor("Activar árboles",
            () => generador.lsystem.activar, v => generador.lsystem.activar = v);
        AgregarSlider("Iteraciones", 1, 6, true,
            () => generador.lsystem.iteraciones, v => generador.lsystem.iteraciones = (int)v);
        AgregarSlider("Ángulo", 0, 90, false,
            () => generador.lsystem.angulo, v => generador.lsystem.angulo = v);
        AgregarSlider("Variación de ángulo", 0, 45, false,
            () => generador.lsystem.variacionAngulo, v => generador.lsystem.variacionAngulo = v);
        AgregarSlider("Largo de paso", 0.2f, 2f, false,
            () => generador.lsystem.largoPaso, v => generador.lsystem.largoPaso = v);
        AgregarSlider("Tamaño de hoja", 0, 3, true,
            () => generador.lsystem.radioHoja, v => generador.lsystem.radioHoja = (int)v);
        AgregarSlider("Cantidad de árboles", 0, 100, true,
            () => generador.lsystem.cantidad, v => generador.lsystem.cantidad = (int)v);
        AgregarSlider("Distancia mínima", 2, 30, false,
            () => generador.lsystem.distanciaMinima, v => generador.lsystem.distanciaMinima = v);
        AgregarSlider("Altura mínima", 0, 1, false,
            () => generador.lsystem.alturaMinima, v => generador.lsystem.alturaMinima = v);
        AgregarSlider("Altura máxima", 0, 1, false,
            () => generador.lsystem.alturaMaxima, v => generador.lsystem.alturaMaxima = v);
    }

    // agrega un título de sección 
    void AgregarTitulo(string texto)
    {
        GameObject obj = Instantiate(filaPrefab, contenedor);

        Slider sl = obj.GetComponentInChildren<Slider>();
        Destroy(sl.gameObject);

        Text et = obj.GetComponentInChildren<Text>();
        et.text = texto;
        et.fontStyle = FontStyle.Bold;

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, 50);
    }

    // agrega una fila con etiqueta y slider conectada a un parámetro
    void AgregarSlider(string nombre, float min, float max, bool entero,
                       Func<float> leer, Action<float> guardar)
    {
        GameObject obj = Instantiate(filaPrefab, contenedor);

        FilaParametro fila = new FilaParametro();
        fila.nombre = nombre;
        fila.slider = obj.GetComponentInChildren<Slider>();
        fila.etiqueta = obj.GetComponentInChildren<Text>();
        fila.entero = entero;
        fila.leer = leer;

        fila.slider.minValue = min;
        fila.slider.maxValue = max;
        fila.slider.wholeNumbers = entero;

        // cuando se mueve el slider: guarda el valor, actualiza el texto y regenera el mapa
        fila.slider.onValueChanged.AddListener((float v) =>
        {
            guardar(v);
            ActualizarEtiqueta(fila);
            generador.Generar();
        });

        filas.Add(fila);
    }

    // un interruptor es un slider de 0 a 1 que se muestra como Sí / No
    void AgregarInterruptor(string nombre, Func<bool> leer, Action<bool> guardar)
    {
        AgregarSlider(nombre, 0, 1, true,
            () => leer() ? 1f : 0f,
            v => guardar(v > 0.5f));

        filas[filas.Count - 1].interruptor = true;
    }

    // escribe "Nombre: valor" en la etiqueta
    void ActualizarEtiqueta(FilaParametro fila)
    {
        string valor;

        if (fila.interruptor)
            valor = fila.slider.value > 0.5f ? "Sí" : "No";
        else if (fila.entero)
            valor = fila.slider.value.ToString("F0");
        else
            valor = fila.slider.value.ToString("F2");

        fila.etiqueta.text = fila.nombre + ": " + valor;
    }

    // pone cada slider en el valor actual del generador (se usa al cambiar de contexto)
    public void Refrescar()
    {
        foreach (FilaParametro fila in filas)
        {
            // SetValueWithoutNotify cambia el slider sin regenerar el mapa
            fila.slider.SetValueWithoutNotify(fila.leer());
            ActualizarEtiqueta(fila);
        }
    }
}