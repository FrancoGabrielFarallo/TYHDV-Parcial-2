using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// =====================================================================
//  CreadorDeEjemplos.cs  (OPCIONAL - solo funciona en el editor)
//
//  Crea de una sola vez todos los ScriptableObjects de ejemplo:
//  5 tierras, 18 criaturas/hechizos, la colección, las reglas, los
//  datos de los 2 jugadores y 2 mazos de 60 cartas listos para jugar.
//
//  Uso: menú de arriba  Herramientas > Magic Lite > Crear datos de ejemplo
//  Si un archivo ya existe NO lo pisa, así que podés correrlo de nuevo.
//
//  Tiene que estar dentro de una carpeta llamada "Editor".
// =====================================================================
public static class CreadorDeEjemplos
{
    const string CARPETA = "Assets/Datos";
    const string CARPETA_CARTAS = "Assets/Datos/Cartas";

    [MenuItem("Herramientas/Magic Lite/Crear datos de ejemplo")]
    public static void CrearTodo()
    {
        CrearCarpeta("Assets", "Datos");
        CrearCarpeta(CARPETA, "Cartas");

        // ---------------- TIERRAS ----------------
        LandCardData montania = CrearTierra("Montaña", ColorMana.Rojo);
        LandCardData bosque = CrearTierra("Bosque", ColorMana.Verde);
        LandCardData llanura = CrearTierra("Llanura", ColorMana.Blanco);
        LandCardData pantano = CrearTierra("Pantano", ColorMana.Negro);
        LandCardData isla = CrearTierra("Isla", ColorMana.Azul);

        // ---------------- ROJO ----------------
        // Costo: un número = genérico, R=Rojo V=Verde B=Blanco N=Negro A=Azul
        CardData goblin = CrearCriatura("Goblin Furioso", "R", 2, 1, TipoMovimiento.Tierra);
        CardData lanzallamas = CrearCriatura("Lanzallamas", "2R", 3, 2, TipoMovimiento.Tierra);
        CardData dragon = CrearCriatura("Dragón Joven", "3RR", 4, 4, TipoMovimiento.Voladora);
        CardData rayo = CrearHechizo("Rayo", "R", EfectoHechizo.DanioACriaturaOJugador, 3, 0, 0, true);
        CardData bolaDeFuego = CrearHechizo("Bola de Fuego", "1R", EfectoHechizo.DanioAlJugador, 3, 0, 0, true);

        // ---------------- VERDE ----------------
        CardData oso = CrearCriatura("Oso Pardo", "1V", 2, 2, TipoMovimiento.Tierra);
        CardData lobo = CrearCriatura("Lobo Salvaje", "2V", 3, 3, TipoMovimiento.Tierra);
        CardData gigante = CrearCriatura("Gigante del Bosque", "3VV", 5, 5, TipoMovimiento.Tierra);
        CardData crecimiento = CrearHechizo("Crecimiento Gigante", "V", EfectoHechizo.PotenciarCriatura, 0, 3, 3, true);

        // ---------------- BLANCO ----------------
        CardData soldado = CrearCriatura("Soldado Leal", "B", 1, 2, TipoMovimiento.Tierra);
        CardData pegaso = CrearCriatura("Pegaso", "1B", 2, 1, TipoMovimiento.Voladora);
        CardData angel = CrearCriatura("Ángel Guardián", "3BB", 3, 4, TipoMovimiento.Voladora);
        CardData bendicion = CrearHechizo("Bendición", "1B", EfectoHechizo.PotenciarCriatura, 0, 1, 1, false);

        // ---------------- NEGRO ----------------
        CardData esqueleto = CrearCriatura("Esqueleto", "N", 2, 1, TipoMovimiento.Tierra);
        CardData murcielago = CrearCriatura("Murciélago Vampiro", "1N", 2, 1, TipoMovimiento.Voladora);
        CardData zombi = CrearCriatura("Zombi Hambriento", "2N", 3, 3, TipoMovimiento.Tierra);
        CardData drenar = CrearHechizo("Drenar Vida", "2N", EfectoHechizo.DanioAlJugador, 3, 0, 0, true);
        CardData toqueMortal = CrearHechizo("Toque Mortal", "2N", EfectoHechizo.DanioACriaturaOJugador, 4, 0, 0, true);

        // ---------------- AZUL ----------------
        CardData triton = CrearCriatura("Tritón", "1A", 1, 3, TipoMovimiento.Tierra);
        CardData halcon = CrearCriatura("Halcón de las Nubes", "1A", 2, 2, TipoMovimiento.Voladora);
        CardData serpiente = CrearCriatura("Serpiente Marina", "3AA", 4, 5, TipoMovimiento.Tierra);
        CardData rayoArcano = CrearHechizo("Rayo Arcano", "A", EfectoHechizo.DanioACriaturaOJugador, 2, 0, 0, true);

        // ---------------- COLECCIÓN ----------------
        CardCollection coleccion = CargarOCrear<CardCollection>(CARPETA + "/Coleccion.asset");
        coleccion.todasLasCartas = new List<CardData>()
        {
            montania, bosque, llanura, pantano, isla,
            goblin, lanzallamas, dragon, rayo, bolaDeFuego,
            oso, lobo, gigante, crecimiento,
            soldado, pegaso, angel, bendicion,
            esqueleto, murcielago, zombi, drenar, toqueMortal,
            triton, halcon, serpiente, rayoArcano
        };
        EditorUtility.SetDirty(coleccion);

        // ---------------- REGLAS ----------------
        CargarOCrear<GameRules>(CARPETA + "/ReglasDelJuego.asset");

        // ---------------- MAZOS (60 cartas cada uno) ----------------
        // Jugador: Rojo/Verde = 12 + 12 tierras + 9 cartas x4 = 60
        DeckData mazoJugador = CargarOCrear<DeckData>(CARPETA + "/Mazo_Jugador.asset");
        if (mazoJugador.entradas.Count == 0)
        {
            mazoJugador.nombreMazo = "Rojo-Verde";
            Agregar(mazoJugador, montania, 12);
            Agregar(mazoJugador, bosque, 12);
            Agregar(mazoJugador, goblin, 4);
            Agregar(mazoJugador, lanzallamas, 4);
            Agregar(mazoJugador, dragon, 4);
            Agregar(mazoJugador, rayo, 4);
            Agregar(mazoJugador, bolaDeFuego, 4);
            Agregar(mazoJugador, oso, 4);
            Agregar(mazoJugador, lobo, 4);
            Agregar(mazoJugador, gigante, 4);
            Agregar(mazoJugador, crecimiento, 4);
            EditorUtility.SetDirty(mazoJugador);
        }

        // IA: Blanco/Negro (con muchas voladoras)
        DeckData mazoIA = CargarOCrear<DeckData>(CARPETA + "/Mazo_IA.asset");
        if (mazoIA.entradas.Count == 0)
        {
            mazoIA.nombreMazo = "Blanco-Negro";
            Agregar(mazoIA, llanura, 12);
            Agregar(mazoIA, pantano, 12);
            Agregar(mazoIA, soldado, 4);
            Agregar(mazoIA, pegaso, 4);
            Agregar(mazoIA, angel, 4);
            Agregar(mazoIA, bendicion, 4);
            Agregar(mazoIA, esqueleto, 4);
            Agregar(mazoIA, murcielago, 4);
            Agregar(mazoIA, zombi, 4);
            Agregar(mazoIA, drenar, 4);
            Agregar(mazoIA, toqueMortal, 4);
            EditorUtility.SetDirty(mazoIA);
        }

        // ---------------- JUGADORES ----------------
        V2PlayerData datosJugador = CargarOCrear<V2PlayerData>(CARPETA + "/Jugador_Humano.asset");
        datosJugador.nombreJugador = "Vos";
        datosJugador.mazo = mazoJugador;
        EditorUtility.SetDirty(datosJugador);

        V2PlayerData datosIA = CargarOCrear<V2PlayerData>(CARPETA + "/Jugador_IA.asset");
        datosIA.nombreJugador = "IA";
        datosIA.mazo = mazoIA;
        EditorUtility.SetDirty(datosIA);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Datos de ejemplo creados en " + CARPETA);
    }

