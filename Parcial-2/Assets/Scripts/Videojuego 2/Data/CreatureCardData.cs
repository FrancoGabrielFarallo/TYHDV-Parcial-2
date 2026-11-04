using UnityEngine;

// =====================================================================
//  CreatureCardData.cs  -  Carta de CRIATURA
//  Click derecho en Project > Create > Cartas > Criatura
// =====================================================================
[CreateAssetMenu(fileName = "Criatura_", menuName = "Cartas/Criatura")]
public class CreatureCardData : CardData
{
    [Header("Criatura")]
    public int ataque = 1;
    public int vida = 1;
    public TipoMovimiento movimiento = TipoMovimiento.Tierra;

    public override TipoCarta ObtenerTipo()
    {
        return TipoCarta.Criatura;
    }

    public bool EsVoladora()
    {
        return movimiento == TipoMovimiento.Voladora;
    }
}
