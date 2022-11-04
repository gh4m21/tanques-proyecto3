using UnityEngine;

public class TankMovement : MonoBehaviour
{
    public int m_PlayerNumber = 1;     //Utilizado para identificar  que tanque es cada jugador. Configurado por el tank's manager.

    public float m_Speed = 12f;        //rapidez con la que el tanque se mueve alante y atras.

    public float m_TurnSpeed = 180f;   // Rapidez de giro del tanque en grados por segundos.

    public AudioSource m_MovementAudio;    //Referencia al audio del motor del tanque (diferente del disparo)

    public AudioClip m_EngineIdling;    //Audio a reproducir cuando el tanque no se mueve

    public AudioClip m_EngineDriving;      //Audio a reproducir cuando el tanque se esta moviendo

    public float m_PitchRange = 0.2f; //Cantidad de variacion de afinacion del audio del motor (para que seaa mas real)

    private string m_MovementAxisName;   //Nombre del eje para moverse alante y atras

    private string m_TurnAxisName;       //Nombre del eje para girar

    private Rigidbody m_Rigidbody;     //Referencia del componente para mover el tanque

    private float m_MovementInputValue;   //valor actural de entrada para el movimiento

    private float m_TurnInputValue;        //valor actual de entrada para el giro

    private float m_OriginalPitch;        //valor del pitch de la fuente de audio al inicio 


    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }


    private void OnEnable ()
    {
        //Al arrancar/habilitar el tanque, deshabilitamos la kinematica del tanque para que se pueda mover
        m_Rigidbody.isKinematic = false;
        m_MovementInputValue = 0f;
        m_TurnInputValue = 0f;
    }


    private void OnDisable ()
    {
        //All parar/deshabilitar el tanque, habilitamos la kinematica del tanque para que se pare.
        m_Rigidbody.isKinematic = true;
    }


    private void Start()
    {
        //Nombres de los ejes segun el numero de jugador
        m_MovementAxisName = "Vertical" + m_PlayerNumber;
        m_TurnAxisName = "Horizontal" + m_PlayerNumber;

        //Almaceno la afinacion original del audio del motor
        m_OriginalPitch = m_MovementAudio.pitch;
    }
    

    private void Update()
    {
        // Almaceno los valores de entrada
        m_MovementInputValue = Input.GetAxis(m_MovementAxisName);
        m_TurnInputValue = Input.GetAxis(m_TurnAxisName);

        //Llamo a la funcion que gestiona el audio del motor
        EngineAudio();
    }


    private void EngineAudio()
    {
        // Si no hay entrada, es que esta quieto...
        if(Mathf.Abs(m_MovementInputValue) < 0.1f && Mathf.Abs(m_TurnInputValue) < 0.1f)
        {
            //... y si estaba reproduciendo el audio de moverse...
            if (m_MovementAudio.clip == m_EngineDriving)
            {
                // ... cambio el audio al de estar parado y lo reproduzco
                m_MovementAudio.clip = m_EngineIdling;
                m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                m_MovementAudio.Play();
            }
        }
        else
        {
            // Si hay entrada es que esta moviendo. Si estaba reproduciendo el de estar parado
            if(m_MovementAudio.clip == m_EngineIdling)
            {
                // ... cambio el audio al de moverse y lo reproduzco
                m_MovementAudio.clip = m_EngineDriving;
                m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                m_MovementAudio.Play();

            }
        }
    }


    private void FixedUpdate()
    {
        // Ajusto la posicion y rotacion
        Move();
        Turn();
    }


    private void Move()
    {
        // Creo un vector en la direccion en la que apunta el tanque, con una magnitud basada en la entrada, la velocidad y el timepo en tres frames.
        Vector3 movement = transform.forward * m_MovementInputValue * m_Speed * Time.deltaTime;

        // Aplico ese vector de movimientoo al rigidbody del tanque
        m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
    }


    private void Turn()
    {
        // calculo el numero de grados de rotacion basandome en la entrada, la velocidad y el timepo entre frames.
        float turn = m_TurnInputValue * m_TurnSpeed * Time.deltaTime;

        // Convirtio este numero en una rotacion en el eje Y.
        Quaternion turnRotation = Quaternion.Euler(0f,turn,0f);

        // Aplico esa rotacion al rigidbody del tanque
        m_Rigidbody.MoveRotation(m_Rigidbody.rotation * turnRotation);
    }
}