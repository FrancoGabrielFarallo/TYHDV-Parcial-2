using UnityEngine;

// =====================================================================
//  CardData.cs  (ScriptableObject BASE de todas las cartas)
//
//  Esta clase tiene lo que comparten TODAS las cartas: nombre, dibujo,
//  descripción y costo de mana.
//  No se crea directamente: se crean sus "hijas" (Tierra, Criatura,
//  Hechizo), que heredan todo esto y agregan lo suyo.
//  Por eso es "abstract": Unity no te deja crear una CardData sola.
// =====================================================================
public abstract class CardData : ScriptableObject
{
    [Header("Datos generales")]
    public string nombre = "Nueva carta";
    [TextArea(2, 4)]
    public string descripcion = "";
    public Sprite arte;

    [Header("Costo de mana (cuánto mana de cada color necesita)")]
    public int costoRojo = 0;
    public int costoVerde = 0;
    public int costoBlanco = 0;
    public int costoNegro = 0;
    public int costoAzul = 0;
    [Tooltip("Mana de cualquier color")]
    public int costoGenerico = 0;

    // Cada hija dice qué tipo de carta es
    public abstract TipoCarta ObtenerTipo();

    // Devuelve el costo de UN color en particular
    public int ObtenerCostoDeColor(ColorMana color)
    {
        if (color == ColorMana.Rojo) return costoRojo;
        if (color == ColorMana.Verde) return costoVerde;
        if (color == ColorMana.Blanco) return costoBlanco;
        if (color == ColorMana.Negro) return costoNegro;
        if (color == ColorMana.Azul) return costoAzul;
        return 0;
    }

    // Suma de todo el costo (sirve para la IA y para mostrar)
    public int ObtenerCostoTotal()
    {
        return costoRojo + costoVerde + costoBlanco + costoNegro + costoAzul + costoGenerico;
    }

    // Arma un texto tipo "2 R R" para mostrar en la carta
    public string ObtenerTextoCosto()
    {
        string texto = "";
        if (costoGenerico > 0) texto = texto + costoGenerico + " ";
        for (int i = 0; i < costoRojo; i++) texto = texto + "R ";
        for (int i = 0; i < costoVerde; i++) texto = texto + "V ";
        for (int i = 0; i < costoBlanco; i++) texto = texto + "B ";
        for (int i = 0; i < costoNegro; i++) texto = texto + "N ";
        for (int i = 0; i < costoAzul; i++) texto = texto + "A ";
        return texto.Trim();
    }

    // El color "principal" de la carta, para pintar el fondo.
    // Si tiene costo de un solo color, devuelve ese. Si no tiene color devuelve -1.
    public virtual int ObtenerColorPrincipal()
    {
        if (costoRojo > 0) return (int)ColorMana.Rojo;
        if (costoVerde > 0) return (int)ColorMana.Verde;
        if (costoBlanco > 0) return (int)ColorMana.Blanco;
        if (costoNegro > 0) return (int)ColorMana.Negro;
        if (costoAzul > 0) return (int)ColorMana.Azul;
        return -1;
    }
}
