using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// =====================================================================
//  DeckBuilderManager.cs  -  Escena donde armás tu mazo de 60 cartas.
//
//  - Muestra todas las cartas de la CardCollection.
//  - Clic IZQUIERDO en una carta = agregar 1 copia al mazo.
//  - Clic DERECHO en una carta  = quitar 1 copia.
//  - Las elecciones se guardan en el ScriptableObject DeckData del
//    jugador, y la escena de juego lee ese mismo SO.
// =====================================================================
public class DeckBuilderManager : MonoBehaviour
{
    public static DeckBuilderManager instancia;

    [Header("ScriptableObjects")]
    public CardCollection coleccion;
    public V2PlayerData jugador;      // se usa jugador.mazo
    public GameRules reglas;

    [Header("Prefabs")]
    public CardView prefabCarta;

    [Header("UI")]
    public Transform contenedorColeccion;   // un panel con GridLayoutGroup
    public TMP_Text textoListaMazo;
    public TMP_Text textoContador;
    public TMP_Text textoMensaje;
    public Button botonJugar;
    public string escenaJuego = "Juego";

    private List<CardView> vistas = new List<CardView>();

    void Awake()
    {
        instancia = this;
    }

    void OnDestroy()
    {
        if (instancia == this) instancia = null;
    }

    void Start()
    {
        // Creamos una carta en pantalla por cada carta de la colección
        for (int i = 0; i < coleccion.todasLasCartas.Count; i++)
        {
            CardData datos = coleccion.todasLasCartas[i];
            if (datos == null) continue;

            CardView vista = Instantiate(prefabCarta, contenedorColeccion);
            vista.Configurar(datos, null, false);
            vista.zona = ZonaCarta.Coleccion;

            // Le agregamos el script que detecta los clics del Deck Builder
            DeckBuilderCard boton = vista.gameObject.AddComponent<DeckBuilderCard>();
            boton.manager = this;
            boton.vista = vista;

            vistas.Add(vista);
        }

        Mensaje("Clic izquierdo: agregar. Clic derecho: quitar.");
        Actualizar();
    }

    // =================================================================
    //  AGREGAR / QUITAR
    // =================================================================
    public void AgregarCarta(CardData carta)
    {
        DeckData mazo = jugador.mazo;

        if (mazo.ContarCartas() >= reglas.cartasEnElMazo)
        {
            Mensaje("El mazo ya tiene " + reglas.cartasEnElMazo + " cartas.");
            return;
        }

        bool esTierra = carta.ObtenerTipo() == TipoCarta.Tierra;
        if (!esTierra && mazo.ContarCopias(carta) >= reglas.maximoCopiasPorCarta)
        {
            Mensaje("Máximo " + reglas.maximoCopiasPorCarta + " copias de " + carta.nombre + ".");
            return;
        }

        mazo.AgregarCarta(carta);
        Mensaje("Agregaste " + carta.nombre + ".");
        Actualizar();
    }

    public void QuitarCarta(CardData carta)
    {
        if (jugador.mazo.ContarCopias(carta) == 0) return;
        jugador.mazo.QuitarCarta(carta);
        Mensaje("Quitaste " + carta.nombre + ".");
        Actualizar();
    }

    // =================================================================
    //  BOTONES
    // =================================================================
    public void BotonVaciar()
    {
        jugador.mazo.Vaciar();
        Mensaje("Mazo vacío.");
        Actualizar();
    }

