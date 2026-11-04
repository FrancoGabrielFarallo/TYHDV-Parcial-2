// =====================================================================
//  Enums.cs
//  Acá guardamos todas las "listas de opciones" que usa el juego.
//  Un enum es una variable que solo puede tener uno de los valores
//  que escribimos adentro. Es más seguro que usar strings o números.
// =====================================================================

// Los 5 colores de mana
public enum ColorMana
{
    Rojo,
    Verde,
    Blanco,
    Negro,
    Azul
}

// Los 3 tipos de carta que existen en el juego
public enum TipoCarta
{
    Tierra,
    Criatura,
    Hechizo
}

// Una criatura puede ser de tierra o voladora
public enum TipoMovimiento
{
    Tierra,
    Voladora
}

// Los 3 efectos que puede tener un hechizo
public enum EfectoHechizo
{
    DanioAlJugador,            // X de daño al jugador rival
    DanioACriaturaOJugador,    // X de daño a una criatura o a un jugador
    PotenciarCriatura          // +X/+X a una criatura
}

// Las fases de un turno, en orden
public enum Fase
{
    IniciarTurno,
    BajarCartas,
    Combate,
    Defensa,
    FinalizarTurno,
    FinDelJuego
}

// Dónde está una carta en este momento
public enum ZonaCarta
{
    Mano,
    Campo,
    Cementerio,
    Coleccion   // se usa en la escena del Deck Builder
}
