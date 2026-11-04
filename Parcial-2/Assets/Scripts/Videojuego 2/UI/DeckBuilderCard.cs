using UnityEngine;
using UnityEngine.EventSystems;

// =====================================================================
//  DeckBuilderCard.cs  -  Detecta clics sobre una carta en el Deck Builder.
//  NO hace falta ponerlo a mano: el DeckBuilderManager lo agrega solo
//  con AddComponent cuando crea cada carta.
// =====================================================================
public class DeckBuilderCard : MonoBehaviour, IPointerClickHandler
{
    public DeckBuilderManager manager;
    public CardView vista;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            manager.AgregarCarta(vista.datos);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            manager.QuitarCarta(vista.datos);
        }
    }
}
