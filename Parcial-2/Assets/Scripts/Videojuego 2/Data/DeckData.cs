using System.Collections.Generic;
using UnityEngine;

// =====================================================================
//  DeckData.cs  -  Un MAZO guardado como ScriptableObject
//  Click derecho en Project > Create > Cartas > Mazo
//
//  En vez de una lista de 60 cartas sueltas, guardamos "entradas":
//  carta + cantidad. Ej: 20x Montaña, 4x Goblin...
//  Así es mucho más fácil armar el mazo de la IA a mano en el Inspector.
// =====================================================================

// Una línea del mazo: qué carta y cuántas copias.
// [System.Serializable] hace que se vea en el Inspector.
[System.Serializable]
public class EntradaMazo
{
    public CardData carta;
    public int cantidad = 1;
}

[CreateAssetMenu(fileName = "Mazo_", menuName = "Cartas/Mazo")]
public class DeckData : ScriptableObject
{
    public string nombreMazo = "Mi mazo";
    public List<EntradaMazo> entradas = new List<EntradaMazo>();

    // Cuenta todas las cartas del mazo
    public int ContarCartas()
    {
        int total = 0;
        for (int i = 0; i < entradas.Count; i++)
        {
            if (entradas[i].carta != null)
            {
                total = total + entradas[i].cantidad;
            }
        }
        return total;
    }

    // Cuántas copias de una carta hay en el mazo
    public int ContarCopias(CardData carta)
    {
        for (int i = 0; i < entradas.Count; i++)
        {
            if (entradas[i].carta == carta)
            {
                return entradas[i].cantidad;
            }
        }
        return 0;
    }

    // Agrega 1 copia de una carta
    public void AgregarCarta(CardData carta)
    {
        for (int i = 0; i < entradas.Count; i++)
        {
            if (entradas[i].carta == carta)
            {
                entradas[i].cantidad = entradas[i].cantidad + 1;
                return;
            }
        }
        // No estaba: creo una entrada nueva
        EntradaMazo nueva = new EntradaMazo();
        nueva.carta = carta;
        nueva.cantidad = 1;
        entradas.Add(nueva);
    }

    // Quita 1 copia de una carta
    public void QuitarCarta(CardData carta)
    {
        for (int i = 0; i < entradas.Count; i++)
        {
            if (entradas[i].carta == carta)
            {
                entradas[i].cantidad = entradas[i].cantidad - 1;
                if (entradas[i].cantidad <= 0)
                {
                    entradas.RemoveAt(i);
                }
                return;
            }
        }
    }

    public void Vaciar()
    {
        entradas.Clear();
    }

    // Convierte las entradas en una lista "plana" de cartas
    // (20x Montaña -> 20 elementos). Esto es lo que se baraja al jugar.
    public List<CardData> ObtenerListaDeCartas()
    {
        List<CardData> lista = new List<CardData>();
        for (int i = 0; i < entradas.Count; i++)
        {
            for (int j = 0; j < entradas[i].cantidad; j++)
            {
                if (entradas[i].carta != null)
                {
                    lista.Add(entradas[i].carta);
                }
            }
        }
        return lista;
    }
}
