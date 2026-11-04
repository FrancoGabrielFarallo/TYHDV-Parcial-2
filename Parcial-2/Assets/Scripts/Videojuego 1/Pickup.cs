using UnityEngine;

// Va en el objeto recogible (moneda, pocion, etc.).
// El objeto necesita un Collider; el script lo marca como Trigger solo.
public class Pickup : MonoBehaviour
{
    [Header("Datos (ScriptableObject)")]
    public PickupData datos;

    void Start()
    {
        if (datos == null)
        {
            Debug.LogWarning("Pickup '" + name + "': falta asignar el PickupData.");
        }

        // 1. Si hay colliders en los hijos (el modelo), los volvemos Trigger
        //    para que el jugador no se choque con ellos.
        Collider[] collidersHijos = GetComponentsInChildren<Collider>();
        for (int i = 0; i < collidersHijos.Length; i++)
        {
            collidersHijos[i].isTrigger = true;
        }

        // 2. El collider que detecta al jugador TIENE que estar en este mismo objeto,
        //    porque OnTriggerEnter solo llega al objeto que tiene el collider.
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            CrearColliderAutomatico();
        }
        else
        {
            col.isTrigger = true;
        }
    }

    // Crea una esfera Trigger del tamaño del modelo (el Renderer de los hijos)
    void CrearColliderAutomatico()
    {
        SphereCollider esfera = gameObject.AddComponent<SphereCollider>();
        esfera.isTrigger = true;

        Renderer modelo = GetComponentInChildren<Renderer>();
        if (modelo != null)
        {
            // El tamaño del modelo esta en coordenadas del mundo;
            // lo pasamos a coordenadas locales dividiendo por la escala.
            Vector3 medidas = modelo.bounds.extents;
            float radioMundo = Mathf.Max(medidas.x, medidas.y, medidas.z);

            Vector3 escala = transform.lossyScale;
            float escalaMayor = Mathf.Max(Mathf.Abs(escala.x), Mathf.Abs(escala.y), Mathf.Abs(escala.z));

            esfera.radius = radioMundo / escalaMayor;
            esfera.center = transform.InverseTransformPoint(modelo.bounds.center);
        }
    }

    void OnTriggerEnter(Collider otro)
    {
        if (datos == null) return;

        // El collider que entra es la Capsule; el PlayerController esta en su padre
        PlayerController jugador = otro.GetComponentInParent<PlayerController>();
        if (jugador == null) return; // no es el jugador

        switch (datos.tipo)
        {
            case TipoRecogible.Coleccionable:
                jugador.AgregarColeccionable(datos.puntos);
                break;

            case TipoRecogible.AumentoVelocidad:
                jugador.AumentarVelocidad(datos.multiplicador, datos.duracion);
                break;

            case TipoRecogible.AumentoSalto:
                jugador.AumentarSalto(datos.multiplicador, datos.duracion);
                break;
        }

        Destroy(gameObject); // el objeto desaparece al agarrarlo
    }
}