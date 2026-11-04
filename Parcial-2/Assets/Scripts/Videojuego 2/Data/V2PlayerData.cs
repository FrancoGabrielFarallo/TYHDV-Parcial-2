using UnityEngine;

// =====================================================================
//  PlayerData.cs  -  Datos de un jugador (humano o IA)
//  Click derecho en Project > Create > Cartas > Datos de jugador
//
//  Creá uno para vos (con el mazo que armás en el Deck Builder)
//  y otro para la IA (con un mazo armado a mano).
// =====================================================================
[CreateAssetMenu(fileName = "Jugador_", menuName = "Cartas/Datos de jugador")]
public class V2PlayerData : ScriptableObject
{
    public string nombreJugador = "Jugador";
    public Sprite retrato;
    public DeckData mazo;
}
