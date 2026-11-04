using System.Collections.Generic;
using UnityEngine;

// =====================================================================
//  CardCollection.cs  -  TODAS las cartas que existen en el juego.
//  El Deck Builder muestra esta lista para que elijas tu mazo.
//  Click derecho en Project > Create > Cartas > Coleccion
// =====================================================================
[CreateAssetMenu(fileName = "Coleccion", menuName = "Cartas/Coleccion")]
public class CardCollection : ScriptableObject
{
    public List<CardData> todasLasCartas = new List<CardData>();

    // Busca la tierra que produce un color (la usa "Rellenar con tierras")
    public LandCardData BuscarTierraDeColor(ColorMana color)
    {
        for (int i = 0; i < todasLasCartas.Count; i++)
        {
            LandCardData tierra = todasLasCartas[i] as LandCardData;
            if (tierra != null && tierra.colorQueProduce == color)
            {
                return tierra;
            }
        }
        return null;
    }
}
