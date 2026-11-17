using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Va en el Canvas de la escena "MainMenuV1".
public class MainMenu : MonoBehaviour
{
    [Header("Botones")]
    public Button botonJugar;
    public Button botonOpciones;
    public Button botonSalir;

    [Header("Opciones")]
    public SettingsMenu menuOpciones; // la instancia del prefab SettingsV1 (si queda vacio, la busca)

    [Header("Escenas")]
    public string escenaJuego = "V1MainScene";

    void Start()
    {
        Time.timeScale = 1f; // por si venimos de una partida pausada

        if (menuOpciones == null)
        {
            menuOpciones = FindFirstObjectByType<SettingsMenu>();
        }

        // Conectamos los botones por codigo
        if (botonJugar != null) botonJugar.onClick.AddListener(Jugar);
        if (botonOpciones != null) botonOpciones.onClick.AddListener(AbrirOpciones);
        if (botonSalir != null) botonSalir.onClick.AddListener(Salir);
    }

    public void Jugar()
    {
        SceneManager.LoadScene(escenaJuego);
    }

    public void AbrirOpciones()
    {
        if (menuOpciones != null)
        {
            menuOpciones.Abrir();
        }
        else
        {
            Debug.LogWarning("MainMenu: no hay un SettingsV1 en la escena.");
        }
    }

    public void Salir()
    {
        // En el .exe cierra el juego. En el editor no hace nada,
        // asi que ademas cortamos el Play para poder probarlo.
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
