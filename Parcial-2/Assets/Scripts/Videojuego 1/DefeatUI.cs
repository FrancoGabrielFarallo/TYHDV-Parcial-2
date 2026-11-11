using UnityEngine;
using UnityEngine.SceneManagement;

// Va en el CANVAS (no en el panel, porque el panel arranca apagado
// y un objeto apagado no ejecuta sus scripts ni se puede encontrar).
public class DefeatUI : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject panelDerrota; // el panel con el texto y el boton

    void Start()
    {
        Time.timeScale = 1f; // por si veniamos de una derrota

        if (panelDerrota != null)
        {
            panelDerrota.SetActive(false);
        }
        else
        {
            Debug.LogWarning("DefeatUI: falta asignar el Panel Derrota.");
        }
    }

    // La llama el RisingHazard cuando el jugador lo toca
    public void Mostrar()
    {
        if (panelDerrota != null)
        {
            panelDerrota.SetActive(true);
        }

        Time.timeScale = 0f; // pausa el juego (fisica, cubo, timers)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Se conecta al OnClick del boton "Reintentar"
    public void Reintentar()
    {
        Time.timeScale = 1f; // IMPORTANTE: si no, la escena nueva arranca pausada
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
