using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// =====================================================================
//  SettingsMenu.cs  -  Va en la RAÍZ del prefab "SettingsV2".
//
//  - Se abre/cierra con ESC o con los botones.
//  - Si se abre durante una partida (existe un GameManager), pausa el
//    juego con Time.timeScale = 0. Al cerrarlo lo despausa.
//  - Slider de volumen: cambia AudioListener.volume (todo el sonido).
//  - Toggle de pantalla completa / ventana.
//  - El volumen se guarda con PlayerPrefs, así se recuerda al volver
//    a abrir el juego.
//
//  IMPORTANTE: este script tiene que estar en un objeto que SIEMPRE
//  esté activo (la raíz del prefab). Lo que se prende y apaga es el
//  hijo "panel". Si apagás el objeto que tiene el script, deja de
//  escuchar la tecla ESC.
// =====================================================================
public class V2SettingsMenu : MonoBehaviour
{
    [Header("Referencias del prefab")]
    [Tooltip("El hijo que se muestra/oculta (fondo oscuro + ventana)")]
    public GameObject panel;
    public Slider sliderVolumen;
    public Toggle togglePantallaCompleta;
    public Button botonVolver;

    [Header("Opciones")]
    public bool abrirConEscape = true;
    [Tooltip("Tamaño de la ventana cuando se sale de pantalla completa")]
    public int anchoVentana = 1280;
    public int altoVentana = 720;

    private bool abierto = false;
    private bool pauseElJuego = false;   // para despausar solo si fui yo el que pausó

    const string CLAVE_VOLUMEN = "Volumen";

    // =================================================================
    //  INICIO
    // =================================================================
    void Start()
    {
        // Cargamos el volumen guardado (1 = 100% si nunca se guardó)
        float volumen = PlayerPrefs.GetFloat(CLAVE_VOLUMEN, 1f);
        AudioListener.volume = volumen;

        if (sliderVolumen != null)
        {
            sliderVolumen.minValue = 0f;
            sliderVolumen.maxValue = 1f;
            // "WithoutNotify" = cambia el valor sin disparar el evento
            sliderVolumen.SetValueWithoutNotify(volumen);
            sliderVolumen.onValueChanged.AddListener(CambiarVolumen);
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

        // Arranca cerrado
        if (panel != null) panel.SetActive(false);
        abierto = false;
    }

    void Update()
    {
        // Update sigue funcionando aunque Time.timeScale sea 0
        if (abrirConEscape && EscapePresionado())
        {
            if (abierto) Cerrar();
            else Abrir();
        }
    }

    // Lee la tecla ESC con el sistema de input que tenga el proyecto.
    // Unity 6 usa por defecto el "Input System" nuevo; si tu proyecto usa
    // el viejo, se usa Input.GetKeyDown. Unity elige solo cuál compilar.
    bool EscapePresionado()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }

    // =================================================================
    //  ABRIR / CERRAR
    // =================================================================
    public void Abrir()
    {
        if (abierto) return;
        abierto = true;
        if (panel != null) panel.SetActive(true);

        // Actualizo el toggle por si la pantalla cambió con Alt+Enter
        if (togglePantallaCompleta != null) togglePantallaCompleta.SetIsOnWithoutNotify(Screen.fullScreen);

        // Si estamos en una partida, pausamos
        if (EstamosEnPartida())
        {
            Time.timeScale = 0f;
            pauseElJuego = true;
        }
    }

    public void Cerrar()
    {
        if (!abierto) return;
        abierto = false;
        if (panel != null) panel.SetActive(false);

        PlayerPrefs.Save();

        if (pauseElJuego)
        {
            Time.timeScale = 1f;
            pauseElJuego = false;
        }
    }

    public bool EstaAbierto()
    {
        return abierto;
    }

    // Hay partida si en la escena existe un GameManager
    bool EstamosEnPartida()
    {
        return GameManager.instancia != null;
    }

    void OnDestroy()
    {
        // Si cambiamos de escena con el juego pausado, no lo dejamos trabado
        if (pauseElJuego) Time.timeScale = 1f;
    }

    // =================================================================
    //  OPCIONES
    // =================================================================
    public void CambiarVolumen(float valor)
    {
        AudioListener.volume = valor;
        PlayerPrefs.SetFloat(CLAVE_VOLUMEN, valor);
    }

    // OJO: en el editor de Unity esto no hace nada visible.
    // Se prueba en el juego compilado (.exe).
    public void CambiarPantallaCompleta(bool completa)
    {
        if (completa)
        {
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.FullScreenWindow);
        }
        else
        {
            Screen.SetResolution(anchoVentana, altoVentana, FullScreenMode.Windowed);
        }
    }
}
