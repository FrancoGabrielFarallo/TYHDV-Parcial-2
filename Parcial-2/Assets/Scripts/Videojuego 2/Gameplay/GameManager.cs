using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// =====================================================================
//  GameManager.cs  -  El "árbitro" de la partida.
//
//  Controla las fases del turno y hace cumplir las reglas.
//  Tanto el humano (con el mouse) como la IA (con su script) llaman a
//  las MISMAS funciones de acá: IntentarJugarCarta, DeclararAtaque,
//  ConfirmarAtaques, DeclararBloqueo... Así los dos juegan con las
//  mismas reglas.
//
//  Fases:  IniciarTurno -> BajarCartas -> Combate -> Defensa -> FinalizarTurno
// =====================================================================
public class GameManager : MonoBehaviour
{
    // "Singleton": una variable estática para encontrar el GameManager
    // desde cualquier script escribiendo GameManager.instancia
    public static GameManager instancia;

    [Header("ScriptableObjects")]
    public GameRules reglas;

    [Header("Jugadores")]
    public Jugador jugadorHumano;
    public Jugador jugadorIA;
    public IAJugador cerebroIA;

    [Header("Prefabs")]
    public CardView prefabCarta;

    [Header("UI")]
    public Transform capaArrastre;       // objeto vacío al final del Canvas
    public TMP_Text textoFase;
    public TMP_Text textoMensaje;
    public Button botonSiguiente;
    public TMP_Text textoBotonSiguiente;
    public GameObject panelFinDelJuego;
    public TMP_Text textoFinDelJuego;
    public string escenaDeckBuilder = "DeckBuilder";

    [Header("Estado (solo para mirar)")]
    public Fase faseActual = Fase.IniciarTurno;
    public Jugador jugadorActivo;
    public int numeroDeTurno = 0;
    public List<Ataque> ataques = new List<Ataque>();
    public CardView criaturaSeleccionada;

    // =================================================================
    //  INICIO
    // =================================================================
    void Awake()
    {
        instancia = this;
    }

    void OnDestroy()
    {
        if (instancia == this) instancia = null;
    }

    void Start()
    {
        if (panelFinDelJuego != null) panelFinDelJuego.SetActive(false);
        botonSiguiente.onClick.AddListener(BotonSiguientePresionado);
        ComenzarPartida();
    }

    void ComenzarPartida()
    {
        // Controles para que no explote si falta algo en el Inspector
        if (jugadorHumano.datos == null || jugadorHumano.datos.mazo == null ||
            jugadorIA.datos == null || jugadorIA.datos.mazo == null)
        {
            Debug.LogError("Falta asignar PlayerData o su mazo en algún Jugador.");
            return;
        }
        if (jugadorHumano.datos.mazo.ContarCartas() != reglas.cartasEnElMazo)
        {
            Debug.LogWarning("Tu mazo no tiene " + reglas.cartasEnElMazo + " cartas. Armalo en el Deck Builder.");
        }

        jugadorHumano.PrepararPartida(reglas.vidaInicial);
        jugadorIA.PrepararPartida(reglas.vidaInicial);

        for (int i = 0; i < reglas.cartasManoInicial; i++)
        {
            jugadorHumano.RobarCarta();
            jugadorIA.RobarCarta();
        }

        numeroDeTurno = 0;
        IniciarTurno(jugadorHumano);   // empieza el humano
    }

    public Jugador ObtenerRival(Jugador jugador)
    {
        if (jugador == jugadorHumano) return jugadorIA;
        return jugadorHumano;
    }

