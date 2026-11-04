using System.Collections.Generic;
using UnityEngine;

// =====================================================================
//  Jugador.cs  -  Un jugador en la partida (vos o la IA).
//  El humano y la IA usan EXACTAMENTE esta misma clase. La única
//  diferencia es que la IA tiene además el script IAJugador que decide
//  por ella.
//
//  Los datos "fijos" (nombre, mazo) vienen del ScriptableObject
//  PlayerData. Acá se guarda lo que cambia durante la partida.
// =====================================================================
public class Jugador : MonoBehaviour
{
    [Header("Datos (ScriptableObject)")]
    public V2PlayerData datos;
    public bool esIA = false;

    [Header("Zonas de la UI")]
    public Transform zonaMano;
    public Transform zonaTierras;
    public Transform zonaCriaturas;
    public FieldZone campo;
    public PlayerPortrait retrato;

    [Header("Estado en la partida (solo para mirar)")]
    public int vida;
    public int tierrasJugadasEsteTurno = 0;
    public List<CardData> mazo = new List<CardData>();
    public List<CardView> mano = new List<CardView>();
    public List<CardView> tierras = new List<CardView>();
    public List<CardView> criaturas = new List<CardView>();
    public List<CardData> cementerio = new List<CardData>();

    // -----------------------------------------------------------------
    //  PREPARAR LA PARTIDA
    // -----------------------------------------------------------------
    public void PrepararPartida(int vidaInicial)
    {
        vida = vidaInicial;
        tierrasJugadasEsteTurno = 0;
        mano.Clear();
        tierras.Clear();
        criaturas.Clear();
        cementerio.Clear();

        // Copiamos las cartas del mazo (SO) a una lista nueva.
        // Así barajamos y robamos SIN modificar el ScriptableObject.
        mazo = datos.mazo.ObtenerListaDeCartas();
        Barajar();
        ActualizarRetrato();
    }

    // Mezcla el mazo (algoritmo de Fisher-Yates):
    // recorro de atrás para adelante y cambio cada carta con una al azar.
    public void Barajar()
    {
        for (int i = mazo.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            CardData temporal = mazo[i];
            mazo[i] = mazo[j];
            mazo[j] = temporal;
        }
    }

    // Roba la primera carta del mazo. Devuelve false si el mazo está vacío.
    public bool RobarCarta()
    {
        if (mazo.Count == 0)
        {
            return false;
        }

        CardData carta = mazo[0];
        mazo.RemoveAt(0);

        CardView vista = Instantiate(GameManager.instancia.prefabCarta, zonaMano);
        vista.Configurar(carta, this, esIA);   // las cartas de la IA se ven de dorso
        vista.zona = ZonaCarta.Mano;
        mano.Add(vista);

        ActualizarRetrato();
        return true;
    }

    // -----------------------------------------------------------------
    //  INICIO DE TURNO: se habilitan todas las cartas
    // -----------------------------------------------------------------
    public void HabilitarTodo()
    {
        for (int i = 0; i < tierras.Count; i++)
        {
            tierras[i].Habilitar();
        }
        for (int i = 0; i < criaturas.Count; i++)
        {
            criaturas[i].Habilitar();
        }
        tierrasJugadasEsteTurno = 0;
        ActualizarRetrato();
    }

    // -----------------------------------------------------------------
    //  MANA
    // -----------------------------------------------------------------
    public int ContarTierrasHabilitadasDeColor(ColorMana color)
    {
        int cantidad = 0;
        for (int i = 0; i < tierras.Count; i++)
        {
            LandCardData tierra = tierras[i].datos as LandCardData;
            if (!tierras[i].deshabilitada && tierra != null && tierra.colorQueProduce == color)
            {
                cantidad++;
            }
        }
        return cantidad;
    }

    public int ContarTierrasHabilitadas()
    {
        int cantidad = 0;
        for (int i = 0; i < tierras.Count; i++)
        {
            if (!tierras[i].deshabilitada) cantidad++;
        }
        return cantidad;
    }

