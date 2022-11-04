using System;
using UnityEngine;

[Serializable] //hace que los atributos aparezcan en el inspector (si no los escondemos)
public class TankManager
{
    // esta clase gestiona la configuracion del tanque junto con el gamemanager.
    //Gestiona el comportamiento de los tanques y si los jugadores tienen control sobre el tanque en los distintos momentos del juego
    public Color m_PlayerColor;      //color para el tanque      
    public Transform m_SpawnPoint;    //posicion y direccion en la que se generara el tanque     
    [HideInInspector] public int m_PlayerNumber;   //especifica con que jugador esta actuando el game manager          
    [HideInInspector] public string m_ColoredPlayerText;  //string que representa el color del tanque
    [HideInInspector] public GameObject m_Instance;          //referencia a la instancia del tanque cuando se crea
    [HideInInspector] public int m_Wins;                     //numero de victorias del jugador


    private TankMovement m_Movement;       //referencia al script de movimiento del tanque. Utilizado para deshabilitar y habilitar el control
    private TankShooting m_Shooting;//referencia al script de disparo del tanque. utilizado para deshabilitar y habilitar el control
    private GameObject m_CanvasGameObject;//utilizado  para deshabilitar el UI del mundo durante las fases de inicio y fin de cada ronda


    public void Setup()
    {
        // cojo de referencia de los componentes
        m_Movement = m_Instance.GetComponent<TankMovement>();
        m_Shooting = m_Instance.GetComponent<TankShooting>();
        m_CanvasGameObject = m_Instance.GetComponentInChildren<Canvas>().gameObject;

        // Ajusto los numeros de jugadores para que sean iguales en todos los scripts
        m_Movement.m_PlayerNumber = m_PlayerNumber;
        m_Shooting.m_PlayerNumber = m_PlayerNumber;

        // creo un string usando el color del tanque que diga PLAYER 1,etc.
        m_ColoredPlayerText = "<color=#" + ColorUtility.ToHtmlStringRGB(m_PlayerColor) + ">PLAYER " + m_PlayerNumber + "</color>";

        // cojo todos los renders del tanque
        MeshRenderer[] renderers = m_Instance.GetComponentsInChildren<MeshRenderer>();

        // Los recorro...
        for (int i = 0; i < renderers.Length; i++)
        {
            // ... y ajusto el color del material al del tanque
            renderers[i].material.color = m_PlayerColor;
        }
    }

    // Usado durante la fases del juego en las que el jugador no debe poder controlar el tanque
    public void DisableControl()
    {
        m_Movement.enabled = false;
        m_Shooting.enabled = false;

        m_CanvasGameObject.SetActive(false);
    }

    // usadon durante la fases del juego en las que el jugador debe poder controlar el tanque
    public void EnableControl()
    {
        m_Movement.enabled = true;
        m_Shooting.enabled = true;

        m_CanvasGameObject.SetActive(true);
    }

    // usado al inicio de cada ronda para poner el tanque en su estado inicial
    public void Reset()
    {
        m_Instance.transform.position = m_SpawnPoint.position;
        m_Instance.transform.rotation = m_SpawnPoint.rotation;

        m_Instance.SetActive(false);
        m_Instance.SetActive(true);
    }
}
