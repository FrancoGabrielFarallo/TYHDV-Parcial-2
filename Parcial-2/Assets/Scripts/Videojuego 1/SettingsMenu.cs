using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

// Va en la RAIZ del prefab "SettingsV1" (el Canvas), no en el panel.
// Funciona igual en el menu principal y en el gameplay:
// - Se abre/cierra con ESC o con los botones.
// - Si en la escena hay un Player, al abrirse pausa el juego.
// Los valores se guardan con PlayerPrefs, asi se mantienen entre escenas y entre partidas.
public class SettingsMenu : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject panelOpciones;       // el popup (arranca apagado)
    public Slider sliderVolumen;
    public Toggle togglePantallaCompleta;
    public Slider sliderSensibilidad;
    public Button botonVolver;

    [Header("Rango de sensibilidad")]
    public float sensibilidadMinima = 0.2f;
    public float sensibilidadMaxima = 3f;

    private bool abierto = false;
    private bool pausaElJuego = false;     // true si estamos en el gameplay
    private float tiempoAnterior = 1f;     // para volver a la velocidad que habia antes de pausar

    void Start()
    {
        // Si hay un Player en la escena, estamos en el gameplay
        pausaElJuego = FindFirstObjectByType<PlayerController>() != null;

        // Cargar valores guardados (o los de por defecto la primera vez)
        float volumen = PlayerPrefs.GetFloat("Volumen", 1f);
        float sensibilidad = PlayerPrefs.GetFloat("Sensibilidad", 1f);
        AudioListener.volume = volumen;

        // Configurar los controles por codigo (asi no hay que tocarlos en el Inspector)
        if (sliderVolumen != null)
        {
            sliderVolumen.minValue = 0f;
            sliderVolumen.maxValue = 1f;
            sliderVolumen.SetValueWithoutNotify(volumen);
            sliderVolumen.onValueChanged.AddListener(CambiarVolumen);
        }

        if (sliderSensibilidad != null)
        {
            sliderSensibilidad.minValue = sensibilidadMinima;
            sliderSensibilidad.maxValue = sensibilidadMaxima;
            sliderSensibilidad.SetValueWithoutNotify(sensibilidad);
            sliderSensibilidad.onValueChanged.AddListener(CambiarSensibilidad);
        }

        if (togglePantallaCompleta != null)
        {
            togglePantallaCompleta.SetIsOnWithoutNotify(Screen.fullScreen);
            togglePantallaCompleta.onValueChanged.AddListener(CambiarPantallaCompleta);
        }

        if (botonVolver != null)
        {
            botonVolver.onClick.AddListener(Cerrar);
        }

        if (panelOpciones != null)
        {
            panelOpciones.SetActive(false);
        }
    }

    void Update()
    {
        // Update sigue funcionando aunque el juego este pausado (timeScale = 0)
        if (Keyboard.current == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (abierto)
            {
                Cerrar();
            }
            else if (!EndGameUI.juegoTerminado) // no abrir encima de la pantalla final
            {
                Abrir();
            }
        }
    }

    public void Abrir()
    {
        if (abierto) return;
        abierto = true;

        if (panelOpciones != null)
        {
            panelOpciones.SetActive(true);
        }

        if (pausaElJuego)
        {
            tiempoAnterior = Time.timeScale;
            Time.timeScale = 0f;
        }
    }

    public void Cerrar()
    {
        if (!abierto) return;
        abierto = false;

        if (panelOpciones != null)
        {
            panelOpciones.SetActive(false);
        }

        if (pausaElJuego)
        {
            Time.timeScale = tiempoAnterior; // despausa
        }

        PlayerPrefs.Save(); // guardamos a disco al cerrar
    }

    void CambiarVolumen(float valor)
    {
        AudioListener.volume = valor; // volumen general de todo el juego (0 a 1)
        PlayerPrefs.SetFloat("Volumen", valor);
    }

    void CambiarSensibilidad(float valor)
    {
        // La lee el CameraController cada frame
        PlayerPrefs.SetFloat("Sensibilidad", valor);
    }

    void CambiarPantallaCompleta(bool pantallaCompleta)
    {
        // Solo se nota en el .exe; en el editor no cambia nada
        if (pantallaCompleta)
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }
    }
}
