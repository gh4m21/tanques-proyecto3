using UnityEngine;
using UnityEngine.UI;

public class TankHealth : MonoBehaviour
{
    public float m_StartingHealth = 100f;   //cantidad de salud  con la que empieza el tanque       
    public Slider m_Slider;      //slider que representa la salud del tanque                  
    public Image m_FillImage;      //componente de imagen del slider                
    public Color m_FullHealthColor = Color.green;  //color del slider con salud completa (verde)
    public Color m_ZeroHealthColor = Color.red;    //color del slider con salud vacia (rojo)
    public GameObject m_ExplosionPrefab; //Prefab que instanciamos al inicio y usamos cuando el tanque se muere
    
    
    private AudioSource m_ExplosionAudio;     //La fuente de audio a reproducir cuando el tanque explota     
    private ParticleSystem m_ExplosionParticles;   //sistema de particulas que se reproducen al destruir el tanque
    private float m_CurrentHealth;  //vriable para almacenar la salud del tanque
    private bool m_Dead;            //variable para comprobar si ell tanque tiene salud


    private void Awake()
    {
        // instanciamos el prefab de la explosion
        m_ExplosionParticles = Instantiate(m_ExplosionPrefab).GetComponent<ParticleSystem>();
        m_ExplosionAudio = m_ExplosionParticles.GetComponent<AudioSource>();

        // referencia de la fuente de audio para la explosion
        m_ExplosionParticles.gameObject.SetActive(false);

        // Deshabilitamos el sistema de particulas de la explosion (para activarlo cuando explote)
        m_ExplosionParticles.gameObject.SetActive(false);
    }


    private void OnEnable()
    {
        // Al habilitar el tanque, reseteamos la salud y el booleano de si esta muerto o no.
        m_CurrentHealth = m_StartingHealth;
        m_Dead = false;

        // Actualizamos el slider de salud (valor y color)
        SetHealthUI();
    }
    

    public void TakeDamage(float amount)
    {
        // reducimos la salud segun la cantidad de dano recibida
        m_CurrentHealth -= amount;

        //Actualizamos el slider de salud con esos valores
        SetHealthUI();

        // si la salud es menor que 0 y aun no lo he explotado, llamo al metodo OnDeath (al morir)
        if(m_CurrentHealth <= 0f && !m_Dead)
        {
            OnDeath();
        }
    }


    private void SetHealthUI()
    {
        // Ajusto el valor del slider
        m_Slider.value = m_CurrentHealth;

        // creo un color para el slider entre verde y rojo en funcion del porcentaje de salud
        m_FillImage.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / m_StartingHealth);
    }


    private void OnDeath()
    {
        // configuro el booleano a true para asegurarme de que explota solo una vez.
        m_Dead = true;

        // coloco el prefab de explosion en la posicion actual del tanque y lo activo.
        m_ExplosionParticles.transform.position = transform.position;
        m_ExplosionParticles.gameObject.SetActive(true);

        // reproduzco el sistema de particulas del tanque explotando
        m_ExplosionParticles.Play();

        // reproduzco el audio del tanque explotando
        m_ExplosionAudio.Play();

        // Desactivvo el tanque
        gameObject.SetActive(false);
    }
}