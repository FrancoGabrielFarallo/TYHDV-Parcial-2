using UnityEngine;

// =====================================================================
//  GameRules.cs  -  Los NÚMEROS de las reglas, en un ScriptableObject.
//  Si querés cambiar la vida inicial o el tamaño del mazo, se cambia acá
//  sin tocar el código.
//  Click derecho en Project > Create > Cartas > Reglas del juego
// =====================================================================
[CreateAssetMenu(fileName = "ReglasDelJuego", menuName = "Cartas/Reglas del juego")]
public class GameRules : ScriptableObject
{
    [Header("Jugadores")]
    public int vidaInicial = 20;
    public int cartasManoInicial = 7;

    [Header("Mazo")]
    public int cartasEnElMazo = 60;
    [Tooltip("Copias máximas de una misma carta (las tierras no tienen límite)")]
    public int maximoCopiasPorCarta = 4;

    [Header("Turnos")]
    public int tierrasPorTurno = 1;
    [Tooltip("El que empieza no roba carta en su primer turno")]
    public bool primerJugadorNoRoba = true;
    [Tooltip("Una criatura no puede atacar el turno en que entra")]
    public bool mareoDeInvocacion = true;
    [Tooltip("El daño de las criaturas se borra al final de cada turno")]
    public bool curarCriaturasAlFinalDelTurno = true;

    [Header("IA")]
    [Tooltip("Segundos que espera la IA entre cada acción (para que se vea)")]
    public float esperaEntreAccionesIA = 1.0f;

    [Header("Colores de las cartas (orden: Rojo, Verde, Blanco, Negro, Azul)")]
    public Color[] coloresMana = new Color[]
    {
        new Color(0.85f, 0.35f, 0.30f), // Rojo
        new Color(0.35f, 0.70f, 0.40f), // Verde
        new Color(0.95f, 0.93f, 0.80f), // Blanco
        new Color(0.45f, 0.40f, 0.45f), // Negro
        new Color(0.35f, 0.55f, 0.90f)  // Azul
    };
    public Color colorSinColor = new Color(0.7f, 0.7f, 0.7f);
}
