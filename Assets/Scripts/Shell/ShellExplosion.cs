using UnityEngine;

public class ShellExplosion : MonoBehaviour
{
    public LayerMask m_TankMask; //usado para filtrar a que afecta la explosion de la bomba. Deberia ajustarse a "Players"
    public ParticleSystem m_ExplosionParticles;       //referencia a las particulas que se reproducira en la explosion
    public AudioSource m_ExplosionAudio;      //referencia al audio que se reproducira en la explosion        
    public float m_MaxDamage = 100f;          //cantidad de dano si la explosion esta centrada en el tanque        
    public float m_ExplosionForce = 1000f;    //cantidad de fuerza anadida al tanque en el centro de la explosion        
    public float m_MaxLifeTime = 2f;          //tiempo de vida en segundo de la bomba
    public float m_ExplosionRadius = 5f;      //radio maximo desde la explosion para calcular los tanques que se veran afectadoos


    private void Start()
    {
        // si no ha se ha deestruido aun, destruir la bomba despues de su tiempo de vida
        Destroy(gameObject, m_MaxLifeTime);
    }


    private void OnTriggerEnter(Collider other)
    {
        // recoge los colliders en una esfera desde la position de la bomba con el radio maximo
        Collider[] colliders = Physics.OverlapSphere(transform.position, m_ExplosionRadius, m_TankMask);

        // recorro los colliders
        for (int i=0; i < colliders.Length; i++)
        {
            // selcciono su rigidbody
            Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody>();

            // si no tiene, paso al siguiente.
            if(!targetRigidbody)
                continue;

            // anando la fuerza de la explosion
            targetRigidbody.AddExplosionForce(m_ExplosionForce, transform.position, m_ExplosionRadius);

            // busco el script TankHealth asociado con el rigidbody
            TankHealth targetHealth = targetRigidbody.GetComponent<TankHealth>();

            // si no hay script Tankhealth, paso al siguiente
            if(!targetHealth)
                continue;
            
            // calculo el dano a aplicar en funcion de la distancia a la bomba
            float damage = CalculateDamage(targetRigidbody.position);

            // aplico el dano al tanque
            targetHealth.TakeDamage(damage);
        }

        // desacnlo el sistema de particulas de la bomba
        m_ExplosionParticles.transform.parent = null;

        // reproduzco el sistema de particulas
        m_ExplosionParticles.Play();

        // reproduzco el audio
        m_ExplosionAudio.Play();

        // cuando las particulas han terminado, destruyo su objeto asociado
        Destroy(m_ExplosionParticles.gameObject, m_ExplosionParticles.main.duration);

        // destruyo la bomba
        Destroy(gameObject);
    }


    private float CalculateDamage(Vector3 targetPosition)
    {
        // Creo un vector desde la bomba al objectivo
        Vector3 explosionToTarget = targetPosition - transform.position;

        // calculo la distancia desde la bomba al objetivo
        float explosionDistance = explosionToTarget.magnitude;

        // calculo la proporcion de maxima distancia (radio maximo) desde la explosion al tanque
        float relativeDistance = (m_ExplosionRadius - explosionDistance) / m_ExplosionRadius;

        // calculo el dano a esa proporcion
        float damage = relativeDistance * m_MaxDamage;

        // me aseguro de que el minimo dno siempre es 0
        damage = Mathf.Max(0f,damage);

        // devuelve el dano
        return damage;
    }
}