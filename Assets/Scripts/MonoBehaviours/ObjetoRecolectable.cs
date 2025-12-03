using UnityEngine;

public class ObjetoRecolectable : MonoBehaviour
{
    [Header("Configuración del Objeto")]
    [Tooltip("Puntos que otorga este objeto")]
    public int puntos = 10;

    [Tooltip("¿Este objeto cuenta para el contador de 'Objetos recogidos'?")]
    public bool contarComoObjetoRecogido = true;

    [Header("Efectos Opcionales")]
    public AudioClip sonidoRecoleccion;
    public GameObject efectoParticulas;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica si el objeto que colisionó tiene el tag "Player"
        if (other.CompareTag("Player"))
        {
            RecogerObjeto();
        }
    }

    void RecogerObjeto()
    {
        // Notificar al GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RecogerObjeto(puntos, contarComoObjetoRecogido);
        }

        // Reproducir sonido si existe
        if (sonidoRecoleccion != null && audioSource != null)
        {
            AudioSource.PlayClipAtPoint(sonidoRecoleccion, transform.position);
        }

        // Crear efecto de partículas si existe
        if (efectoParticulas != null)
        {
            Instantiate(efectoParticulas, transform.position, Quaternion.identity);
        }

        // Destruir el objeto
        Destroy(gameObject);
    }
}