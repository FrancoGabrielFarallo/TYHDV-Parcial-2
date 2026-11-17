using UnityEngine;

// Va en un Empty Object. Al agregar el script se agrega solo un Box Collider.
// Cuando el jugador entra en la caja, se muestra la pantalla de victoria.
[RequireComponent(typeof(BoxCollider))]
public class VictoryZone : MonoBehaviour
{
    [Header("Referencias")]
    public EndGameUI pantallaFinal; // si queda vacio, la busca en la escena

    void Start()
    {
        GetComponent<BoxCollider>().isTrigger = true;

        if (pantallaFinal == null)
        {
            pantallaFinal = FindFirstObjectByType<EndGameUI>();
        }

        if (pantallaFinal == null)
        {
            Debug.LogWarning("VictoryZone: no se encontro la EndGameUI en la escena.");
        }
    }

    void OnTriggerEnter(Collider otro)
    {
        PlayerController jugador = otro.GetComponentInParent<PlayerController>();
        if (jugador == null) return; // no es el jugador

        if (pantallaFinal != null)
        {
            pantallaFinal.MostrarVictoria();
        }
    }

    // Dibuja la caja en verde siempre (el Empty no se ve en la escena)
    void OnDrawGizmos()
    {
        BoxCollider caja = GetComponent<BoxCollider>();
        if (caja == null) return;

        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(caja.center, caja.size);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(caja.center, caja.size);
    }
}
