using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Música por Escena")]
    public AudioClip musicaMenu;
    public AudioClip musicaNivel;

    [Header("Volumen")]
    [Range(0f, 1f)]
    public float volumen = 0.5f;

    private AudioSource audioSource;
    private string escenaActual;

    void Awake()
    {
        // CORREGIDO: Usar la variable estática pública Instance
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        ConfigurarAudioSource();

        // Suscribirse al evento de cambio de escena
        SceneManager.sceneLoaded += OnEscenaCargada;

        // Cargar volumen guardado
        CargarVolumenGuardado();
    }

    void Start()
    {
        ReproducirMusicaParaEscena(SceneManager.GetActiveScene().name);
    }

    private void OnEscenaCargada(Scene escena, LoadSceneMode modo)
    {
        ReproducirMusicaParaEscena(escena.name);
    }

    private void ConfigurarAudioSource()
    {
        audioSource.loop = true;
        audioSource.volume = volumen;
        audioSource.playOnAwake = false;
    }

    private void ReproducirMusicaParaEscena(string nombreEscena)
    {
        AudioClip musicaParaEscena = null;

        // CORREGIDO: Mejor detección de escenas
        if (nombreEscena == "MenuPrincipal" || nombreEscena == "Options")
        {
            musicaParaEscena = musicaMenu;
        }
        else if (nombreEscena.StartsWith("Nivel") || nombreEscena.Contains("MenuANivel"))
        {
            musicaParaEscena = musicaNivel;
        }

        Debug.Log($"🎵 Cambiando música para escena: {nombreEscena} -> {(musicaParaEscena != null ? musicaParaEscena.name : "SILENCIO")}");

        if (musicaParaEscena != null)
        {
            if (audioSource.clip != musicaParaEscena || !audioSource.isPlaying)
            {
                audioSource.clip = musicaParaEscena;
                audioSource.Play();
            }
        }
        else
        {
            audioSource.Stop();
        }
    }

    private void CargarVolumenGuardado()
    {
        if (PlayerPrefs.HasKey("VolumenGlobal"))
        {
            volumen = PlayerPrefs.GetFloat("VolumenGlobal", 0.7f);
            audioSource.volume = volumen;
            AudioListener.volume = volumen;
        }
    }

    public void CambiarVolumenGlobal(float nuevoVolumen)
    {
        volumen = Mathf.Clamp01(nuevoVolumen);
        audioSource.volume = volumen;
        AudioListener.volume = volumen;

        PlayerPrefs.SetFloat("VolumenGlobal", volumen);
        PlayerPrefs.Save();

        Debug.Log($"🔊 Volumen cambiado: {Mathf.RoundToInt(volumen * 100)}%");
    }

    public float ObtenerVolumenActual()
    {
        return volumen;
    }

    public void PausarMusica()
    {
        if (audioSource != null)
        {
            audioSource.Pause();
        }
    }

    public void ReanudarMusica()
    {
        if (audioSource != null)
        {
            audioSource.UnPause();
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnEscenaCargada;
    }
}