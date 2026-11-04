using UnityEngine;

// Los 3 tipos posibles de objeto recogible
public enum TipoRecogible
{
    Coleccionable,
    AumentoVelocidad,
    AumentoSalto
}

// ScriptableObject con los datos de un recogible.
// Se crea desde: click derecho en Project > Create > Plataformero > Pickup Data
// Podes tener varios: "Moneda", "PocionVelocidad", "PocionSalto", etc.
[CreateAssetMenu(fileName = "PickupData", menuName = "Plataformero/Pickup Data")]
public class PickupData : ScriptableObject
{
    [Header("Tipo")]
    public TipoRecogible tipo = TipoRecogible.Coleccionable;

    [Header("Coleccionable")]
    public int puntos = 1;              // cuanto suma al contador

    [Header("Aumentos temporales (velocidad o salto)")]
    public float multiplicador = 1.5f;  // 1.5 = 50% mas; 2 = el doble
    public float duracion = 5f;         // segundos que dura el efecto
}