    // =================================================================
    //  AYUDANTES
    // =================================================================
    static void CrearCarpeta(string padre, string nombre)
    {
        if (!AssetDatabase.IsValidFolder(padre + "/" + nombre))
        {
            AssetDatabase.CreateFolder(padre, nombre);
        }
    }

    // Si el asset existe lo carga, si no lo crea
    static T CargarOCrear<T>(string ruta) where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(ruta);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, ruta);
        }
        return asset;
    }

    static LandCardData CrearTierra(string nombre, ColorMana color)
    {
        string ruta = CARPETA_CARTAS + "/Tierra_" + nombre + ".asset";
        LandCardData carta = AssetDatabase.LoadAssetAtPath<LandCardData>(ruta);
        if (carta != null) return carta;

        carta = ScriptableObject.CreateInstance<LandCardData>();
        carta.nombre = nombre;
        carta.colorQueProduce = color;
        AssetDatabase.CreateAsset(carta, ruta);
        return carta;
    }

    static CardData CrearCriatura(string nombre, string costo, int ataque, int vida, TipoMovimiento movimiento)
    {
        string ruta = CARPETA_CARTAS + "/Criatura_" + nombre + ".asset";
        CreatureCardData carta = AssetDatabase.LoadAssetAtPath<CreatureCardData>(ruta);
        if (carta != null) return carta;

        carta = ScriptableObject.CreateInstance<CreatureCardData>();
        carta.nombre = nombre;
        carta.ataque = ataque;
        carta.vida = vida;
        carta.movimiento = movimiento;
        PonerCosto(carta, costo);
        AssetDatabase.CreateAsset(carta, ruta);
        return carta;
    }

    static CardData CrearHechizo(string nombre, string costo, EfectoHechizo efecto, int danio, int bonusAtaque, int bonusVida, bool temporal)
    {
        string ruta = CARPETA_CARTAS + "/Hechizo_" + nombre + ".asset";
        SpellCardData carta = AssetDatabase.LoadAssetAtPath<SpellCardData>(ruta);
        if (carta != null) return carta;

        carta = ScriptableObject.CreateInstance<SpellCardData>();
        carta.nombre = nombre;
        carta.efecto = efecto;
        carta.cantidadDanio = danio;
        carta.bonusAtaque = bonusAtaque;
        carta.bonusVida = bonusVida;
        carta.soloHastaFinDeTurno = temporal;
        PonerCosto(carta, costo);
        AssetDatabase.CreateAsset(carta, ruta);
        return carta;
    }

    // Lee un texto como "2RR" y lo convierte en el costo de la carta
    static void PonerCosto(CardData carta, string costo)
    {
        for (int i = 0; i < costo.Length; i++)
        {
            char letra = costo[i];
            if (char.IsDigit(letra)) carta.costoGenerico = carta.costoGenerico * 10 + (letra - '0');
            else if (letra == 'R') carta.costoRojo++;
            else if (letra == 'V') carta.costoVerde++;
            else if (letra == 'B') carta.costoBlanco++;
            else if (letra == 'N') carta.costoNegro++;
            else if (letra == 'A') carta.costoAzul++;
        }
    }

    static void Agregar(DeckData mazo, CardData carta, int cantidad)
    {
        EntradaMazo entrada = new EntradaMazo();
        entrada.carta = carta;
        entrada.cantidad = cantidad;
        mazo.entradas.Add(entrada);
    }
}
