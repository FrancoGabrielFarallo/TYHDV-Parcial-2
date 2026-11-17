using UnityEngine;
using UnityEngine.SceneManagement;

// =====================================================================
//  CargadorDeEscenas.cs
//  Una sola función para cambiar de escena desde cualquier script:
//      CargadorDeEscenas.Cargar("MainMenuV2");
//
//  Antes de cargar revisa que la escena exista en
//  File > Build Profiles > Scene List. Si el nombre está mal escrito,
//  en vez de un error raro te muestra un mensaje claro en la Console.
//
//  Es una clase "static": no se pone en ningún GameObject,
//  se usa directamente escribiendo su nombre.
// =====================================================================
public static class CargadorDeEscenas
{
    public static void Cargar(string nombreEscena)
    {
        if (!Application.CanStreamedLevelBeLoaded(nombreEscena))
        {
            Debug.LogError("No se puede cargar la escena '" + nombreEscena + "'. "
                + "Revisá que el nombre sea EXACTO (mayúsculas incluidas) y que esté agregada en "
                + "File > Build Profiles > Scene List.");
            return;
        }

        // Si veníamos con el juego pausado, lo despausamos
        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreEscena);
    }
}
