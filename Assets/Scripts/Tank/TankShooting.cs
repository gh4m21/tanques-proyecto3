using UnityEngine;
using UnityEngine.UI;

public class TankShooting : MonoBehaviour
{
    public int m_PlayerNumber = 1;       //Para identificar a los diferentes jugadores
    public Rigidbody m_Shell;            //prefab de la bomba
    public Transform m_FireTransform;    //hijo del tanque en el que se generara la bomba (desde donde se lanzara)
    public Slider m_AimSlider;           //hijo del tanque que muestra la fuerza de lanzamiento de la bomba
    public AudioSource m_ShootingAudio;  //referencia a la fuente de audio que se reproducira al lanzar la bomba
    public AudioClip m_ChargingClip;     //clip de audio que se reproduce cuando se esta cargando el disparo
    public AudioClip m_FireClip;         //clip de audio que se reproduce al lanzar la bomba
    public float m_MinLaunchForce = 15f; //fuerza minima de disparo (si no mantiene presionado el boton de disparo)
    public float m_MaxLaunchForce = 30f; //fuerza maxima de disparo(si se mantiene presionado el boton de disparo)
    public float m_MaxChargeTime = 0.75f;//tiempo maximo de carga antes de ser lanzado el disparo con maxima fuerza

    
    private string m_FireButton;    //eje de disparo utilizado para lanzar las bombas     
    private float m_CurrentLaunchForce;  //fuerza dada a la bomba cuando la se suelta el boton de disparo
    private float m_ChargeSpeed;         //velocidad de carga, basada en el maximo tiempo de cargo
    private bool m_Fired;                //booleano que comprueba si se ha lanzado la bomba


    private void OnEnable()
    {
        // Al crear el tanque, reseteo la fuerza de lanzamiento y la UI
        m_CurrentLaunchForce = m_MinLaunchForce;
        m_AimSlider.value = m_MinLaunchForce;
    }


    private void Start()
    {
        // el eje de disparo basado en el numero de jugador
        m_FireButton = "Fire" + m_PlayerNumber;

        // velocidad de carga, basada en el maximo tiempo de carga y los valores de carga maximo y minimo
        m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;
    }
    

    private void Update()
    {
        // Asigno el valor maximo y no lo he lanzado
        m_AimSlider.value = m_MinLaunchForce;

        // si llego al valor maximo y no lo he lanzado...
        if(m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
        {
            // ... uso el valor maximo y disparo
            m_CurrentLaunchForce = m_MaxLaunchForce;
            Fire();
        }
        // si no, si ya he pulsado el boton de disparo...
        else if(Input.GetButtonDown(m_FireButton))
        {
            // ... reseteo el booleano de disparo y la fuerza de disparo
            m_Fired = false;
            m_CurrentLaunchForce = m_MinLaunchForce;

            // cambio el clip de audio al de cargando y lo reproduzco
            m_ShootingAudio.clip = m_ChargingClip;
            m_ShootingAudio.Play();
        }
        // Si no, si estoy manteniendo presionado el boton de disparo y aun no he disparado...
        else if(Input.GetButton(m_FireButton) && !m_Fired)
        {
            //incremento la fuerza de disparo y actualizo el slider
            m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;

            m_AimSlider.value = m_CurrentLaunchForce;
        }
        // si no, su ya he soltado el boton de disparo y aun no he lanzado...
        else if(Input.GetButtonUp(m_FireButton) && !m_Fired)
        {
            //... disparo
            Fire();
        }
    }


    private void Fire()
    {
        // Iajusto el booleano a true para que solo se lance una vez.
        m_Fired = true;

        // creo una instancia de la bomba y guardo una referencia en su rigidbody
        Rigidbody shellInstance = Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;

        // ajusto la velocidad de la bomba en la direccion de disparo
        shellInstance.velocity = m_CurrentLaunchForce * m_FireTransform.forward;

        // cambio el audio al de disparo y lo reproduzco
        m_ShootingAudio.clip = m_FireClip;
        m_ShootingAudio.Play();

        // Reseteo la fuerza de lanzamiento como precaucion ante posibles eventos de boton "perdidos"
        m_CurrentLaunchForce = m_MinLaunchForce;
    }
}