    // =================================================================
    //  FASE 1: INICIAR TURNO
    // =================================================================
    void IniciarTurno(Jugador jugador)
    {
        if (faseActual == Fase.FinDelJuego) return;

        jugadorActivo = jugador;
        numeroDeTurno++;
        CambiarFase(Fase.IniciarTurno);

        // Se habilitan todas las cartas deshabilitadas
        jugador.HabilitarTodo();

        // Se roba una carta (menos el primer jugador en el turno 1)
        bool debeRobar = true;
        if (numeroDeTurno == 1 && reglas.primerJugadorNoRoba) debeRobar = false;

        if (debeRobar)
        {
            bool pudoRobar = jugador.RobarCarta();
            if (!pudoRobar)
            {
                TerminarPartida(jugador.datos.nombreJugador + " no tiene cartas para robar. ¡Gana "
                    + ObtenerRival(jugador).datos.nombreJugador + "!");
                return;
            }
        }

        // Pasamos directo a bajar cartas
        CambiarFase(Fase.BajarCartas);

        if (jugador.esIA)
        {
            Mensaje("Turno de " + jugador.datos.nombreJugador + "...");
            cerebroIA.JugarTurno();
        }
        else
        {
            Mensaje("Tu turno: arrastrá cartas de tu mano al campo.");
        }
    }

    // =================================================================
    //  FASE 2: BAJAR CARTAS
    // =================================================================

    // ¿Se puede arrastrar esta carta? (solo el humano arrastra)
    public bool PuedeArrastrar(CardView carta)
    {
        if (faseActual != Fase.BajarCartas) return false;
        if (carta.duenio != jugadorActivo) return false;
        if (carta.duenio.esIA) return false;
        if (carta.zona != ZonaCarta.Mano) return false;
        return true;
    }

    // Intenta jugar una carta que se soltó sobre "destino".
    // Devuelve true si se jugó, false si no (y la carta vuelve a la mano).
    public bool IntentarJugarCarta(CardView carta, GameObject destino)
    {
        Jugador jugador = carta.duenio;

        if (faseActual != Fase.BajarCartas || jugador != jugadorActivo || carta.zona != ZonaCarta.Mano)
        {
            Mensaje("Solo podés jugar cartas en la fase 'Bajar cartas' de tu turno.");
            return false;
        }
        if (destino == null)
        {
            Mensaje("Soltá la carta sobre el campo.");
            return false;
        }

        // Miramos qué hay debajo del mouse. GetComponentInParent busca
        // en el objeto y en sus padres.
        CardView cartaDestino = destino.GetComponentInParent<CardView>();
        PlayerPortrait retratoDestino = destino.GetComponentInParent<PlayerPortrait>();
        FieldZone campoDestino = destino.GetComponentInParent<FieldZone>();

        // Si soltamos sobre una carta que NO está en el campo (ej. en la mano), no cuenta
        if (cartaDestino != null && cartaDestino.zona != ZonaCarta.Campo) cartaDestino = null;

        TipoCarta tipo = carta.datos.ObtenerTipo();
        if (tipo == TipoCarta.Tierra)
        {
            return JugarTierra(carta, campoDestino);
        }
        if (tipo == TipoCarta.Criatura)
        {
            return JugarCriatura(carta, campoDestino);
        }
        return JugarHechizo(carta, cartaDestino, retratoDestino, campoDestino);
    }

    bool JugarTierra(CardView carta, FieldZone campo)
    {
        Jugador jugador = carta.duenio;

        if (campo == null || campo.duenio != jugador)
        {
            Mensaje("Soltá la tierra en TU campo.");
            return false;
        }
        if (jugador.tierrasJugadasEsteTurno >= reglas.tierrasPorTurno)
        {
            Mensaje("Solo podés bajar " + reglas.tierrasPorTurno + " tierra por turno.");
            return false;
        }

        jugador.mano.Remove(carta);
        jugador.tierras.Add(carta);
        jugador.tierrasJugadasEsteTurno++;
        carta.zona = ZonaCarta.Campo;
        carta.transform.SetParent(jugador.zonaTierras, false);
        carta.MostrarDorso(false);
        carta.ActualizarVisual();

        Mensaje(jugador.datos.nombreJugador + " baja " + carta.datos.nombre + ".");
        ActualizarUI();
        return true;
    }

