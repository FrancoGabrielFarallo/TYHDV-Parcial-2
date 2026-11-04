using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// =====================================================================
//  PlayerPortrait.cs  -  El "retrato" de un jugador en la UI.
//  Muestra vida, cartas en mazo/mano y mana disponible.
//  Hacer clic en el retrato rival = atacar al jugador.
//  Soltar un hechizo encima = apuntar al jugador.
// =====================================================================
public class PlayerPortrait : MonoBehaviour, IPointerClickHandler
{
    public Jugador duenio;

    [Header("Referencias de UI")]
    public Image imagenRetrato;
    public TMP_Text textoNombre;
    public TMP_Text textoVida;
    public TMP_Text textoCartas;
    public TMP_Text textoMana;

    public void Actualizar()
    {
        if (duenio == null) return;

        if (textoNombre != null && duenio.datos != null) textoNombre.text = duenio.datos.nombreJugador;
        if (imagenRetrato != null && duenio.datos != null && duenio.datos.retrato != null)
        {
            imagenRetrato.sprite = duenio.datos.retrato;
        }
        if (textoVida != null) textoVida.text = "Vida: " + duenio.vida;
        if (textoCartas != null)
        {
            textoCartas.text = "Mazo: " + duenio.mazo.Count + "  Mano: " + duenio.mano.Count;
        }
        if (textoMana != null) textoMana.text = duenio.ObtenerTextoMana();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameManager.instancia == null) return;
        GameManager.instancia.RetratoClickeado(duenio);
    }
}
