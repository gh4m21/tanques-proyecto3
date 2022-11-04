using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int m_NumRoundsToWin = 5;        //numero de rondas que un jugador debe ganar para ganar el juego
    public float m_StartDelay = 3f;         //delay entre las fases de roundstarting y roundplaying
    public float m_EndDelay = 3f;           //delay entre las fases de roundplaying y roundending
    public CameraControl m_CameraControl;   //referencia al script de camera control
    public Text m_MessageText;              //referencia al texto para mostrar mensajes
    public GameObject m_TankPrefab;         //referencia al prefab del tanque
    public TankManager[] m_Tanks;           //array de tankmanagers para controlar cada tanque
    public AudioSource musicaInicio;//referencia la musica de inicio

    private int m_RoundNumber;    //numero de ronda          
    private WaitForSeconds m_StartWait;     //delay hasta que la ronda empieza
    private WaitForSeconds m_EndWait;       //delay hasta que la ronda acaba
    private TankManager m_RoundWinner;      //referencia al ganador de la ronda para anunciar quien ha ganado
    private TankManager m_GameWinner;       //referencia al ganador del juego para anunciar quien ha ganado


    private void Start()
    {
        // creamos los delays para que solo se apliquen una vez
        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);

        SpawnAllTanks();//generar tanques
        SetCameraTargets();//ajustar camara

        StartCoroutine(GameLoop());//iniciar juego
    }


    private void SpawnAllTanks()
    {
        // recorro los tanques...
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            //... los creo, ajusto el numero de jugador y las referencias necesarias para controlarlo
            m_Tanks[i].m_Instance =
                Instantiate(m_TankPrefab, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
            m_Tanks[i].m_PlayerNumber = i + 1;
            m_Tanks[i].Setup();
        }
    }


    private void SetCameraTargets()
    {
        // creo un array de transform del mismo tamano que el numero de tanques
        Transform[] targets = new Transform[m_Tanks.Length];

        // recorro los transforms...
        for (int i = 0; i < targets.Length; i++)
        {
            //... lo ajusto al transform del tanque apropiado
            targets[i] = m_Tanks[i].m_Instance.transform;
        }

        // ... estos son los targets que la camara debe seguir
        m_CameraControl.m_Targets = targets;
    }

    // llamado al principio y en cada fase del juego despues de otra
    private IEnumerator GameLoop()
    {
        
        // empiezo con la corutina roundstarting y no retorno hasta que finalice
        yield return StartCoroutine(RoundStarting());

        // cuando finalice roundstarting, empiezo con roundplaying y no retorno hasta que finalice
        yield return StartCoroutine(RoundPlaying());

        // cuando finalice roundplaying, empiezo con roundending y no retorno hasta que finalice
        yield return StartCoroutine(RoundEnding());

        // si aun no ha ganado ninguno
        if (m_GameWinner != null)
        {
            // si hay un ganador, reinicio el nivel
            SceneManager.LoadScene(0);
        }
        else
        {
            // si ni, reinicio las corutinas para que continue el bucle, en este caso, sin yiendo, de modo que esta version del gameloop finalizara siempre.
            StartCoroutine(GameLoop());
        }
    }


    private IEnumerator RoundStarting()
    {
         // musica de inicio
        musicaInicio.Play();

        // cuando empiece la ronda reseteo los tanques e impido que se muevan.
        ResetAllTanks();
        DisableTankControl();
        // Ajusto la camara a los tanques resteteados.
        m_CameraControl.SetStartPositionAndSize();

        // Incremento la ronda y muestro el texto informativo.
        m_RoundNumber++;
        m_MessageText.text = "ROUND " + m_RoundNumber;

        // Espero a que pase el tiempo de espera antes de volver al bucle..
        yield return m_StartWait;
    }


    private IEnumerator RoundPlaying()
    {
        // stop musica de inicio
        musicaInicio.Stop();

        // Cuando empiece la ronda dejo que los tanques se muevan.
        EnableTankControl();

        // Borro el texto de la pantalla
        m_MessageText.text = string.Empty;

        // Mientras haya mas de un tanque...
        while(!OneTankLeft())
        {
            // ... vuelvo al frame siguiente
            yield return null;
        }
    }


    private IEnumerator RoundEnding()
    {
        // deshabilito el movimiento de los tanques
        DisableTankControl();

        // borro al ganador de la ronda anterior
        m_RoundWinner = null;

        // Miro si hay un ganador de la ronda
        m_RoundWinner = GetRoundWinner();

        // si lo hay, incremento su puntuacion
        if(m_RoundWinner != null)
            m_RoundWinner.m_Wins++;
        
        // compruebo si alguien ha ganado el juego
        m_GameWinner = GetGameWinner();

        // Genero el mensaje segun si hay un ganador del juego o no.
        string message = EndMessage();
        m_MessageText.text = message;

        // Espero a que pase el tiempo de espera anes de volver al bucle
        yield return m_EndWait;
    }

    // usado para comprobar si queda mas de un tanque
    private bool OneTankLeft()
    {
        // contador de tanques
        int numTanksLeft = 0;

        // recorro los tanques
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            // ... si esta activo, incremento el contador
            if (m_Tanks[i].m_Instance.activeSelf)
                numTanksLeft++;
        }

        // devuelvo true si queda 1 o menos, false si queda mas de uno
        return numTanksLeft <= 1;
    }

    // comprueba si algun tanque ha ganado la ronda (si queda un tanque o menos)
    private TankManager GetRoundWinner()
    {
        // recorro los tanques...
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            // ... si solo queda uno, es el ganador y lo devuelvo
            if (m_Tanks[i].m_Instance.activeSelf)
                return m_Tanks[i];
        }

        // si no hay ninguno activo es un empate, asi que devuelvo null
        return null;
    }

    // Comprueba si hay algun ganador del juego
    private TankManager GetGameWinner()
    {
        // recorro los tanques...
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            // ... si alguno tiene las rondas necesarias, ha ganado y lo devuelvo
            if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                return m_Tanks[i];
        }

        // si no, deveulvo null
        return null;
    }

    // Devuelve el texto del mensaje a mostrar al final de cada ronda
    private string EndMessage()
    {
        // Por defecto no hay ganadores, asi que es empate
        string message = "EMPATE!";

        // si hay un ganador de ronda cambio el mensaje.
        if (m_RoundWinner != null)
            message = m_RoundWinner.m_ColoredPlayerText + " GANA LA RONDA!";

        // retornos de carro.
        message += "\n\n\n\n";

        // recorro los tanques y anado sus puntuaciones
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " GANA\n";
        }

        // si no hay un ganador del juego, cambio el mensaje entero para reflejarlo
        if (m_GameWinner != null)
        {
            message = m_GameWinner.m_ColoredPlayerText + " GANA EL JUEGO!";

            // retornos de carro.
            message += "\n\n\n\n";

            // recorro los tanques y anado sus puntuaciones
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " GANA\n";
            } 
        }
            
        return message;
    }

    // para resetear los tanques (propiedades, posciones, etc.)
    private void ResetAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].Reset();
        }
    }

    // habilita el control del tanque
    private void EnableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].EnableControl();
        }
    }

    // Deshabilita el control del tanque
    private void DisableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].DisableControl();
        }
    }
}