    bool JugarCriatura(CardView carta, FieldZone campo)
    {
        Jugador jugador = carta.duenio;

        if (campo == null || campo.duenio != jugador)
        {
            Mensaje("Soltá la criatura en TU campo.");
            return false;
        }
        if (!jugador.PuedePagar(carta.datos))
        {
            Mensaje("No tenés mana suficiente para " + carta.datos.nombre + ".");
            return false;
        }

        jugador.Pagar(carta.datos);
        jugador.mano.Remove(carta);
        jugador.criaturas.Add(carta);
        carta.zona = ZonaCarta.Campo;
        carta.recienInvocada = reglas.mareoDeInvocacion;
        carta.transform.SetParent(jugador.zonaCriaturas, false);
        carta.MostrarDorso(false);
        carta.ActualizarVisual();

        Mensaje(jugador.datos.nombreJugador + " invoca " + carta.datos.nombre + ".");
        ActualizarUI();
        return true;
    }

    bool JugarHechizo(CardView carta, CardView cartaDestino, PlayerPortrait retratoDestino, FieldZone campoDestino)
    {
        Jugador jugador = carta.duenio;
        Jugador rival = ObtenerRival(jugador);
        SpellCardData hechizo = carta.datos as SpellCardData;

        if (!jugador.PuedePagar(hechizo))
        {
            Mensaje("No tenés mana suficiente para " + hechizo.nombre + ".");
            return false;
        }

        // 1) Revisamos que el objetivo sea válido ANTES de pagar
        bool destinoEsCriatura = cartaDestino != null && cartaDestino.EsCriatura();

        if (hechizo.efecto == EfectoHechizo.DanioAlJugador)
        {
            if (campoDestino == null && retratoDestino == null)
            {
                Mensaje("Soltá el hechizo sobre el campo o sobre el rival.");
                return false;
            }
        }
        else if (hechizo.efecto == EfectoHechizo.DanioACriaturaOJugador)
        {
            if (!destinoEsCriatura && retratoDestino == null)
            {
                Mensaje("Soltá el hechizo ENCIMA de una criatura o de un jugador.");
                return false;
            }
        }
        else if (hechizo.efecto == EfectoHechizo.PotenciarCriatura)
        {
            if (!destinoEsCriatura)
            {
                Mensaje("Soltá el hechizo ENCIMA de una criatura.");
                return false;
            }
        }

        // 2) Pagamos
        jugador.Pagar(hechizo);

        // 3) Aplicamos el efecto
        if (hechizo.efecto == EfectoHechizo.DanioAlJugador)
        {
            rival.RecibirDanio(hechizo.cantidadDanio);
            Mensaje(hechizo.nombre + ": " + hechizo.cantidadDanio + " de daño a " + rival.datos.nombreJugador + ".");
        }
        else if (hechizo.efecto == EfectoHechizo.DanioACriaturaOJugador)
        {
            if (destinoEsCriatura)
            {
                cartaDestino.RecibirDanio(hechizo.cantidadDanio);
                Mensaje(hechizo.nombre + ": " + hechizo.cantidadDanio + " de daño a " + cartaDestino.datos.nombre + ".");
            }
            else
            {
                retratoDestino.duenio.RecibirDanio(hechizo.cantidadDanio);
                Mensaje(hechizo.nombre + ": " + hechizo.cantidadDanio + " de daño a " + retratoDestino.duenio.datos.nombreJugador + ".");
            }
        }
        else if (hechizo.efecto == EfectoHechizo.PotenciarCriatura)
        {
            cartaDestino.AplicarBonus(hechizo.bonusAtaque, hechizo.bonusVida, hechizo.soloHastaFinDeTurno);
            Mensaje(hechizo.nombre + ": " + cartaDestino.datos.nombre + " gana +" + hechizo.bonusAtaque + "/+" + hechizo.bonusVida + ".");
        }

        // 4) El hechizo se va al cementerio
        jugador.mano.Remove(carta);
        jugador.cementerio.Add(hechizo);
        carta.zona = ZonaCarta.Cementerio;
        Destroy(carta.gameObject);

        QuitarCriaturasMuertas();
        if (ComprobarFinDePartida()) return true;
        ActualizarUI();
        return true;
    }

