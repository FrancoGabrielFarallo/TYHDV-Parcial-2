using UnityEngine;

// Va en el cubo que sube. Necesita un Collider (el script lo marca como Trigger).
public class RisingHazard : MonoBehaviour
{
    [Header("Datos (ScriptableObject)")]
    public HazardData datos;

    [Header("Referencias")]
    public DefeatUI uiDerrota; // si queda vacio, la busca en la escena

    private PlayerController jugador;
    private Collider col;
    private bool subiendo = false;
    private bool yaPerdio = false;

    void Start()
    {
        jugador = FindFirstObjectByType<PlayerController>();

        if (uiDerrota == null)
        {
            uiDerrota = FindFirstObjectByType<DefeatUI>();
        }

        col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true; // el jugador lo "toca" sin chocarlo
        }

        if (datos == null) Debug.LogWarning("RisingHazard: falta asignar el HazardData.");
        if (jugador == null) Debug.LogWarning("RisingHazard: no se encontro el Player.");
        if (uiDerrota == null) Debug.LogWarning("RisingHazard: no se encontro la DefeatUI.");
        if (col == null) Debug.LogWarning("RisingHazard: el cubo necesita un Collider.");
    }

    void Update()
    {
        if (datos == null || jugador == null) return;

        // 1. Esperar a que el jugador SALGA del area (se aleje mas que la distancia)
        if (!subiendo)
        {
            if (DistanciaAlJugador() > datos.distanciaActivacion)
            {
                subiendo = true;
                Debug.Log("El cubo empezo a subir");
            }
            return;
        }

        // 2. Subir (si tiene limite, frenar al llegar)
        if (datos.tieneLimite && transform.position.y >= datos.alturaMaxima)
        {
            return;
        }

        transform.position += Vector3.up * datos.velocidadSubida * Time.deltaTime;
    }

    // Distancia desde la SUPERFICIE del cubo hasta el jugador.
    // (Si usaramos el centro, un cubo gigante nunca se activaria.)
    float DistanciaAlJugador()
    {
        Vector3 posJugador = jugador.transform.position;

        if (col != null)
        {
            Vector3 puntoMasCercano = col.ClosestPoint(posJugador);
            return Vector3.Distance(puntoMasCercano, posJugador);
        }

        return Vector3.Distance(transform.position, posJugador);
    }

    void OnTriggerEnter(Collider otro)
    {
        if (yaPerdio) return;

        PlayerController pc = otro.GetComponentInParent<PlayerController>();
        if (pc == null) return; // no es el jugador

        yaPerdio = true;
        pc.enabled = false; // el jugador deja de responder al teclado

        if (uiDerrota != null)
        {
            uiDerrota.Mostrar();
        }
    }

    // Dibuja la zona de activacion (aproximada) seleccionando el cubo
    void OnDrawGizmosSelected()
    {
        if (datos == null) return;

        Collider c = GetComponent<Collider>();
        if (c == null) return;

        Gizmos.color = Color.yellow;
        Bounds zona = c.bounds;
        zona.Expand(datos.distanciaActivacion * 2f);
        Gizmos.DrawWireCube(zona.center, zona.size);
    }
}