using UnityEngine;

// Va en el objeto de la plataforma.
// DefaultExecutionOrder(-10): se ejecuta ANTES que el PlayerController,
// asi el jugador ya sabe cuanto se movio la plataforma en este frame.
[DefaultExecutionOrder(-10)]
[RequireComponent(typeof(Rigidbody))]
public class MovingPlatform : MonoBehaviour
{
    [Header("Datos (ScriptableObject)")]
    public PlatformData datos;

    // Cuanto se movio la plataforma en este frame (lo usa el PlayerController)
    [HideInInspector] public Vector3 movimientoEsteFrame;

    private Rigidbody rb;
    private Vector3 posicionInicial;
    private float recorrido = 0f;  // ejes: posicion actual entre -distancia y +distancia
    private float angulo = 0f;     // circular: angulo actual en radianes
    private int direccion = 1;     // 1 = positivo, -1 = negativo

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;   // la movemos nosotros, la fisica no la empuja
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        posicionInicial = transform.position;

        if (datos == null)
        {
            Debug.LogWarning("MovingPlatform '" + name + "': falta asignar el PlatformData.");
            return;
        }

        if (datos.sentido == SentidoPlataforma.PositivoANegativo)
        {
            direccion = -1;
        }
    }

    void FixedUpdate()
    {
        if (datos == null)
        {
            movimientoEsteFrame = Vector3.zero;
            return;
        }

        Vector3 nuevaPosicion;

        if (datos.tipoMovimiento == TipoMovimientoPlataforma.Circular)
        {
            // Pasamos la velocidad (unidades/seg) a velocidad angular (radianes/seg)
            float velocidadAngular = datos.velocidad / Mathf.Max(datos.radio, 0.01f);
            angulo += direccion * velocidadAngular * Time.fixedDeltaTime;

            nuevaPosicion = posicionInicial + PuntoDelCirculo(angulo);
        }
        else
        {
            // Ida y vuelta: al llegar a un extremo se da vuelta
            recorrido += direccion * datos.velocidad * Time.fixedDeltaTime;

            if (recorrido >= datos.distancia)
            {
                recorrido = datos.distancia;
                direccion = -1;
            }
            else if (recorrido <= -datos.distancia)
            {
                recorrido = -datos.distancia;
                direccion = 1;
            }

            nuevaPosicion = posicionInicial + ObtenerEje() * recorrido;
        }

        movimientoEsteFrame = nuevaPosicion - rb.position;
        rb.MovePosition(nuevaPosicion);
    }

    Vector3 ObtenerEje()
    {
        switch (datos.tipoMovimiento)
        {
            case TipoMovimientoPlataforma.EjeY: return Vector3.up;
            case TipoMovimientoPlataforma.EjeZ: return Vector3.forward;
            default: return Vector3.right; // EjeX
        }
    }

    Vector3 PuntoDelCirculo(float a)
    {
        if (datos.circuloVertical)
        {
            return new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * datos.radio;
        }
        return new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * datos.radio;
    }

    // Dibuja el recorrido en la escena (seleccionando la plataforma)
    void OnDrawGizmosSelected()
    {
        if (datos == null) return;

        Vector3 centro = Application.isPlaying ? posicionInicial : transform.position;
        Gizmos.color = Color.cyan;

        if (datos.tipoMovimiento == TipoMovimientoPlataforma.Circular)
        {
            int segmentos = 32;
            for (int i = 0; i < segmentos; i++)
            {
                float a1 = (i / (float)segmentos) * Mathf.PI * 2f;
                float a2 = ((i + 1) / (float)segmentos) * Mathf.PI * 2f;
                Gizmos.DrawLine(centro + PuntoDelCirculo(a1), centro + PuntoDelCirculo(a2));
            }
            Gizmos.DrawSphere(centro, 0.1f);
        }
        else
        {
            Vector3 eje = ObtenerEje();
            Vector3 extremoA = centro - eje * datos.distancia;
            Vector3 extremoB = centro + eje * datos.distancia;
            Gizmos.DrawLine(extremoA, extremoB);
            Gizmos.DrawSphere(extremoA, 0.15f);
            Gizmos.DrawSphere(extremoB, 0.15f);
        }
    }
}