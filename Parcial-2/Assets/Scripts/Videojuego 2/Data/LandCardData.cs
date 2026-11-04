using UnityEngine;

// =====================================================================
//  LandCardData.cs  -  Carta de TIERRA (mana)
//  Click derecho en Project > Create > Cartas > Tierra
// =====================================================================
[CreateAssetMenu(fileName = "Tierra_", menuName = "Cartas/Tierra")]
public class LandCardData : CardData
{
    [Header("Tierra")]
    public ColorMana colorQueProduce = ColorMana.Rojo;

    public override TipoCarta ObtenerTipo()
    {
        return TipoCarta.Tierra;
    }

    // Una tierra se pinta del color que produce
    public override int ObtenerColorPrincipal()
    {
        return (int)colorQueProduce;
    }
}
