using UnityEngine;

// =====================================================================
//  FieldZone.cs  -  Marca un panel como "el campo de un jugador".
//  Ponelo en el panel del campo (el que contiene las filas de
//  tierras y criaturas). Ese panel TIENE que tener un componente Image
//  (aunque sea transparente) para que detecte el mouse al soltar.
// =====================================================================
public class FieldZone : MonoBehaviour
{
    public Jugador duenio;
}
