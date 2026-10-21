using UnityEngine;

[System.Serializable] //agrupa los parámetros del ruido para poder editarlos en el inspector
public class NoiseSettings
{
    public float escala = 30f;          // "zoom": más alto = manchas más grandes
    [Range(1, 8)] public int octavas = 4;          // capas de detalle
    [Range(0f, 1f)] public float persistencia = 0.5f; // cuánto pesa cada capa nueva
    public float lacunaridad = 2f;      // cuánto más fino es el detalle de cada capa
    public Vector2 desplazamiento;      // mueve el mapa sin cambiar la seed  
}

//estática ya que solo se usa como herramienta
public static class NoiseGenerator 
{
    public static float[,] Generar(int ancho, int alto, int seed, NoiseSettings s)
    {
        float[,] mapa = new float[ancho, alto];
        float escala = Mathf.Max(0.0001f, s.escala);

        // Con la seed generamos un punto de partida distinto por cada octava
        System.Random rng = new System.Random(seed);
        Vector2[] offsets = new Vector2[s.octavas];
        for (int i = 0; i < s.octavas; i++)
        {
            offsets[i] = new Vector2(
                rng.Next(-10000, 10000) + s.desplazamiento.x,
                rng.Next(-10000, 10000) + s.desplazamiento.y);
        }

        float min = float.MaxValue;
        float max = float.MinValue;

        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                float amplitud = 1f;
                float frecuencia = 1f;
                float valor = 0f;

                // Sumamos todas las octavas
                for (int o = 0; o < s.octavas; o++)
                {
                    float sx = (x / escala) * frecuencia + offsets[o].x;
                    float sy = (y / escala) * frecuencia + offsets[o].y;
                    float perlin = Mathf.PerlinNoise(sx, sy) * 2f - 1f; // rango -1 a 1
                    valor += perlin * amplitud;

                    amplitud *= s.persistencia;
                    frecuencia *= s.lacunaridad;
                }

                mapa[x, y] = valor;
                if (valor < min) min = valor;
                if (valor > max) max = valor;
            }
        }

        // Normalizamos todo para que quede entre 0 y 1
        for (int x = 0; x < ancho; x++)
            for (int y = 0; y < alto; y++)
                mapa[x, y] = Mathf.InverseLerp(min, max, mapa[x, y]);

        return mapa;
    }
}
