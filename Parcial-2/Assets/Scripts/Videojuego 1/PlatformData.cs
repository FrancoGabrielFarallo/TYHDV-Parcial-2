using UnityEngine;

// Como se mueve la plataforma
public enum TipoMovimientoPlataforma
{
    EjeX,
    EjeY,
    EjeZ,
    Circular
}

// Hacia donde arranca
public enum SentidoPlataforma
{
    NegativoAPositivo,  // ejes: arranca yendo hacia + | circular: antihorario
    PositivoANegativo   // ejes: arranca yendo hacia - | circular: horario
}

// ScriptableObject con los datos de una plataforma movil.
// Se crea desde: click derecho en Project > Create > Plataformero > Platform Data
[CreateAssetMenu(fileName = "PlatformData", menuName = "Plataformero/Platform Data")]
public class PlatformData : ScriptableObject
{
    [Header("Movimiento")]
    public TipoMovimientoPlataforma tipoMovimiento = TipoMovimientoPlataforma.EjeX;
    public SentidoPlataforma sentido = SentidoPlataforma.NegativoAPositivo;
    public float velocidad = 2f;        // unidades por segundo (tambien en circular)

    [Header("Ejes X / Y / Z")]
    public float distancia = 3f;        // cuanto se aleja hacia CADA lado de su posicion inicial

    [Header("Circular")]
    public float radio = 3f;            // la posicion inicial es el CENTRO del circulo
    public bool circuloVertical = false; // false = gira acostado (plano XZ) | true = parado (plano XY)
}