using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// =====================================================================
//  CardView.cs  -  Va en el PREFAB de la carta.
//
//  Hace 3 cosas:
//   1) MUESTRA los datos de un CardData (nombre, costo, ataque/vida...).
//   2) GUARDA el estado de esa carta en la partida (si está
//      deshabilitada, cuánto daño recibió, bonus, etc.).
//      Ojo: el estado va acá y NO en el ScriptableObject, porque el SO
//      es compartido por todas las copias de la carta.
//   3) Permite ARRASTRARLA (drag & drop) y hacerle CLIC.
// =====================================================================
public class CardView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("Referencias del prefab (arrastrar desde la jerarquía)")]
    public Image fondo;
    public Image imagenArte;
    public TMP_Text textoNombre;
    public TMP_Text textoCosto;
    public TMP_Text textoTipo;
    public TMP_Text textoDescripcion;
    public TMP_Text textoStats;
    public TMP_Text textoEstado;
    public GameObject dorso;          // imagen que tapa la carta (cartas de la IA en la mano)
    public Outline resaltado;         // borde amarillo cuando está seleccionada
    public CanvasGroup canvasGroup;   // para que no bloquee el mouse mientras se arrastra

    [Header("Estado en la partida (solo para mirar en Play)")]
    public CardData datos;
    public Jugador duenio;
    public ZonaCarta zona = ZonaCarta.Mano;
    public bool deshabilitada = false;
    public bool recienInvocada = false;

    // Valores de criatura
    public int ataqueBase = 0;
    public int vidaBase = 0;
    public int bonusAtaqueTemporal = 0;
    public int bonusVidaTemporal = 0;
    public int danioRecibido = 0;

    // Para el drag & drop
    private bool arrastrando = false;
    private Transform padreOriginal;
    private int indiceOriginal;

    // -----------------------------------------------------------------
    //  CONFIGURAR: se llama apenas se crea la carta
    // -----------------------------------------------------------------
    public void Configurar(CardData nuevosDatos, Jugador nuevoDuenio, bool mostrarDorso)
    {
        datos = nuevosDatos;
        duenio = nuevoDuenio;
        deshabilitada = false;
        recienInvocada = false;
        bonusAtaqueTemporal = 0;
        bonusVidaTemporal = 0;
        danioRecibido = 0;

        CreatureCardData criatura = datos as CreatureCardData;
        if (criatura != null)
        {
            ataqueBase = criatura.ataque;
            vidaBase = criatura.vida;
        }

        SetResaltado(false);
        SetEstado("");
        MostrarDorso(mostrarDorso);
        ActualizarVisual();
    }

    // -----------------------------------------------------------------
    //  VISUAL
    // -----------------------------------------------------------------
    public void ActualizarVisual()
    {
        if (datos == null) return;

        if (textoNombre != null) textoNombre.text = datos.nombre;
        if (textoCosto != null) textoCosto.text = datos.ObtenerTextoCosto();
        if (imagenArte != null)
        {
            imagenArte.sprite = datos.arte;
            imagenArte.enabled = datos.arte != null;
        }

        // Tipo y descripción según la clase de carta
        string tipo = "";
        string descripcion = datos.descripcion;
        string stats = "";

        LandCardData tierra = datos as LandCardData;
        CreatureCardData criatura = datos as CreatureCardData;
        SpellCardData hechizo = datos as SpellCardData;

        if (tierra != null)
        {
            tipo = "Tierra - " + tierra.colorQueProduce;
            if (descripcion == "") descripcion = "Produce 1 mana " + tierra.colorQueProduce + ".";
        }
        if (criatura != null)
        {
            tipo = "Criatura - " + criatura.movimiento;
            stats = ObtenerAtaque() + "/" + ObtenerVida();
        }
        if (hechizo != null)
        {
            tipo = "Hechizo";
            if (descripcion == "") descripcion = hechizo.ObtenerTextoEfecto();
        }

        if (textoTipo != null) textoTipo.text = tipo;
        if (textoDescripcion != null) textoDescripcion.text = descripcion;
        if (textoStats != null) textoStats.text = stats;

        // Color de fondo: color del mana. Si está deshabilitada, más oscura.
        if (fondo != null)
        {
            Color color = ObtenerColorDeFondo();
            if (deshabilitada)
            {
                color = color * 0.45f;
                color.a = 1f;
            }
            fondo.color = color;
        }
    }

    Color ObtenerColorDeFondo()
    {
        GameRules reglas = null;
        if (GameManager.instancia != null) reglas = GameManager.instancia.reglas;
        if (reglas == null && DeckBuilderManager.instancia != null) reglas = DeckBuilderManager.instancia.reglas;
        if (reglas == null) return Color.white;

        int indiceColor = datos.ObtenerColorPrincipal();
        if (indiceColor < 0 || indiceColor >= reglas.coloresMana.Length) return reglas.colorSinColor;
        return reglas.coloresMana[indiceColor];
    }

    public void MostrarDorso(bool mostrar)
    {
        if (dorso != null) dorso.SetActive(mostrar);
    }

    public void SetResaltado(bool activo)
    {
        if (resaltado != null) resaltado.enabled = activo;
    }

    public void SetEstado(string texto)
    {
        if (textoEstado != null) textoEstado.text = texto;
    }

    // -----------------------------------------------------------------
    //  ESTADO DE LA CARTA
    // -----------------------------------------------------------------
    public bool EsCriatura()
    {
        return datos != null && datos.ObtenerTipo() == TipoCarta.Criatura;
    }

    public bool EsVoladora()
    {
        CreatureCardData criatura = datos as CreatureCardData;
        return criatura != null && criatura.EsVoladora();
    }

    public int ObtenerAtaque()
    {
        return ataqueBase + bonusAtaqueTemporal;
    }

    public int ObtenerVida()
    {
        return vidaBase + bonusVidaTemporal - danioRecibido;
    }

    public bool EstaMuerta()
    {
        return EsCriatura() && ObtenerVida() <= 0;
    }

    public void Deshabilitar()
    {
        deshabilitada = true;
        ActualizarVisual();
    }

    public void Habilitar()
    {
        deshabilitada = false;
        recienInvocada = false;
        ActualizarVisual();
    }

    public void RecibirDanio(int cantidad)
    {
        danioRecibido = danioRecibido + cantidad;
        ActualizarVisual();
    }

    // +X/+X de un hechizo
    public void AplicarBonus(int ataque, int vida, bool temporal)
    {
        if (temporal)
        {
            bonusAtaqueTemporal = bonusAtaqueTemporal + ataque;
            bonusVidaTemporal = bonusVidaTemporal + vida;
        }
        else
        {
            ataqueBase = ataqueBase + ataque;
            vidaBase = vidaBase + vida;
        }
        ActualizarVisual();
    }

    // Se llama al final de CADA turno
    public void FinDeTurno(bool curar)
    {
        bonusAtaqueTemporal = 0;
        bonusVidaTemporal = 0;
        if (curar) danioRecibido = 0;
        SetEstado("");
        SetResaltado(false);
        ActualizarVisual();
    }

    // ¿Puede atacar? Tiene que estar en el campo, habilitada y sin mareo
    public bool PuedeAtacar()
    {
        return EsCriatura() && zona == ZonaCarta.Campo && !deshabilitada && !recienInvocada;
    }

    // ¿Puede bloquear a este atacante?
    // REGLA: una criatura de tierra NO puede bloquear a una voladora.
    public bool PuedeBloquear(CardView atacante)
    {
        if (!EsCriatura()) return false;
        if (zona != ZonaCarta.Campo) return false;
        if (deshabilitada) return false;
        if (atacante.EsVoladora() && !EsVoladora()) return false;
        return true;
    }

    // -----------------------------------------------------------------
    //  DRAG & DROP (arrastrar desde la mano al campo)
    // -----------------------------------------------------------------
    public void OnBeginDrag(PointerEventData eventData)
    {
        GameManager gm = GameManager.instancia;
        if (gm == null) return;   // en el Deck Builder no se arrastra

        if (!gm.PuedeArrastrar(this))
        {
            arrastrando = false;
            return;
        }

        arrastrando = true;
        padreOriginal = transform.parent;
        indiceOriginal = transform.GetSiblingIndex();

        // La movemos a una capa que está arriba de todo, para que se vea encima
        transform.SetParent(gm.capaArrastre, true);

        // Que el mouse "atraviese" la carta, así detectamos qué hay debajo
        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!arrastrando) return;
        transform.position = eventData.position;   // funciona con Canvas "Screen Space - Overlay"
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!arrastrando) return;
        arrastrando = false;
        if (canvasGroup != null) canvasGroup.blocksRaycasts = true;

        // ¿Qué objeto de la UI quedó debajo del mouse al soltar?
        GameObject destino = eventData.pointerCurrentRaycast.gameObject;

        bool seJugo = GameManager.instancia.IntentarJugarCarta(this, destino);
        if (!seJugo)
        {
            VolverALaMano();
        }
    }

    public void VolverALaMano()
    {
        transform.SetParent(padreOriginal, false);
        transform.SetSiblingIndex(indiceOriginal);
    }

    // -----------------------------------------------------------------
    //  CLIC (elegir atacantes, objetivos y defensores)
    // -----------------------------------------------------------------
    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameManager.instancia == null) return;
        if (eventData.dragging) return;  // si fue un arrastre, no cuenta como clic
        GameManager.instancia.CartaClickeada(this);
    }
}