    // Completa hasta 60 con tierras de los colores que usa el mazo
    public void BotonRellenarConTierras()
    {
        DeckData mazo = jugador.mazo;

        // 1) Contamos cuántos símbolos de cada color piden las cartas del mazo
        int[] simbolos = new int[5];
        int totalSimbolos = 0;
        for (int i = 0; i < mazo.entradas.Count; i++)
        {
            CardData carta = mazo.entradas[i].carta;
            if (carta == null || carta.ObtenerTipo() == TipoCarta.Tierra) continue;
            for (int c = 0; c < 5; c++)
            {
                int cantidad = carta.ObtenerCostoDeColor((ColorMana)c) * mazo.entradas[i].cantidad;
                simbolos[c] = simbolos[c] + cantidad;
                totalSimbolos = totalSimbolos + cantidad;
            }
        }

        // Si no hay ningún color, usamos los 5 por igual
        if (totalSimbolos == 0)
        {
            for (int c = 0; c < 5; c++) simbolos[c] = 1;
        }

        // 2) Agregamos de a una tierra, siempre del color que "más le falta"
        int seguridad = 0;
        while (mazo.ContarCartas() < reglas.cartasEnElMazo && seguridad < 200)
        {
            seguridad++;
            int mejorColor = -1;
            float mejorFalta = -1f;

            for (int c = 0; c < 5; c++)
            {
                if (simbolos[c] == 0) continue;
                LandCardData tierra = coleccion.BuscarTierraDeColor((ColorMana)c);
                if (tierra == null) continue;

                // Cuantas menos tierras tenga en proporción a los símbolos, más falta
                float falta = simbolos[c] / (mazo.ContarCopias(tierra) + 1f);
                if (falta > mejorFalta)
                {
                    mejorFalta = falta;
                    mejorColor = c;
                }
            }

            if (mejorColor == -1)
            {
                Mensaje("No hay tierras en la colección para esos colores.");
                break;
            }
            mazo.AgregarCarta(coleccion.BuscarTierraDeColor((ColorMana)mejorColor));
        }

        Mensaje("Mazo completado con tierras.");
        Actualizar();
    }

    public void BotonJugar()
    {
        if (jugador.mazo.ContarCartas() != reglas.cartasEnElMazo)
        {
            Mensaje("El mazo tiene que tener exactamente " + reglas.cartasEnElMazo + " cartas.");
            return;
        }

#if UNITY_EDITOR
        // En el editor, marcamos el SO como modificado para que se guarde en el disco
        UnityEditor.EditorUtility.SetDirty(jugador.mazo);
#endif
        SceneManager.LoadScene(escenaJuego);
    }

    // =================================================================
    //  UI
    // =================================================================
    void Actualizar()
    {
        DeckData mazo = jugador.mazo;
        int total = mazo.ContarCartas();

        if (textoContador != null) textoContador.text = total + " / " + reglas.cartasEnElMazo;
        if (botonJugar != null) botonJugar.interactable = total == reglas.cartasEnElMazo;

        // Lista del mazo ordenada por tipo
        string lista = "";
        lista = lista + ArmarListaDeTipo(TipoCarta.Criatura, "CRIATURAS");
        lista = lista + ArmarListaDeTipo(TipoCarta.Hechizo, "HECHIZOS");
        lista = lista + ArmarListaDeTipo(TipoCarta.Tierra, "TIERRAS");
        if (textoListaMazo != null) textoListaMazo.text = lista;

        // En cada carta de la colección mostramos cuántas hay en el mazo
        for (int i = 0; i < vistas.Count; i++)
        {
            int copias = mazo.ContarCopias(vistas[i].datos);
            if (copias > 0) vistas[i].SetEstado("En mazo: " + copias);
            else vistas[i].SetEstado("");
        }
    }

    string ArmarListaDeTipo(TipoCarta tipo, string titulo)
    {
        DeckData mazo = jugador.mazo;
        string texto = "";
        int cantidadDelTipo = 0;

        for (int i = 0; i < mazo.entradas.Count; i++)
        {
            EntradaMazo entrada = mazo.entradas[i];
            if (entrada.carta == null || entrada.carta.ObtenerTipo() != tipo) continue;
            texto = texto + entrada.cantidad + "x " + entrada.carta.nombre + "\n";
            cantidadDelTipo = cantidadDelTipo + entrada.cantidad;
        }

        if (cantidadDelTipo == 0) return "";
        return "<b>" + titulo + " (" + cantidadDelTipo + ")</b>\n" + texto + "\n";
    }

    void Mensaje(string texto)
    {
        if (textoMensaje != null) textoMensaje.text = texto;
    }
}
