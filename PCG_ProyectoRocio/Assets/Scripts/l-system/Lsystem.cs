using UnityEngine;
using System.Collections.Generic; //Para List y Stack
using System.Text;

// Una regla de reemplazo, por ejemplo: F -> FF
[System.Serializable]
public class ReglaLSystem
{
    public string simbolo = "F";     // letra que se reemplaza (escribe solo UNA letra)
    public string reemplazo = "FF";  // texto que la reemplaza
    [Range(0f, 1f)] public float probabilidad = 1f; // 1 = siempre se aplica (menos = estocástico)
}

// Parámetros del L-System, editables en el Inspector
[System.Serializable]
public class LSystemSettings
{
    public bool activar = true;

    [Header("Gramática")]
    public string axioma = "TTL";    // texto inicial (ejercicio 4.4 de tu Lab8)
    public List<ReglaLSystem> reglas = new List<ReglaLSystem>()
    {
        new ReglaLSystem { simbolo = "F", reemplazo = "FF", probabilidad = 1f },
        new ReglaLSystem { simbolo = "L", reemplazo = "F[+L][-L]", probabilidad = 1f }
    };
    [Range(1, 6)] public int iteraciones = 4;

    [Header("Dibujo")]
    public float angulo = 40f;           // cuánto gira con + y -
    public float variacionAngulo = 5f;   // azar que se suma al giro (árboles menos perfectos)
    public float largoPaso = 0.7f;       // largo de cada F, en pixeles
    public float anguloInicial = 90f;    // dirección de partida (90 = hacia arriba)
    [Range(0, 3)] public int radioHoja = 1;   // tamaño de las hojas (0 = un pixel)

    [Header("Colocación")]
    [Range(0, 100)] public int cantidad = 18;  // cuántos árboles intentar colocar
    public float distanciaMinima = 11f;         // separación mínima entre árboles
    [Range(0f, 1f)] public float alturaMinima = 0.46f; // zona de altura donde pueden crecer
    [Range(0f, 1f)] public float alturaMaxima = 0.70f;
}

public static class LSystemGenerator
{
    // Valores que tendrá cada celda de la grilla de salida
    public const int NADA = 0;
    public const int RAMA = 1;   // tronco y ramas (F, T)
    public const int HOJA = 2;   // hojas (L)

    // Tope para que el texto no crezca sin control y congele el programa
    const int LARGO_MAXIMO = 100000;

    public static int largoCadena;
    public static int arbolesColocados;

    // Devuelve una grilla: NADA, RAMA u HOJA en cada celda
    public static int[,] Generar(int ancho, int alto, int seed, LSystemSettings s,
                                 float[,] alturas, bool[,] cuevas)
    {
        System.Random rng = new System.Random(seed + 1);
        int[,] arboles = new int[ancho, alto];
        List<Vector2Int> colocados = new List<Vector2Int>();

        // Intentamos colocar árboles en posiciones al azar.
        // Limitamos los intentos para no quedar en un ciclo infinito.
        int intentos = 0;
        while (colocados.Count < s.cantidad && intentos < s.cantidad * 30)
        {
            intentos++;

            int x = rng.Next(0, ancho);
            int y = rng.Next(0, alto);

            // Solo crece dentro de la zona de altura permitida
            float h = alturas[x, y];
            if (h < s.alturaMinima || h > s.alturaMaxima) continue;

            // No crece dentro de una cueva
            if (cuevas[x, y]) continue;

            // No crece muy cerca de otro árbol
            bool muyCerca = false;
            foreach (Vector2Int otro in colocados)
            {
                if (Vector2Int.Distance(new Vector2Int(x, y), otro) < s.distanciaMinima)
                {
                    muyCerca = true;
                    break;
                }
            }
            if (muyCerca) continue;

            // Posición válida: creamos el texto del árbol y lo dibujamos
            colocados.Add(new Vector2Int(x, y));
            string cadena = Expandir(s, rng);
            largoCadena = cadena.Length;
            Dibujar(cadena, new Vector2(x, y), s, rng, arboles, ancho, alto);
        }

        arbolesColocados = colocados.Count;
        return arboles;
    }

    //aplica las reglas varias veces
    static string Expandir(LSystemSettings s, System.Random rng)
    {
        string actual = s.axioma;

        for (int i = 0; i < s.iteraciones; i++)
        {
            StringBuilder nuevo = new StringBuilder();

            foreach (char letra in actual)
            {
                string reemplazo = BuscarReemplazo(letra, s, rng);

                if (reemplazo != null)
                    nuevo.Append(reemplazo);  // la letra tenía regla: escribimos el reemplazo
                else
                    nuevo.Append(letra);      // sin regla: queda igual
            }

            actual = nuevo.ToString();

            if (actual.Length > LARGO_MAXIMO) break; // seguridad
        }

        return actual;
    }

