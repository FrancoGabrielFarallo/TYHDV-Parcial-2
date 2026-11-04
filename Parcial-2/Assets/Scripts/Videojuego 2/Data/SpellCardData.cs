using UnityEngine;

// =====================================================================
//  SpellCardData.cs  -  Carta de HECHIZO
//  Click derecho en Project > Create > Cartas > Hechizo
//
//  Cómo se usa cada efecto al arrastrar la carta:
//   - DanioAlJugador:         soltala en el campo o en el retrato rival.
//   - DanioACriaturaOJugador: soltala ENCIMA de una criatura o de un retrato.
//   - PotenciarCriatura:      soltala ENCIMA de una criatura.
// =====================================================================
[CreateAssetMenu(fileName = "Hechizo_", menuName = "Cartas/Hechizo")]
public class SpellCardData : CardData
{
    [Header("Hechizo")]
    public EfectoHechizo efecto = EfectoHechizo.DanioAlJugador;

    [Tooltip("La X del efecto: cuánto daño hace")]
    public int cantidadDanio = 2;

    [Tooltip("Solo para PotenciarCriatura: el +X de ataque")]
    public int bonusAtaque = 2;
    [Tooltip("Solo para PotenciarCriatura: el +X de vida")]
    public int bonusVida = 2;
    [Tooltip("Si está tildado, el bonus se va al final del turno (como en Magic)")]
    public bool soloHastaFinDeTurno = true;

    public override TipoCarta ObtenerTipo()
    {
        return TipoCarta.Hechizo;
    }

    // Texto corto que explica el efecto (se muestra en la carta)
    public string ObtenerTextoEfecto()
    {
        if (efecto == EfectoHechizo.DanioAlJugador)
        {
            return "Hace " + cantidadDanio + " de daño al jugador rival.";
        }
        if (efecto == EfectoHechizo.DanioACriaturaOJugador)
        {
            return "Hace " + cantidadDanio + " de daño a una criatura o jugador.";
        }
        string duracion = "";
        if (soloHastaFinDeTurno) duracion = " hasta el fin del turno";
        return "Una criatura gana +" + bonusAtaque + "/+" + bonusVida + duracion + ".";
    }
}