    // =================================================================
    //  FASE 3: COMBATE
    // =================================================================
    public void IrACombate()
    {
        if (faseActual != Fase.BajarCartas) return;
        LimpiarSeleccion();
        ataques.Clear();
        CambiarFase(Fase.Combate);
        if (!jugadorActivo.esIA)
        {
            Mensaje("Combate: clic en tu criatura y después clic en una criatura rival o en el retrato rival.");
        }
    }

    // Busca el ataque de una criatura (o null si no está atacando)
    public Ataque BuscarAtaqueDe(CardView atacante)
    {
        for (int i = 0; i < ataques.Count; i++)
        {
            if (ataques[i].atacante == atacante) return ataques[i];
        }
        return null;
    }

    // objetivo = criatura rival, o null para atacar al jugador
    public bool DeclararAtaque(CardView atacante, CardView objetivo)
    {
        if (faseActual != Fase.Combate) return false;
        if (atacante.duenio != jugadorActivo) return false;
        if (!atacante.PuedeAtacar())
        {
            Mensaje(atacante.datos.nombre + " no puede atacar (deshabilitada o recién invocada).");
            return false;
        }
        if (objetivo != null)
        {
            if (objetivo.duenio == jugadorActivo || objetivo.zona != ZonaCarta.Campo || !objetivo.EsCriatura())
            {
                Mensaje("Solo podés atacar criaturas rivales que estén en el campo.");
                return false;
            }
        }

        // Si ya estaba atacando, le cambio el objetivo
        Ataque ataque = BuscarAtaqueDe(atacante);
        if (ataque == null)
        {
            ataque = new Ataque();
            ataque.atacante = atacante;
            ataques.Add(ataque);
        }
        ataque.objetivo = objetivo;

        string nombreObjetivo = ObtenerRival(jugadorActivo).datos.nombreJugador;
        if (objetivo != null) nombreObjetivo = objetivo.datos.nombre;
        atacante.SetEstado("ATACA a " + nombreObjetivo);
        Mensaje(atacante.datos.nombre + " va a atacar a " + nombreObjetivo + ".");
        ActualizarUI();
        return true;
    }

    public void CancelarAtaque(CardView atacante)
    {
        Ataque ataque = BuscarAtaqueDe(atacante);
        if (ataque != null)
        {
            ataques.Remove(ataque);
            atacante.SetEstado("");
            Mensaje(atacante.datos.nombre + " ya no ataca.");
        }
        ActualizarUI();
    }

    // Termina la fase de combate y pasa a la defensa del rival
    public void ConfirmarAtaques()
    {
        if (faseActual != Fase.Combate) return;
        LimpiarSeleccion();

        // Las criaturas que atacan se deshabilitan
        for (int i = 0; i < ataques.Count; i++)
        {
            ataques[i].atacante.Deshabilitar();
        }

        if (ataques.Count == 0)
        {
            FinalizarTurno();
            return;
        }

        Jugador defensor = ObtenerRival(jugadorActivo);

        // Si el defensor no tiene con qué bloquear, resolvemos directo
        if (!defensor.PuedeBloquearAlgo(ataques))
        {
            ResolverCombate();
            return;
        }

        CambiarFase(Fase.Defensa);

        if (defensor.esIA)
        {
            cerebroIA.ElegirDefensa(ataques);
            // Esperamos un poco para que se vea cómo defendió
            Invoke("ResolverCombate", reglas.esperaEntreAccionesIA);
        }
        else
        {
            Mensaje("¡Te atacan! Clic en tu criatura y después clic en la criatura atacante para bloquearla. Luego 'Confirmar defensa'.");
        }
    }

