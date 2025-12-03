using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioManagerSimple : MonoBehaviour
{
    [Header("Referencias UI")]
    public Slider volumenSlider;
    public TextMeshProUGUI textoVolumen;

    [Header("Configuración")]
    private const string VOLUMEN_KEY = "VolumenGlobal";
    private float volumenActual = 0.7f;
    private bool uiConfigurada = false;

    void Start()
    {
        InicializarAudioManager();
    }

    void OnEnable()
    {
        // Reconfigurar cuando el objeto o panel se active
        if (uiConfigurada)
        {
            ActualizarUI();
        }
        else
        {
            InicializarAudioManager();
        }
    }

    void InicializarAudioManager()
    {
        // Cargar volumen guardado
        CargarVolumen();

        // Configurar UI si las referencias están asignadas
        if (volumenSlider != null && textoVolumen != null)
        {
            ConfigurarUI();
            uiConfigurada = true;
            Debug.Log("✅ AudioManagerSimple inicializado correctamente");
        }
        else
        {
            Debug.LogWarning("⚠️ AudioManagerSimple: Faltan referencias UI, buscando automáticamente...");
            BuscarReferenciasUI();
        }
    }

    void BuscarReferenciasUI()
    {
        // Buscar Slider en el mismo GameObject o hijos
        if (volumenSlider == null)
        {
            volumenSlider = GetComponentInChildren<Slider>();
            if (volumenSlider != null)
                Debug.Log("✅ Slider encontrado automáticamente");
        }

        // Buscar Texto en el mismo GameObject o hijos
        if (textoVolumen == null)
        {
            textoVolumen = GetComponentInChildren<TextMeshProUGUI>();
            if (textoVolumen != null)
                Debug.Log("✅ TextoVolumen encontrado automáticamente");
        }

        // Si encontramos ambas referencias, configurar
        if (volumenSlider != null && textoVolumen != null)
        {
            ConfigurarUI();
            uiConfigurada = true;
        }
        else
        {
            Debug.LogError("❌ AudioManagerSimple: No se pudieron encontrar las referencias UI automáticamente");
        }
    }

    void ConfigurarUI()
    {
        // Remover listeners antiguos para evitar duplicados
        if (volumenSlider != null)
        {
            volumenSlider.onValueChanged.RemoveAllListeners();
            volumenSlider.value = volumenActual;
            volumenSlider.onValueChanged.AddListener(OnSliderCambiado);
        }

        ActualizarTextoVolumen();
    }

    void ActualizarUI()
    {
        // Actualizar UI con los valores actuales
        if (volumenSlider != null)
        {
            volumenSlider.value = volumenActual;
        }
        ActualizarTextoVolumen();
    }

    void CargarVolumen()
    {
        volumenActual = PlayerPrefs.GetFloat(VOLUMEN_KEY, 0.7f);

        // Aplicar volumen inmediatamente al cargar
        AudioListener.volume = volumenActual;
    }

    void OnSliderCambiado(float nuevoValor)
    {
        volumenActual = nuevoValor;
        AplicarVolumen();
    }

    void AplicarVolumen()
    {
        // Aplicar volumen a TODO el juego
        AudioListener.volume = volumenActual;

        // Buscar y actualizar el MusicManager si existe
        ActualizarMusicManager();

        // Guardar preferencias
        PlayerPrefs.SetFloat(VOLUMEN_KEY, volumenActual);
        PlayerPrefs.Save();

        ActualizarTextoVolumen();

        Debug.Log($"🔊 Volumen aplicado: {Mathf.RoundToInt(volumenActual * 100)}%");
    }

    void ActualizarMusicManager()
    {
        // Buscar el MusicManager en la escena actual o entre objetos persistentes
        MusicManager musicManager = FindFirstObjectByType<MusicManager>();

        if (musicManager != null)
        {
            musicManager.CambiarVolumenGlobal(volumenActual);
        }
        // Si no encuentra, no es error - el volumen global ya se aplicó
    }

    void ActualizarTextoVolumen()
    {
        if (textoVolumen != null)
        {
            textoVolumen.text = Mathf.RoundToInt(volumenActual * 100) + "%";
        }
    }

    // ========== MÉTODOS PÚBLICOS PARA BOTONES ==========

    public void SubirVolumen()
    {
        volumenActual = Mathf.Clamp01(volumenActual + 0.1f);
        if (volumenSlider != null) volumenSlider.value = volumenActual;
        AplicarVolumen();
    }

    public void BajarVolumen()
    {
        volumenActual = Mathf.Clamp01(volumenActual - 0.1f);
        if (volumenSlider != null) volumenSlider.value = volumenActual;
        AplicarVolumen();
    }

    public void ToggleSilencio()
    {
        if (volumenActual > 0)
        {
            // Guardar volumen actual y silenciar
            PlayerPrefs.SetFloat("VolumenAntesSilencio", volumenActual);
            volumenActual = 0;
        }
        else
        {
            // Restaurar volumen anterior
            volumenActual = PlayerPrefs.GetFloat("VolumenAntesSilencio", 0.7f);
        }

        if (volumenSlider != null) volumenSlider.value = volumenActual;
        AplicarVolumen();
    }

    // Método para obtener volumen actual
    public float GetVolumenActual()
    {
        return volumenActual;
    }

    // Método para forzar actualización desde otros scripts
    public void ForzarActualizacion()
    {
        CargarVolumen();
        ActualizarUI();
    }
}