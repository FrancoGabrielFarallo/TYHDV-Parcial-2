using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// =====================================================================
//  IAJugador.cs  -  El "cerebro" del rival.
//
//  La IA NO tiene reglas especiales: llama a las mismas funciones del
//  GameManager que usa el humano cuando arrastra o hace clic.
//  Lo único que hace este script es DECIDIR qué hacer.
//
//  Usa una "corrutina" (IEnumerator + yield return) para poder
//  esperar unos segundos entre jugada y jugada, así se ve lo que hace.
// =====================================================================
public class IAJugador : MonoBehaviour
{
    [Tooltip("El componente Jugador que controla esta IA")]
    public Jugador yo;

    // Atajos para no escribir tanto
    GameManager Gm()
    {
        return GameManager.instancia;
    }

    Jugador Rival()
    {
        return Gm().ObtenerRival(yo);
    }

    // =================================================================
    //  EL TURNO DE LA IA
    // =================================================================
    public void JugarTurno()
    {
        StartCoroutine(RutinaDelTurno());
    }

    public void Detener()
    {
        StopAllCoroutines();
    }

    IEnumerator RutinaDelTurno()
    {
        float espera = Gm().reglas.esperaEntreAccionesIA;
        yield return new WaitForSeconds(espera);

        // ---------- FASE BAJAR CARTAS ----------

        // 1) Bajar una tierra (si tiene)
        CardView tierra = ElegirTierra();
        if (tierra != null)
        {
            Gm().IntentarJugarCarta(tierra, yo.campo.gameObject);
            yield return new WaitForSeconds(espera);
        }

        // 2) Jugar criaturas y hechizos mientras pueda pagar alguno
        bool jugoAlgo = true;
        while (jugoAlgo)
        {
            if (Gm().faseActual != Fase.BajarCartas) yield break;   // por si terminó la partida

            jugoAlgo = false;

            CardView criatura = ElegirCriatura();
            if (criatura != null)
            {
                jugoAlgo = Gm().IntentarJugarCarta(criatura, yo.campo.gameObject);
            }
            else
            {
                jugoAlgo = IntentarLanzarUnHechizo();
            }

            if (jugoAlgo) yield return new WaitForSeconds(espera);
        }

        if (Gm().faseActual != Fase.BajarCartas) yield break;

        // ---------- FASE COMBATE ----------
        Gm().IrACombate();
        yield return new WaitForSeconds(espera * 0.5f);

        // Copio la lista porque puede cambiar mientras la recorro
        List<CardView> misCriaturas = new List<CardView>(yo.criaturas);
        for (int i = 0; i < misCriaturas.Count; i++)
        {
            CardView atacante = misCriaturas[i];
            if (atacante.PuedeAtacar() && ConvieneAtacar(atacante))
            {
                CardView objetivo = ElegirObjetivoDeAtaque(atacante);
                Gm().DeclararAtaque(atacante, objetivo);
                yield return new WaitForSeconds(espera * 0.5f);
            }
        }

        yield return new WaitForSeconds(espera * 0.5f);

        // Pasa a la defensa del humano (o al final del turno si no atacó)
        Gm().ConfirmarAtaques();
    }

    // =================================================================
    //  DECISIONES: BAJAR CARTAS
    // =================================================================

    // Elige la tierra del color que más le falta para las cartas de su mano
    CardView ElegirTierra()
    {
        if (yo.tierrasJugadasEsteTurno >= Gm().reglas.tierrasPorTurno) return null;

        CardView mejor = null;
        int mejorPuntaje = -999;

        for (int i = 0; i < yo.mano.Count; i++)
        {
            LandCardData tierra = yo.mano[i].datos as LandCardData;
            if (tierra == null) continue;

            // Puntaje = cuánto de ese color piden mis cartas - cuántas tierras de ese color ya tengo
            int pedido = 0;
            for (int j = 0; j < yo.mano.Count; j++)
            {
                pedido = pedido + yo.mano[j].datos.ObtenerCostoDeColor(tierra.colorQueProduce);
            }
            int tengo = ContarMisTierrasDeColor(tierra.colorQueProduce);
            int puntaje = pedido - tengo;

            if (puntaje > mejorPuntaje)
            {
                mejorPuntaje = puntaje;
                mejor = yo.mano[i];
            }
        }
        return mejor;
    }

    int ContarMisTierrasDeColor(ColorMana color)
    {
        int cantidad = 0;
        for (int i = 0; i < yo.tierras.Count; i++)
        {
            LandCardData tierra = yo.tierras[i].datos as LandCardData;
            if (tierra.colorQueProduce == color) cantidad++;
        }
        return cantidad;
    }

    // Elige la criatura MÁS CARA que puede pagar
    CardView ElegirCriatura()
    {
        CardView mejor = null;
        for (int i = 0; i < yo.mano.Count; i++)
        {
            CardView carta = yo.mano[i];
            if (carta.datos.ObtenerTipo() != TipoCarta.Criatura) continue;
            if (!yo.PuedePagar(carta.datos)) continue;

            if (mejor == null || carta.datos.ObtenerCostoTotal() > mejor.datos.ObtenerCostoTotal())
            {
                mejor = carta;
            }
        }
        return mejor;
    }

    // Busca un hechizo que convenga lanzar y lo lanza. Devuelve true si lanzó uno.
    bool IntentarLanzarUnHechizo()
    {
        for (int i = 0; i < yo.mano.Count; i++)
        {
            CardView carta = yo.mano[i];
            SpellCardData hechizo = carta.datos as SpellCardData;
            if (hechizo == null) continue;
            if (!yo.PuedePagar(hechizo)) continue;

            GameObject objetivo = ElegirObjetivoDeHechizo(hechizo);
            if (objetivo != null)
            {
                return Gm().IntentarJugarCarta(carta, objetivo);
            }
        }
        return false;
    }