    // Busca una regla para la letra. Devuelve null si no hay ninguna.
    // Si hay varias reglas para la misma letra, se prueban en orden y se usa la
    // primera cuya probabilidad "salga" (así los árboles salen distintos).
    static string BuscarReemplazo(char letra, LSystemSettings s, System.Random rng)
    {
        foreach (ReglaLSystem regla in s.reglas)
        {
            if (string.IsNullOrEmpty(regla.simbolo)) continue;
            if (regla.simbolo[0] != letra) continue;

            if (rng.NextDouble() < regla.probabilidad)
                return regla.reemplazo;
        }
        return null;
    }

    // lee el texto y mueve la tortuga
    static void Dibujar(string cadena, Vector2 inicio, LSystemSettings s,
                        System.Random rng, int[,] arboles, int ancho, int alto)
    {
        Vector2 pos = inicio;
        float angulo = s.anguloInicial;

        // Pila para los corchetes: [ guarda (x, y, ángulo) y ] vuelve a lo guardado
        Stack<Vector3> pila = new Stack<Vector3>();

        foreach (char c in cadena)
        {
            if (c == 'F')        // avanza dibujando
            {
                Vector2 destino = Destino(pos, angulo, s.largoPaso);
                DibujarLinea(pos, destino, RAMA, 0, arboles, ancho, alto);
                pos = destino;
            }
            else if (c == 'f')   // avanza sin dibujar
            {
                pos = Destino(pos, angulo, s.largoPaso);
            }
            else if (c == 'T')   // tronco: avanza dibujando el DOBLE
            {
                Vector2 destino = Destino(pos, angulo, 2f * s.largoPaso);
                DibujarLinea(pos, destino, RAMA, 0, arboles, ancho, alto);
                pos = destino;
            }
            else if (c == 'L')   // hoja: dibuja 1.5 veces el paso, pero NO mueve la tortuga
            {
                Vector2 destino = Destino(pos, angulo, 1.5f * s.largoPaso);
                DibujarLinea(pos, destino, HOJA, s.radioHoja, arboles, ancho, alto);
            }
            else if (c == '+')   // gira a la izquierda
            {
                angulo += s.angulo + Variacion(s, rng);
            }
            else if (c == '-')   // gira a la derecha
            {
                angulo -= s.angulo + Variacion(s, rng);
            }
            else if (c == '[')   // guardar posición y ángulo
            {
                pila.Push(new Vector3(pos.x, pos.y, angulo));
            }
            else if (c == ']')   // volver a lo guardado
            {
                if (pila.Count > 0)
                {
                    Vector3 guardado = pila.Pop();
                    pos = new Vector2(guardado.x, guardado.y);
                    angulo = guardado.z;
                }
            }
            // Cualquier otra letra (como X) no hace nada al dibujar
        }
    }

    // Punto al que llegas si avanzas "largo" desde "pos" mirando hacia "angulo" (en grados)
    static Vector2 Destino(Vector2 pos, float angulo, float largo)
    {
        float rad = angulo * Mathf.Deg2Rad;
        return pos + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * largo;
    }

    // Número al azar entre -variacionAngulo y +variacionAngulo
    static float Variacion(LSystemSettings s, System.Random rng)
    {
        return ((float)rng.NextDouble() * 2f - 1f) * s.variacionAngulo;
    }

    // Pinta las celdas entre dos puntos. "radio" engorda la línea (0 = 1 pixel, 1 = cuadrado de 3x3)
    static void DibujarLinea(Vector2 a, Vector2 b, int tipo, int radio, int[,] arboles, int ancho, int alto)
    {
        int pasos = Mathf.CeilToInt(Vector2.Distance(a, b) * 2f) + 1;

        for (int i = 0; i <= pasos; i++)
        {
            Vector2 p = Vector2.Lerp(a, b, (float)i / pasos);
            int cx = Mathf.RoundToInt(p.x);
            int cy = Mathf.RoundToInt(p.y);

            // Pintamos un cuadrado centrado en el punto
            for (int dx = -radio; dx <= radio; dx++)
            {
                for (int dy = -radio; dy <= radio; dy++)
                {
                    int x = cx + dx;
                    int y = cy + dy;

                    if (x < 0 || x >= ancho || y < 0 || y >= alto) continue;

                    // Una hoja no tapa a una rama que ya estaba ahí
                    if (tipo == HOJA && arboles[x, y] != NADA) continue;

                    arboles[x, y] = tipo;
                }
            }
        }
    }
}