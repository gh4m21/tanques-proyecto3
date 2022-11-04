using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float m_DampTime = 0.2f;      //Tiempo de espera para mover la camara

    public float m_ScreenEdgeBuffer = 4f;    //Pequeno padding para que los tanques no se pegen a los border

    public float m_MinSize = 6.5f;   //Tamano minimo de zoom

   [HideInInspector] public Transform[] m_Targets; //Array de tanques, no se mostraran en el inspector cuando haya Game Manager 


    private Camera m_Camera;       //La camara                 
    private float m_ZoomSpeed;      //Velocidad de zoom                      
    private Vector3 m_MoveVelocity;  //velocidad de movimiento               
    private Vector3 m_DesiredPosition;   //posicion a la que quiero llegar           


    private void Awake()
    {
        // Al arrancar cogemos la camara
        m_Camera = GetComponentInChildren<Camera>();
    }


    private void FixedUpdate()
    {
        Move(); //Mueve la camara
        Zoom(); //Ajusta el tamano de la camara
    }


    private void Move()
    {
        // Busco la posicion intermedia entre los tanques
        FindAveragePosition();

        // Muevo la camara de forma suave
        transform.position = Vector3.SmoothDamp(transform.position, m_DesiredPosition, ref m_MoveVelocity, m_DampTime);
    }


    private void FindAveragePosition()
    {
        Vector3 averagePos = new Vector3();
        int numTargets = 0;

        // Recorre la cantidad de tanques activos, captura su posicion y asigna m_DesirePosition el puunto medio entre ellos (en el eje Y)
        for (int i = 0; i < m_Targets.Length; i++)
        {
            if (!m_Targets[i].gameObject.activeSelf)
                continue;

            // Incremento el valor a la media y el numero de elementos
            averagePos += m_Targets[i].position;
            numTargets++;
        }

        // Si hay elementos, hago la media
        if (numTargets > 0)
            averagePos /= numTargets;

        // Mantengo el valor de y
        averagePos.y = transform.position.y;

        // La posicion deseada es la media
        m_DesiredPosition = averagePos;
    }


    private void Zoom()
    {
        // Buscamos la posicion requerida de zoom (size) y la asignamos a la camara
        float requiredSize = FindRequiredSize();
        // Ajusto el tamano de la camara de forma suave
        m_Camera.orthographicSize = Mathf.SmoothDamp(m_Camera.orthographicSize, requiredSize, ref m_ZoomSpeed, m_DampTime);
    }


    private float FindRequiredSize()
    {
        // Teniendo en cuenta la posicion deseada
        Vector3 desiredLocalPos = transform.InverseTransformPoint(m_DesiredPosition);

        float size = 0f;

        // Recorremos los tanques activos y cojemos la posicion mas alta(el que estaria mas lejos del centro)
        for (int i = 0; i < m_Targets.Length; i++)
        {
            // Si no esta activo, me lo salto
            if (!m_Targets[i].gameObject.activeSelf)
                continue;

            // posicion del tanque en el espacio de la camara
            Vector3 targetLocalPos = transform.InverseTransformPoint(m_Targets[i].position);

            // diferencia entre la deseada y la actual
            Vector3 desiredPosToTarget = targetLocalPos - desiredLocalPos;

            // escojo el maximo entre el tamano de camara actual y la distancia del tanque (arriba o abajo)
            size = Mathf.Max (size, Mathf.Abs (desiredPosToTarget.y));

            // escojo el maximo entre el tamano de camara actual y la distancia del tanque (izquierda o derecha)
            size = Mathf.Max (size, Mathf.Abs (desiredPosToTarget.x) / m_Camera.aspect);
        }
        
        // Aplicamos el padding
        size += m_ScreenEdgeBuffer;

        // Comprobamos que al menos tenemos el zoom minimo
        size = Mathf.Max(size, m_MinSize);

        return size;
    }

    // La usaremos en el Game Manager para resetear la posicion y el zoom en cada escena
    public void SetStartPositionAndSize()
    {
        // Buscamos la posicion deseada
        FindAveragePosition();

        // Ajustamos la posicion de la camara (sin damping porque va a ser al entrar)
        transform.position = m_DesiredPosition;

        // buscamos y ajustamos el tamano de la camara
        m_Camera.orthographicSize = FindRequiredSize();
    }
}