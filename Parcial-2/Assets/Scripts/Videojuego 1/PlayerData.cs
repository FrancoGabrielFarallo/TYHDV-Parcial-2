using UnityEngine;

// ScriptableObject con los datos "oficiales" del jugador.
// Se crea desde: click derecho en Project > Create > Plataformero > Player Data
[CreateAssetMenu(fileName = "PlayerData", menuName = "Plataformero/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Movimiento")]
    public float velocidad = 6f;        // unidades por segundo
    public float velocidadGiro = 10f;   // que tan rapido gira el personaje hacia donde camina

    [Header("Salto")]
    public float alturaSalto = 2f;      // altura maxima del salto en unidades
    public float alturaDobleSalto = 1.5f; // altura del segundo salto (en el aire)
    public float gravedad = -20f;       // negativa = hacia abajo
}