    // =================================================================
    //  FASE 4: DEFENSA (la elige el rival del jugador activo)
    // =================================================================

    // Busca qué ataque está bloqueando una criatura (o null)
    public Ataque BuscarAtaqueBloqueadoPor(CardView bloqueador)
    {
        for (int i = 0; i < ataques.Count; i++)
        {
            if (ataques[i].bloqueador == bloqueador) return ataques[i];
        }
        return null;
    }

    public bool DeclararBloqueo(CardView bloqueador, Ataque ataque)
    {
        if (faseActual != Fase.Defensa) return false;
        if (bloqueador.duenio == jugadorActivo) return false;

        if (!bloqueador.PuedeBloquear(ataque.atacante))
        {
            if (ataque.atacante.EsVoladora() && !bloqueador.EsVoladora())
            {
                Mensaje("Una criatura de tierra no puede bloquear a una voladora.");
            }
            else
            {
                Mensaje(bloqueador.datos.nombre + " no puede bloquear (está deshabilitada).");
            }
            return false;
        }
        if (ataque.bloqueador != null)
        {
            Mensaje(ataque.atacante.datos.nombre + " ya está bloqueada.");
            return false;
        }

        // Si ya bloqueaba a otra, la saco de ese bloqueo
        Ataque anterior = BuscarAtaqueBloqueadoPor(bloqueador);
        if (anterior != null) anterior.bloqueador = null;

        ataque.bloqueador = bloqueador;
        bloqueador.SetEstado("BLOQUEA a " + ataque.atacante.datos.nombre);
        Mensaje(bloqueador.datos.nombre + " bloquea a " + ataque.atacante.datos.nombre + ".");
        return true;
    }

    public void CancelarBloqueo(CardView bloqueador)
    {
        Ataque ataque = BuscarAtaqueBloqueadoPor(bloqueador);
        if (ataque != null)
        {
            ataque.bloqueador = null;
            bloqueador.SetEstado("");
            Mensaje(bloqueador.datos.nombre + " ya no bloquea.");
        }
    }

    public void ConfirmarDefensa()
    {
        if (faseActual != Fase.Defensa) return;
        LimpiarSeleccion();
        ResolverCombate();
    }

    // Aplica el daño de todos los ataques
    void ResolverCombate()
    {
        if (faseActual == Fase.FinDelJuego) return;

        Jugador defensor = ObtenerRival(jugadorActivo);

        for (int i = 0; i < ataques.Count; i++)
        {
            Ataque ataque = ataques[i];
            CardView atacante = ataque.atacante;
            if (atacante.EstaMuerta()) continue;

            if (ataque.bloqueador != null && !ataque.bloqueador.EstaMuerta())
            {
                // Bloqueado: pelean atacante y bloqueador
                Pelear(atacante, ataque.bloqueador);
            }
            else if (ataque.objetivo != null)
            {
                // Atacó a una criatura y nadie bloqueó: pelean con la criatura objetivo
                if (!ataque.objetivo.EstaMuerta())
                {
                    Pelear(atacante, ataque.objetivo);
                }
            }
            else
            {
                // Atacó al jugador y nadie bloqueó
                defensor.RecibirDanio(atacante.ObtenerAtaque());
                Mensaje(atacante.datos.nombre + " le hace " + atacante.ObtenerAtaque() + " de daño a " + defensor.datos.nombreJugador + ".");
            }
        }

        QuitarCriaturasMuertas();
        if (ComprobarFinDePartida()) return;
        FinalizarTurno();
    }

    // Dos criaturas se hacen daño al mismo tiempo
    void Pelear(CardView a, CardView b)
    {
        int danioDeA = a.ObtenerAtaque();
        int danioDeB = b.ObtenerAtaque();
        a.RecibirDanio(danioDeB);
        b.RecibirDanio(danioDeA);
        Mensaje(a.datos.nombre + " pelea contra " + b.datos.nombre + ".");
    }

