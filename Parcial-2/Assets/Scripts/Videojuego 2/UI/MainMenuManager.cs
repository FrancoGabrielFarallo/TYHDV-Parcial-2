using UnityEngine;

// =====================================================================
//  MainMenuManager.cs  -  Va en la escena "MainMenuV2".
//  Tiene una función para cada botón del menú:
//    BotonJugar    -> va al Deck Builder
//    BotonOpciones -> abre el popup SettingsV2
//    BotonSalir    -> cierra el juego (en el editor, detiene el Play)
// =====================================================================
public class MainMenuManager : MonoBehaviour
{
    [Header("Escenas (el nombre tiene que ser EXACTO)")]
    public string escenaDeckBuilder = "DeckBuilder";

    [Header("Popup de opciones (la instancia de SettingsV2 en esta escena)")]
    public V2SettingsMenu menuOpciones;

    void Start()
    {
        // Por si volvimos al menú con el juego pausado
        Time.timeScale = 1f;
    }

    public void BotonJugar()
    {
        CargadorDeEscenas.Cargar(escenaDeckBuilder);
    }

    public void BotonOpciones()
    {
        if (menuOpciones == null)
        {
            Debug.LogError("MainMenuManager: falta asignar 'Menu Opciones' (el SettingsV2 de la escena).");
            return;
        }
        menuOpciones.Abrir();
    }

    public void BotonSalir()
    {
#if UNITY_EDITOR
        // Application.Quit no hace nada en el editor, así que paramos el Play
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
