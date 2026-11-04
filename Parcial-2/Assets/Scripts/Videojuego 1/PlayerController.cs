using UnityEngine;
using UnityEngine.InputSystem; // Unity 6 usa el Input System nuevo por defecto

// Va en el objeto "Player" (el Empty Object padre), junto con el Rigidbody.
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Datos reales (ScriptableObject)")]
    public PlayerData datos;

    [Header("Valores de prueba (se cargan desde el SO al dar Play)")]
    // Estos los podes tocar en Play Mode para testear.
    // Al salir de Play vuelven solos; los valores definitivos se cambian en el SO.
    public float velocidad;
    public float velocidadGiro;
    public float alturaSalto;
    public float alturaDobleSalto;
    public float gravedad;

    [Header("Referencias")]
    public Transform camara;   // si queda vacio usa la Main Camera

    [Header("Suelo")]
    // Distancia desde el pivote del Player hasta la base de la capsula.
    // Si la capsula esta centrada en el Player (posicion 0,0,0 y altura 2) -> 1
    // Si la capsula esta subida para que el pivote quede en los pies -> 0
    public float alturaPies = 1f;
    public LayerMask capasSuelo = ~0; // por defecto: todas las capas

    private Rigidbody rb;
    private Vector3 direccionMovimiento; // se calcula en Update, se usa en FixedUpdate
    private bool quiereSaltar = false;
    private bool enSuelo = false;
    private bool puedeDobleSaltar = false;
    private MovingPlatform plataformaDebajo; // la plataforma movil que estamos pisando (si hay)

    [Header("Estado (solo para mirar)")]
    public int coleccionables = 0;

    // Aumentos temporales: multiplicador actual y cuanto tiempo le queda
    private float multiplicadorVelocidad = 1f;
    private float tiempoVelocidad = 0f;
    private float multiplicadorSalto = 1f;
    private float tiempoSalto = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Configuracion del Rigidbody por codigo (asi no hay que acordarse en el Inspector)
        rb.useGravity = false;          // usamos nuestra propia gravedad (la del SO)
        rb.freezeRotation = true;       // que la fisica no lo tumbe; lo giramos nosotros
        rb.interpolation = RigidbodyInterpolation.Interpolate; // camara mas suave

        CargarDatos();

        if (camara == null && Camera.main != null)
        {
            camara = Camera.main.transform;
        }
    }

    // Copia los datos del SO a las variables de prueba.
    // Tambien se puede llamar con click derecho en el componente > "Cargar datos del SO".
    [ContextMenu("Cargar datos del SO")]
    public void CargarDatos()
    {
        if (datos == null)
        {
            Debug.LogWarning("PlayerController: falta asignar el PlayerData (ScriptableObject).");
            return;
        }

        velocidad = datos.velocidad;
        velocidadGiro = datos.velocidadGiro;
        alturaSalto = datos.alturaSalto;
        alturaDobleSalto = datos.alturaDobleSalto;
        gravedad = datos.gravedad;
    }

    // ---------- UPDATE: solo leemos el teclado ----------
    void Update()
    {
        ActualizarAumentos();

        Keyboard teclado = Keyboard.current;
        if (teclado == null) return;

        // 1. Leer WASD
        float horizontal = 0f;
        float vertical = 0f;

        if (teclado.wKey.isPressed) vertical += 1f;
        if (teclado.sKey.isPressed) vertical -= 1f;
        if (teclado.dKey.isPressed) horizontal += 1f;
        if (teclado.aKey.isPressed) horizontal -= 1f;

        // 2. Direcciones relativas a la camara (sin la inclinacion)
        Vector3 adelante = transform.forward;
        Vector3 derecha = transform.right;

        if (camara != null)
        {
            adelante = camara.forward;
            adelante.y = 0f;
            adelante.Normalize();

            derecha = camara.right;
            derecha.y = 0f;
            derecha.Normalize();
        }

        direccionMovimiento = adelante * vertical + derecha * horizontal;

        // Para que en diagonal no se mueva mas rapido
        if (direccionMovimiento.magnitude > 1f)
        {
            direccionMovimiento.Normalize();
        }

        // 3. Guardamos que se apreto saltar (se aplica en FixedUpdate)
        if (teclado.spaceKey.wasPressedThisFrame)
        {
            quiereSaltar = true;
        }
    }

    // ---------- FIXED UPDATE: todo lo que toca el Rigidbody ----------
    void FixedUpdate()
    {
        enSuelo = HayPiso();

        // Si estamos parados sobre una plataforma movil, nos movemos con ella
        if (enSuelo && plataformaDebajo != null)
        {
            rb.MovePosition(rb.position + plataformaDebajo.movimientoEsteFrame);
        }

        Rotar();
        Mover();
        Saltar();

        // Nuestra gravedad (ForceMode.Acceleration ignora la masa)
        rb.AddForce(Vector3.up * gravedad, ForceMode.Acceleration);
    }

    // El Player mira hacia donde mira la camara (solo el giro horizontal)
    void Rotar()
    {
        if (camara == null) return;

        Quaternion rotacionDeseada = Quaternion.Euler(0f, camara.eulerAngles.y, 0f);
        Quaternion nuevaRotacion = Quaternion.Slerp(rb.rotation, rotacionDeseada, velocidadGiro * Time.fixedDeltaTime);
        rb.MoveRotation(nuevaRotacion);
    }

    void Mover()
    {
        // Cambiamos solo X y Z; la Y la manejan la gravedad y el salto
        Vector3 velocidadActual = rb.linearVelocity;
        velocidadActual.x = direccionMovimiento.x * velocidad * multiplicadorVelocidad;
        velocidadActual.z = direccionMovimiento.z * velocidad * multiplicadorVelocidad;
        rb.linearVelocity = velocidadActual;
    }

    void Saltar()
    {
        // Esta "apoyado" si el rayo toca el piso y no esta subiendo.
        // (Justo despues de saltar el rayo todavia toca el piso un instante,
        //  por eso tambien revisamos que no estemos subiendo.)
        bool apoyado = enSuelo && rb.linearVelocity.y <= 0.1f;

        // Al tocar el piso se recarga el doble salto
        if (apoyado)
        {
            puedeDobleSaltar = true;
        }

        if (quiereSaltar)
        {
            if (apoyado)
            {
                // Salto normal desde el piso
                AplicarSalto(alturaSalto);
            }
            else if (puedeDobleSaltar)
            {
                // Doble salto en el aire (solo una vez hasta volver a tocar el piso)
                AplicarSalto(alturaDobleSalto);
                puedeDobleSaltar = false;
            }
        }

        quiereSaltar = false; // ya usamos el input de este salto
    }

    void AplicarSalto(float altura)
    {
        // Formula de fisica: v = raiz(altura * -2 * gravedad)
        // Reemplazamos la velocidad en Y (no la sumamos), asi el doble salto
        // siempre llega a la misma altura aunque el jugador ya este cayendo.
        Vector3 velocidadActual = rb.linearVelocity;
        // El aumento de salto multiplica la altura (sirve para salto y doble salto)
        velocidadActual.y = Mathf.Sqrt(altura * multiplicadorSalto * -2f * gravedad);
        rb.linearVelocity = velocidadActual;
    }

    // Tira un rayo hacia abajo e ignora los colliders del propio Player
    bool HayPiso()
    {
        plataformaDebajo = null;

        Vector3 origenRayo = transform.position + Vector3.up * 0.5f;
        float largoRayo = alturaPies + 0.5f + 0.15f;

        RaycastHit[] golpes = Physics.RaycastAll(origenRayo, Vector3.down, largoRayo, capasSuelo, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < golpes.Length; i++)
        {
            // IsChildOf devuelve true para el Player y todos sus hijos
            if (!golpes[i].collider.transform.IsChildOf(transform))
            {
                // Guardamos si el piso es una plataforma movil (o hijo de una)
                plataformaDebajo = golpes[i].collider.GetComponentInParent<MovingPlatform>();
                return true;
            }
        }

        return false;
    }

    // ---------- RECOGIBLES (los llama el script Pickup) ----------

    public void AgregarColeccionable(int puntos)
    {
        coleccionables += puntos;
        Debug.Log("Coleccionables: " + coleccionables);
    }

    // Si agarras otro igual mientras esta activo, se reinicia el tiempo
    public void AumentarVelocidad(float multiplicador, float duracion)
    {
        multiplicadorVelocidad = multiplicador;
        tiempoVelocidad = duracion;
        Debug.Log("Aumento de velocidad x" + multiplicador + " por " + duracion + " segundos");
    }

    public void AumentarSalto(float multiplicador, float duracion)
    {
        multiplicadorSalto = multiplicador;
        tiempoSalto = duracion;
        Debug.Log("Aumento de salto x" + multiplicador + " por " + duracion + " segundos");
    }

    // Descuenta el tiempo de los aumentos y los apaga cuando se terminan
    void ActualizarAumentos()
    {
        if (tiempoVelocidad > 0f)
        {
            tiempoVelocidad -= Time.deltaTime;
            if (tiempoVelocidad <= 0f)
            {
                multiplicadorVelocidad = 1f;
                Debug.Log("Se termino el aumento de velocidad");
            }
        }

        if (tiempoSalto > 0f)
        {
            tiempoSalto -= Time.deltaTime;
            if (tiempoSalto <= 0f)
            {
                multiplicadorSalto = 1f;
                Debug.Log("Se termino el aumento de salto");
            }
        }
    }

    // Dibuja el rayo del suelo en la escena para ver si esta bien configurado
    void OnDrawGizmosSelected()
    {
        Gizmos.color = enSuelo ? Color.green : Color.red;
        Vector3 origen = transform.position + Vector3.up * 0.5f;
        Gizmos.DrawLine(origen, origen + Vector3.down * (alturaPies + 0.65f));
    }
}