    // =================================================================
    //  FASE 5: FINALIZAR TURNO
    // =================================================================
    public void FinalizarTurno()
    {
        if (faseActual == Fase.FinDelJuego) return;

        CambiarFase(Fase.FinalizarTurno);
        LimpiarSeleccion();

        // Se van los bonus temporales y (si la regla está activa) se cura el daño
        jugadorHumano.FinDeTurno(reglas.curarCriaturasAlFinalDelTurno);
        jugadorIA.FinDeTurno(reglas.curarCriaturasAlFinalDelTurno);
        ataques.Clear();

        QuitarCriaturasMuertas();
        if (ComprobarFinDePartida()) return;

        // Le toca al otro
        IniciarTurno(ObtenerRival(jugadorActivo));
    }

    // =================================================================
    //  CLICS DEL HUMANO
    // =================================================================
    public void CartaClickeada(CardView carta)
    {
        if (!carta.EsCriatura() || carta.zona != ZonaCarta.Campo) return;

        // --- Mi turno, fase de combate: elegir atacante y objetivo ---
        if (faseActual == Fase.Combate && jugadorActivo == jugadorHumano)
        {
            if (carta.duenio == jugadorHumano)
            {
                if (BuscarAtaqueDe(carta) != null)
                {
                    CancelarAtaque(carta);        // clic de nuevo = no ataca
                    LimpiarSeleccion();
                }
                else if (carta.PuedeAtacar())
                {
                    Seleccionar(carta);
                    Mensaje("Elegí el objetivo de " + carta.datos.nombre + ": una criatura rival o el retrato rival.");
                }
                else
                {
                    Mensaje(carta.datos.nombre + " no puede atacar (deshabilitada o recién invocada).");
                }
            }
            else if (criaturaSeleccionada != null)
            {
                DeclararAtaque(criaturaSeleccionada, carta);
                LimpiarSeleccion();
            }
            return;
        }

        // --- Turno de la IA, fase de defensa: elegir bloqueadores ---
        if (faseActual == Fase.Defensa && jugadorActivo == jugadorIA)
        {
            if (carta.duenio == jugadorHumano)
            {
                if (BuscarAtaqueBloqueadoPor(carta) != null)
                {
                    CancelarBloqueo(carta);
                    LimpiarSeleccion();
                }
                else if (carta.deshabilitada)
                {
                    Mensaje(carta.datos.nombre + " está deshabilitada y no puede bloquear.");
                }
                else
                {
                    Seleccionar(carta);
                    Mensaje("Ahora hacé clic en la criatura rival que querés bloquear.");
                }
            }
            else if (criaturaSeleccionada != null)
            {
                Ataque ataque = BuscarAtaqueDe(carta);
                if (ataque == null)
                {
                    Mensaje(carta.datos.nombre + " no está atacando.");
                }
                else
                {
                    DeclararBloqueo(criaturaSeleccionada, ataque);
                }
                LimpiarSeleccion();
            }
        }
    }

    public void RetratoClickeado(Jugador jugador)
    {
        // Atacar al jugador rival
        if (faseActual == Fase.Combate && jugadorActivo == jugadorHumano &&
            jugador == jugadorIA && criaturaSeleccionada != null)
        {
            DeclararAtaque(criaturaSeleccionada, null);
            LimpiarSeleccion();
        }
    }

    void Seleccionar(CardView carta)
    {
        LimpiarSeleccion();
        criaturaSeleccionada = carta;
        carta.SetResaltado(true);
    }

    void LimpiarSeleccion()
    {
        if (criaturaSeleccionada != null) criaturaSeleccionada.SetResaltado(false);
        criaturaSeleccionada = null;
    }

    // =================================================================
    //  BOTÓN "SIGUIENTE"
    // =================================================================
    public void BotonSiguientePresionado()
    {
        if (faseActual == Fase.FinDelJuego) return;

        if (jugadorActivo == jugadorHumano)
        {
            if (faseActual == Fase.BajarCartas) IrACombate();
            else if (faseActual == Fase.Combate) ConfirmarAtaques();
        }
        else
        {
            // En el turno de la IA solo podemos confirmar nuestra defensa
            if (faseActual == Fase.Defensa) ConfirmarDefensa();
        }
    }

