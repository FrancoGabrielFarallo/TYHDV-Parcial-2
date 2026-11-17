using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Pantalla final compartida: la misma sirve para "Perdiste" y "Ganaste".
// Va en el CANVAS del gameplay (no en el panel, porque el panel arranca apagado).
// Reemplaza al viejo DefeatUI.
public class EndGameUI : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject panelFinal;   // el panel que se muestra al terminar
    public TMP_Text textoTitulo;    // el texto que dice "Perdiste" / "Ganaste"
    public Button botonMenu;        // el boton que vuelve al menu principal

    [Header("Textos")]
    public string textoDerrota = "Perdiste";
    public string textoVictoria = "Ganaste";

    [Header("Escenas")]
    public string escenaMenu = "MainMenuV1";

    // Lo consulta el menu de opciones para no abrirse con ESC si ya termino el juego
    public static bool juegoTerminado = false;

    void Start()
    {
        Time.timeScale = 1f;
        juegoTerminado = false;

        if (panelFinal != null)
        {
            panelFinal.SetActive(false);
        }

        // Conectamos el boton por codigo (no hace falta configurar el OnClick)
        if (botonMenu != null)
        {
            botonMenu.onClick.AddListener(VolverAlMenu);
        }
    }

    public void MostrarDerrota()
    {
        Mostrar(textoDerrota);
    }

    public void MostrarVictoria()
    {
        Mostrar(textoVictoria);
    }

    void Mostrar(string titulo)
    {
        if (juegoTerminado) return; // por si se toca la victoria y la lava al mismo tiempo
        juegoTerminado = true;

        if (textoTitulo != null)
        {
            textoTitulo.text = titulo;
        }

        if (panelFinal != null)
        {
            panelFinal.SetActive(true);
        }

        // El jugador deja de responder
        PlayerController jugador = FindFirstObjectByType<PlayerController>();
        if (jugador != null)
        {
            jugador.enabled = false;
        }

        Time.timeScale = 0f; // pausa el juego
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void VolverAlMenu()
    {
        Time.timeScale = 1f;      // IMPORTANTE: si no, el menu arranca pausado
        juegoTerminado = false;
        SceneManager.LoadScene(escenaMenu);
    }
}