    GameObject ElegirObjetivoDeHechizo(SpellCardData hechizo)
    {
        Jugador rival = Rival();

        if (hechizo.efecto == EfectoHechizo.DanioAlJugador)
        {
            return rival.retrato.gameObject;
        }

        if (hechizo.efecto == EfectoHechizo.DanioACriaturaOJugador)
        {
            // Si el daño mata al rival, ¡a la cara!
            if (rival.vida <= hechizo.cantidadDanio) return rival.retrato.gameObject;

            // Si puede matar una criatura rival, elige la de más ataque
            CardView mejor = null;
            for (int i = 0; i < rival.criaturas.Count; i++)
            {
                CardView c = rival.criaturas[i];
                if (c.ObtenerVida() <= hechizo.cantidadDanio)
                {
                    if (mejor == null || c.ObtenerAtaque() > mejor.ObtenerAtaque()) mejor = c;
                }
            }
            if (mejor != null) return mejor.gameObject;

            // Si no, daño al jugador
            return rival.retrato.gameObject;
        }

        if (hechizo.efecto == EfectoHechizo.PotenciarCriatura)
        {
            // Potencia a su criatura más fuerte que pueda atacar este turno
            CardView mejor = null;
            for (int i = 0; i < yo.criaturas.Count; i++)
            {
                CardView c = yo.criaturas[i];
                if (!c.PuedeAtacar()) continue;
                if (mejor == null || c.ObtenerAtaque() > mejor.ObtenerAtaque()) mejor = c;
            }
            // Si el bonus es permanente, cualquier criatura sirve
            if (mejor == null && !hechizo.soloHastaFinDeTurno && yo.criaturas.Count > 0)
            {
                mejor = yo.criaturas[0];
            }
            if (mejor != null) return mejor.gameObject;
        }

        return null;   // no conviene lanzarlo ahora
    }

    // =================================================================
    //  DECISIONES: COMBATE
    // =================================================================

    // No ataca si el rival tiene un bloqueador que la mata sin morir
    bool ConvieneAtacar(CardView atacante)
    {
        Jugador rival = Rival();
        for (int i = 0; i < rival.criaturas.Count; i++)
        {
            CardView defensor = rival.criaturas[i];
            if (!defensor.PuedeBloquear(atacante)) continue;

            bool defensorLaMata = defensor.ObtenerAtaque() >= atacante.ObtenerVida();
            bool defensorSobrevive = defensor.ObtenerVida() > atacante.ObtenerAtaque();
            if (defensorLaMata && defensorSobrevive) return false;
        }
        return true;
    }

    // Si puede matar una criatura rival sin morir, la ataca. Si no, al jugador (null).
    CardView ElegirObjetivoDeAtaque(CardView atacante)
    {
        Jugador rival = Rival();
        CardView mejor = null;

        for (int i = 0; i < rival.criaturas.Count; i++)
        {
            CardView c = rival.criaturas[i];
            bool laMato = atacante.ObtenerAtaque() >= c.ObtenerVida();
            bool sobrevivo = c.ObtenerAtaque() < atacante.ObtenerVida();
            if (laMato && sobrevivo)
            {
                if (mejor == null || c.ObtenerAtaque() > mejor.ObtenerAtaque()) mejor = c;
            }
        }
        return mejor;
    }

    // =================================================================
    //  DECISIONES: DEFENSA (cuando el humano ataca)
    // =================================================================
    public void ElegirDefensa(List<Ataque> ataques)
    {
        // Cuánto daño me llega a la cara si no bloqueo nada
        int danioAlJugador = 0;
        for (int i = 0; i < ataques.Count; i++)
        {
            if (ataques[i].objetivo == null) danioAlJugador = danioAlJugador + ataques[i].atacante.ObtenerAtaque();
        }
        bool enPeligro = danioAlJugador >= yo.vida;

        for (int i = 0; i < ataques.Count; i++)
        {
            Ataque ataque = ataques[i];
            CardView atacante = ataque.atacante;
            CardView elegido = null;

            // Busco un bloqueador que sobreviva o que mate al atacante
            for (int j = 0; j < yo.criaturas.Count; j++)
            {
                CardView c = yo.criaturas[j];
                if (!c.PuedeBloquear(atacante)) continue;              // regla de voladoras incluida
                if (Gm().BuscarAtaqueBloqueadoPor(c) != null) continue; // ya bloquea a otro

                bool sobrevive = c.ObtenerVida() > atacante.ObtenerAtaque();
                bool loMata = c.ObtenerAtaque() >= atacante.ObtenerVida();
                if (sobrevive || loMata)
                {
                    elegido = c;
                    break;
                }
            }

            // Si voy a morir, bloqueo con cualquiera ("bloqueo de sacrificio")
            if (elegido == null && enPeligro && ataque.objetivo == null)
            {
                for (int j = 0; j < yo.criaturas.Count; j++)
                {
                    CardView c = yo.criaturas[j];
                    if (c.PuedeBloquear(atacante) && Gm().BuscarAtaqueBloqueadoPor(c) == null)
                    {
                        elegido = c;
                        break;
                    }
                }
            }

            if (elegido != null)
            {
                bool bloqueo = Gm().DeclararBloqueo(elegido, ataque);
                if (bloqueo && ataque.objetivo == null)
                {
                    danioAlJugador = danioAlJugador - atacante.ObtenerAtaque();
                    enPeligro = danioAlJugador >= yo.vida;
                }
            }
        }
    }
}
