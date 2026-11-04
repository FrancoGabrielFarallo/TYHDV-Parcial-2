// =====================================================================
//  Ataque.cs  -  Guarda UN ataque declarado en la fase de combate.
//  Es una clase común (no MonoBehaviour): solo guarda datos.
// =====================================================================
[System.Serializable]
public class Ataque
{
    public CardView atacante;     // mi criatura que ataca
    public CardView objetivo;     // criatura rival atacada (null = ataca al JUGADOR)
    public CardView bloqueador;   // criatura rival que defiende (null = nadie)
}