    // =================================================================
    //  FIN DE LA PARTIDA
    // =================================================================
    void QuitarCriaturasMuertas()
    {
        // Si la criatura seleccionada murió, la deselecciono
        if (criaturaSeleccionada != null && criaturaSeleccionada.EstaMuerta()) criaturaSeleccionada = null;
        jugadorHumano.QuitarCriaturasMuertas();
        jugadorIA.QuitarCriaturasMuertas();
    }

    bool ComprobarFinDePartida()
    {
        bool humanoMuerto = jugadorHumano.vida <= 0;
        bool iaMuerta = jugadorIA.vida <= 0;

        if (humanoMuerto && iaMuerta)
        {
            TerminarPartida("¡Empate!");
            return true;
        }
        if (humanoMuerto)
        {
            TerminarPartida("Perdiste. Gana " + jugadorIA.datos.nombreJugador + ".");
            return true;
        }
        if (iaMuerta)
        {
            TerminarPartida("¡Ganaste!");
            return true;
        }
        return false;
    }

    void TerminarPartida(string texto)
    {
        faseActual = Fase.FinDelJuego;
        CancelInvoke();
        if (cerebroIA != null) cerebroIA.Detener();
        Mensaje(texto);
        ActualizarUI();
        if (panelFinDelJuego != null) panelFinDelJuego.SetActive(true);
        if (textoFinDelJuego != null) textoFinDelJuego.text = texto;
    }

    // Para los botones del panel de fin de juego
    public void Reiniciar()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void VolverAlDeckBuilder()
    {
        SceneManager.LoadScene(escenaDeckBuilder);
    }

    // =================================================================
    //  UI
    // =================================================================
    void CambiarFase(Fase nuevaFase)
    {
        faseActual = nuevaFase;
        ActualizarUI();
    }

    public void Mensaje(string texto)
    {
        if (textoMensaje != null) textoMensaje.text = texto;
        Debug.Log(texto);
    }

    string NombreDeFase(Fase fase)
    {
        if (fase == Fase.IniciarTurno) return "Iniciar turno";
        if (fase == Fase.BajarCartas) return "Bajar cartas";
        if (fase == Fase.Combate) return "Combate";
        if (fase == Fase.Defensa) return "Defensa (rival)";
        if (fase == Fase.FinalizarTurno) return "Finalizar turno";
        return "Fin del juego";
    }

    public void ActualizarUI()
    {
        if (textoFase != null && jugadorActivo != null)
        {
            textoFase.text = "Turno " + numeroDeTurno + " - " + jugadorActivo.datos.nombreJugador + "\n" + NombreDeFase(faseActual);
        }

        // El botón cambia de texto según la fase
        bool interactuable = false;
        string texto = "Esperando...";

        if (faseActual == Fase.FinDelJuego)
        {
            texto = "Fin";
        }
        else if (jugadorActivo == jugadorHumano)
        {
            if (faseActual == Fase.BajarCartas)
            {
                interactuable = true;
                texto = "Ir a combate";
            }
            else if (faseActual == Fase.Combate)
            {
                interactuable = true;
                if (ataques.Count == 0) texto = "Terminar turno";
                else texto = "Confirmar ataque";
            }
        }
        else if (faseActual == Fase.Defensa)
        {
            interactuable = true;
            texto = "Confirmar defensa";
        }

        if (botonSiguiente != null) botonSiguiente.interactable = interactuable;
        if (textoBotonSiguiente != null) textoBotonSiguiente.text = texto;

        if (jugadorHumano != null) jugadorHumano.ActualizarRetrato();
        if (jugadorIA != null) jugadorIA.ActualizarRetrato();
    }
}