    // ¿Me alcanza el mana para esta carta?
    public bool PuedePagar(CardData carta)
    {
        int sobrante = 0;

        // Primero reviso cada color (hay 5, por eso el 5)
        for (int i = 0; i < 5; i++)
        {
            ColorMana color = (ColorMana)i;
            int disponibles = ContarTierrasHabilitadasDeColor(color);
            int costo = carta.ObtenerCostoDeColor(color);
            if (disponibles < costo)
            {
                return false;   // me falta mana de este color
            }
            sobrante = sobrante + (disponibles - costo);
        }

        // Lo que sobró de todos los colores sirve para el costo genérico
        return sobrante >= carta.costoGenerico;
    }

    // Paga el costo deshabilitando tierras. Llamar SOLO si PuedePagar dio true.
    public void Pagar(CardData carta)
    {
        // 1) Costos de color: deshabilito tierras de ese color exacto
        for (int i = 0; i < 5; i++)
        {
            ColorMana color = (ColorMana)i;
            int falta = carta.ObtenerCostoDeColor(color);
            for (int j = 0; j < tierras.Count; j++)
            {
                if (falta <= 0) break;
                LandCardData tierra = tierras[j].datos as LandCardData;
                if (!tierras[j].deshabilitada && tierra.colorQueProduce == color)
                {
                    tierras[j].Deshabilitar();
                    falta--;
                }
            }
        }

        // 2) Costo genérico: cualquier tierra que siga habilitada
        int faltaGenerico = carta.costoGenerico;
        for (int j = 0; j < tierras.Count; j++)
        {
            if (faltaGenerico <= 0) break;
            if (!tierras[j].deshabilitada)
            {
                tierras[j].Deshabilitar();
                faltaGenerico--;
            }
        }

        ActualizarRetrato();
    }

    // Texto tipo "Mana: R2 V1 B0 N0 A0"
    public string ObtenerTextoMana()
    {
        return "Mana: R" + ContarTierrasHabilitadasDeColor(ColorMana.Rojo)
            + " V" + ContarTierrasHabilitadasDeColor(ColorMana.Verde)
            + " B" + ContarTierrasHabilitadasDeColor(ColorMana.Blanco)
            + " N" + ContarTierrasHabilitadasDeColor(ColorMana.Negro)
            + " A" + ContarTierrasHabilitadasDeColor(ColorMana.Azul);
    }

    // -----------------------------------------------------------------
    //  VIDA Y CRIATURAS
    // -----------------------------------------------------------------
    public void RecibirDanio(int cantidad)
    {
        vida = vida - cantidad;
        ActualizarRetrato();
    }

    // ¿Tengo alguna criatura que pueda bloquear alguno de estos ataques?
    public bool PuedeBloquearAlgo(List<Ataque> ataques)
    {
        for (int i = 0; i < criaturas.Count; i++)
        {
            for (int j = 0; j < ataques.Count; j++)
            {
                if (criaturas[i].PuedeBloquear(ataques[j].atacante))
                {
                    return true;
                }
            }
        }
        return false;
    }

    // Saca del campo las criaturas con vida 0 o menos
    public void QuitarCriaturasMuertas()
    {
        // Recorro de atrás para adelante porque voy borrando de la lista
        for (int i = criaturas.Count - 1; i >= 0; i--)
        {
            if (criaturas[i].EstaMuerta())
            {
                CardView muerta = criaturas[i];
                criaturas.RemoveAt(i);
                cementerio.Add(muerta.datos);
                muerta.zona = ZonaCarta.Cementerio;
                Destroy(muerta.gameObject);
            }
        }
    }

    public void FinDeTurno(bool curar)
    {
        for (int i = 0; i < criaturas.Count; i++)
        {
            criaturas[i].FinDeTurno(curar);
        }
    }

    public void ActualizarRetrato()
    {
        if (retrato != null) retrato.Actualizar();
    }
}
