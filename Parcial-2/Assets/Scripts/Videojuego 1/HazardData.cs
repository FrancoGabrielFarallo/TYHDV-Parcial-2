using UnityEngine;

// ScriptableObject con los datos del cubo que sube (lava, agua, etc.).
// Se crea desde: click derecho en Project > Create > Plataformero > Hazard Data
[CreateAssetMenu(fileName = "HazardData", menuName = "Plataformero/Hazard Data")]
public class HazardData : ScriptableObject
{
    [Header("Activacion")]
    public float distanciaActivacion = 5f; // cuando el jugador se aleja MAS que esto, el cubo empieza a subir

    [Header("Subida")]
    public float velocidadSubida = 1f;     // unidades por segundo

    [Header("Limite (opcional)")]
    public bool tieneLimite = false;       // si esta activado, deja de subir al llegar a la altura maxima
    public float alturaMaxima = 50f;       // altura Y del mundo
}