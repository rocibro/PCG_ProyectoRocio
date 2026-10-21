using UnityEngine;

// Parámetros del autómata, editables en el Inspector
[System.Serializable]
public class CellularSettings
{
    public bool activar = true;                         // checkbox para prender/apagar el algoritmo
    [Range(0f, 1f)] public float relleno = 0.42f;       // % de celdas vivas al inicio
    [Range(0, 10)] public int iteraciones = 5;          // cuántas veces se repite la regla
    [Range(0, 8)] public int limiteNacimiento = 5;      // vecinas vivas para que una celda muerta nazca
    [Range(0, 8)] public int limiteSupervivencia = 4;   // vecinas vivas para que una celda viva sobreviva

    [Header("Zona donde actúa (altura 0 a 1)")]
    [Range(0f, 1f)] public float alturaMinima = 0.70f;
    [Range(0f, 1f)] public float alturaMaxima = 1f;
}

public static class CellularAutomata
{
    // Devuelve una grilla de true/false. true = hueco (cueva o laguna)
    public static bool[,] Generar(int ancho, int alto, int seed, CellularSettings s)
    {
        System.Random rng = new System.Random(seed);
        bool[,] grilla = new bool[ancho, alto];

        // 1) Llenado inicial al azar
        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                grilla[x, y] = rng.NextDouble() < s.relleno;
            }
        }

        // 2) Repetimos la regla varias veces
        for (int i = 0; i < s.iteraciones; i++)
        {
            // Creamos una grilla NUEVA para guardar el resultado.
            // Si cambiáramos la misma grilla que estamos leyendo, las celdas
            // ya modificadas afectarían a las siguientes y saldría un resultado raro.
            bool[,] nueva = new bool[ancho, alto];

            for (int x = 0; x < ancho; x++)
            {
                for (int y = 0; y < alto; y++)
                {
                    int vecinas = ContarVecinasVivas(grilla, x, y, ancho, alto);

                    if (grilla[x, y])
                        nueva[x, y] = vecinas >= s.limiteSupervivencia; // estaba viva
                    else
                        nueva[x, y] = vecinas >= s.limiteNacimiento;    // estaba muerta
                }
            }

            grilla = nueva; // la nueva grilla pasa a ser la actual
        }

        return grilla;
    }

    // Cuenta cuántas de las 8 vecinas están vivas
    static int ContarVecinasVivas(bool[,] grilla, int x, int y, int ancho, int alto)
    {
        int cuenta = 0;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue; // no nos contamos a nosotros mismos

                int vx = x + dx;
                int vy = y + dy;

                // Si la vecina está fuera del mapa, la ignoramos
                if (vx < 0 || vx >= ancho || vy < 0 || vy >= alto) continue;

                if (grilla[vx, vy]) cuenta++;
            }
        }

        return cuenta;
    }
}