using UnityEngine;
using UnityEngine.InputSystem;

// Va en la Main Camera, que es hija del "Player".
// Si "objetivo" queda vacio, usa al padre (el Player).
// La camara orbita alrededor del jugador cuando el mouse toca los bordes de la pantalla.
public class CameraController : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform objetivo;          // arrastrar el "Player"
    public float alturaObjetivo = 1f;   // a que altura del Player mira la camara
    public float distancia = 6f;        // distancia de la camara al jugador

    [Header("Rotacion con bordes de pantalla")]
    public float margenBorde = 25f;         // en pixeles
    public float velocidadRotacion = 120f;  // grados por segundo
    public bool rotarVertical = true;       // bordes de arriba/abajo
    public float anguloMinimo = -10f;
    public float anguloMaximo = 60f;

    private float anguloHorizontal = 0f; // giro alrededor del jugador
    private float anguloVertical = 20f;  // inclinacion

    void Start()
    {
        // Si el objetivo es la propia camara (o un hijo de ella), la camara se
        // persigue a si misma y se aleja sin parar. Lo descartamos.
        if (objetivo != null && objetivo.IsChildOf(transform))
        {
            Debug.LogWarning("CameraController: el Objetivo era la propia camara. Se busca al Player automaticamente.");
            objetivo = null;
        }

        // Buscamos al Player: el padre que tenga el PlayerController
        if (objetivo == null)
        {
            PlayerController jugador = GetComponentInParent<PlayerController>();
            if (jugador == null)
            {
                jugador = FindFirstObjectByType<PlayerController>();
            }
            if (jugador != null)
            {
                objetivo = jugador.transform;
            }
        }

        Debug.Log("CameraController: siguiendo a " + (objetivo != null ? objetivo.name : "NADA"));

        if (objetivo != null)
        {
            anguloHorizontal = objetivo.eulerAngles.y;
        }
    }

    // LateUpdate: se ejecuta despues de que el Player se movio, asi la camara no tiembla
    void LateUpdate()
    {
        if (objetivo == null) return;

        RotarConBordes();
        Posicionar();
    }

    void RotarConBordes()
    {
        if (Mouse.current == null) return;

        Vector2 mouse = Mouse.current.position.ReadValue();

        // Si el mouse esta fuera de la ventana no rotamos
        if (mouse.x < 0 || mouse.y < 0 || mouse.x > Screen.width || mouse.y > Screen.height)
        {
            return;
        }

        float giro = velocidadRotacion * Time.deltaTime;

        // Bordes izquierdo y derecho
        if (mouse.x <= margenBorde)
        {
            anguloHorizontal -= giro;
        }
        else if (mouse.x >= Screen.width - margenBorde)
        {
            anguloHorizontal += giro;
        }

        // Bordes de arriba y abajo
        if (rotarVertical)
        {
            if (mouse.y >= Screen.height - margenBorde)
            {
                anguloVertical -= giro; // mouse arriba = mirar hacia arriba
            }
            else if (mouse.y <= margenBorde)
            {
                anguloVertical += giro; // mouse abajo = mirar hacia abajo
            }

            anguloVertical = Mathf.Clamp(anguloVertical, anguloMinimo, anguloMaximo);
        }
    }

    void Posicionar()
    {
        Quaternion rotacion = Quaternion.Euler(anguloVertical, anguloHorizontal, 0f);
        Vector3 centro = objetivo.position + Vector3.up * alturaObjetivo;

        // La camara se ubica "atras" del centro segun la rotacion
        transform.position = centro - rotacion * Vector3.forward * distancia;
        transform.LookAt(centro);
    }
}