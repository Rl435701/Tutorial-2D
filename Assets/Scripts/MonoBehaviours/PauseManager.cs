using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems; // ✅ NUEVO: Necesario para EventSystem
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("UI Elements - Asignar Manualmente")]
    public GameObject pausePanel;
    public Button pauseButton;
    public Button resumeButton;
    public Button restartButton;
    public Button optionsButton;
    public Button mainMenuButton;

    [Header("Elementos a Ocultar Durante Pausa")]
    public GameObject healthBar;
    public GameObject[] additionalElementsToHide;

    [Header("Configuración de Audio")]
    public AudioSource backgroundMusic;
    public bool pauseMusic = false;

    [Header("Configuración Multi-Escena")]
    public bool autoFindUIElements = true;

    private bool isPaused = false;
    private Keyboard keyboard;

    // Singleton pattern para acceso global
    public static PauseManager Instance { get; private set; }

    void Awake()
    {
        // Implementar singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ PauseManager inicializado como Singleton");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        keyboard = Keyboard.current;
        Debug.Log("=== INICIALIZANDO PAUSE MANAGER ===");

        // ✅ NUEVO: Asegurar que hay EventSystem
        EnsureEventSystem();

        // Forzar búsqueda y configuración
        ForceReconnectUI();

        // Suscribirse a eventos de cambio de escena
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"🔄 Escena cargada: {scene.name} - Reconfigurando UI...");

        // ✅ NUEVO: Asegurar EventSystem en cada escena
        EnsureEventSystem();

        ForceReconnectUI();
    }

    // ✅ NUEVO: Verificar y crear EventSystem si no existe
    void EnsureEventSystem()
    {
        EventSystem existingEventSystem = FindAnyObjectByType<EventSystem>();
        if (existingEventSystem == null)
        {
            Debug.Log("⚠️ No hay EventSystem en la escena - Creando uno automáticamente");

            // Crear nuevo GameObject con EventSystem
            GameObject eventSystemObject = new GameObject("EventSystem");

            // Agregar EventSystem
            EventSystem eventSystem = eventSystemObject.AddComponent<EventSystem>();

            // Agregar StandaloneInputModule
            eventSystemObject.AddComponent<StandaloneInputModule>();

            Debug.Log("✅ EventSystem creado automáticamente");

            // Hacerlo persistente entre escenas
            DontDestroyOnLoad(eventSystemObject);
        }
        else
        {
            Debug.Log("✅ EventSystem encontrado en la escena");
        }
    }

    // Método para forzar reconexión completa
    public void ForceReconnectUI()
    {
        Debug.Log("🔄 FORZANDO RECONEXIÓN COMPLETA DE UI");

        if (autoFindUIElements)
        {
            FindAllUIElements();
        }

        SetupButtonListeners();
        InitializeUI();

        // Verificación extra de botones
        VerifyAllButtons();
    }

    void FindAllUIElements()
    {
        Debug.Log("🔍 Buscando todos los elementos UI automáticamente...");

        pausePanel = FindUIElement("MenuPausaPanel");
        pauseButton = FindUIElement("Btn_Pausar")?.GetComponent<Button>();
        healthBar = FindUIElement("BackgroundHealth");
        resumeButton = FindUIElement("Btn_Jugar")?.GetComponent<Button>();
        restartButton = FindUIElement("Btn_Reiniciar")?.GetComponent<Button>();
        optionsButton = FindUIElement("Btn_Opciones")?.GetComponent<Button>();
        mainMenuButton = FindUIElement("Btn_Menu")?.GetComponent<Button>();

        Debug.Log($"📊 RESULTADOS BÚSQUEDA:");
        Debug.Log($"   - Panel: {pausePanel != null} ({pausePanel?.name})");
        Debug.Log($"   - BtnPausa: {pauseButton != null} ({pauseButton?.name})");
        Debug.Log($"   - HealthBar: {healthBar != null} ({healthBar?.name})");
        Debug.Log($"   - ResumeBtn: {resumeButton != null} ({resumeButton?.name})");
        Debug.Log($"   - RestartBtn: {restartButton != null} ({restartButton?.name})");
        Debug.Log($"   - OptionsBtn: {optionsButton != null} ({optionsButton?.name})");
        Debug.Log($"   - MenuBtn: {mainMenuButton != null} ({mainMenuButton?.name})");
    }

    GameObject FindUIElement(string elementName)
    {
        // Buscar en toda la escena primero
        GameObject element = GameObject.Find(elementName);
        if (element != null)
        {
            Debug.Log($"✅ Encontrado {elementName}: {element.name}");
            return element;
        }

        // Buscar recursivamente en Canvas
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas canvas in canvases)
        {
            GameObject foundElement = FindInChildren(canvas.transform, elementName);
            if (foundElement != null)
            {
                Debug.Log($"✅ Encontrado {elementName} en Canvas {canvas.name}: {foundElement.name}");
                return foundElement;
            }
        }

        Debug.LogError($"❌ NO SE ENCONTRÓ: {elementName}");
        return null;
    }

    GameObject FindInChildren(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child.gameObject;

            GameObject found = FindInChildren(child, childName);
            if (found != null)
                return found;
        }
        return null;
    }

    void SetupButtonListeners()
    {
        Debug.Log("🔗 CONFIGURANDO LISTENERS DE BOTONES...");

        // Limpiar TODOS los listeners primero
        ClearAllButtonListeners();

        // Configurar botón de pausa (en el juego)
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(TogglePause);
            Debug.Log($"✅ PAUSE BUTTON: {pauseButton.name} - Listeners: {pauseButton.onClick.GetPersistentEventCount()}");
        }
        else
        {
            Debug.LogError("❌ PAUSE BUTTON ES NULL");
        }

        // Configurar botones del panel de pausa
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeGame);
            Debug.Log($"✅ RESUME BUTTON: {resumeButton.name} - Listeners: {resumeButton.onClick.GetPersistentEventCount()}");
        }
        else
        {
            Debug.LogError("❌ RESUME BUTTON ES NULL");
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartLevel);
            Debug.Log($"✅ RESTART BUTTON: {restartButton.name} - Listeners: {restartButton.onClick.GetPersistentEventCount()}");
        }
        else
        {
            Debug.LogError("❌ RESTART BUTTON ES NULL");
        }

        if (optionsButton != null)
        {
            optionsButton.onClick.AddListener(OpenOptions);
            Debug.Log($"✅ OPTIONS BUTTON: {optionsButton.name} - Listeners: {optionsButton.onClick.GetPersistentEventCount()}");
        }
        else
        {
            Debug.LogError("❌ OPTIONS BUTTON ES NULL");
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(QuitToMenu);
            Debug.Log($"✅ MENU BUTTON: {mainMenuButton.name} - Listeners: {mainMenuButton.onClick.GetPersistentEventCount()}");
        }
        else
        {
            Debug.LogError("❌ MENU BUTTON ES NULL");
        }

        Debug.Log("🔗 CONFIGURACIÓN DE BOTONES COMPLETADA");
    }

    // Limpiar todos los listeners
    void ClearAllButtonListeners()
    {
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (Button btn in allButtons)
        {
            if (btn.name.Contains("Btn_")) // Solo limpiar botones UI
            {
                btn.onClick.RemoveAllListeners();
            }
        }
        Debug.Log("🧹 Todos los listeners de botones limpiados");
    }

    // Verificar estado de todos los botones
    void VerifyAllButtons()
    {
        Debug.Log("🔍 VERIFICANDO ESTADO DE TODOS LOS BOTONES...");

        Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (Button btn in allButtons)
        {
            if (btn.name.Contains("Btn_"))
            {
                Debug.Log($"   - {btn.name}: " +
                         $"Activo={btn.gameObject.activeInHierarchy}, " +
                         $"Interactuable={btn.interactable}, " +
                         $"Listeners={btn.onClick.GetPersistentEventCount()}");

                // Forzar estado correcto
                if (!btn.interactable)
                {
                    btn.interactable = true;
                    Debug.Log($"     ✅ {btn.name} hecho interactuable");
                }
            }
        }
    }

    void InitializeUI()
    {
        Debug.Log("🎮 INICIALIZANDO UI");

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
            Debug.Log("📱 MenuPausa → OCULTO");
        }

        if (pauseButton != null)
        {
            pauseButton.gameObject.SetActive(true);
            Debug.Log("📱 Btn_Pausar → VISIBLE");
        }

        if (healthBar != null)
        {
            healthBar.SetActive(true);
            Debug.Log("❤️ HealthBar → VISIBLE");
        }

        isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;

        Debug.Log("🎮 UI INICIALIZADA");
    }

    void Update()
    {
        if (keyboard != null && keyboard.pKey.wasPressedThisFrame)
        {
            Debug.Log("⌨️ Tecla P presionada");
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Debug.Log($"🔄 TogglePause() - Nuevo estado: {(isPaused ? "PAUSADO" : "ACTIVO")}");

        if (isPaused)
            PauseGame();
        else
            ResumeGame();
    }

    void PauseGame()
    {
        Debug.Log("⏸️ ========== PAUSANDO JUEGO ==========");
        Time.timeScale = 0f;

        // MOSTRAR menú de pausa
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            Debug.Log("✅ MenuPausa ACTIVADO");

            // Verificar botones del panel específicamente
            VerifyPanelButtons();
        }

        // OCULTAR elementos de juego
        SetGameUIVisibility(false);

        if (pauseMusic && backgroundMusic != null)
        {
            backgroundMusic.Pause();
            Debug.Log("🔊 Música pausada");
        }

        Debug.Log("⏸️ ========== JUEGO PAUSADO ==========");
    }

    public void ResumeGame()
    {
        Debug.Log("▶️ ========== REANUDANDO JUEGO ==========");
        Time.timeScale = 1f;

        // OCULTAR menú de pausa
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
            Debug.Log("✅ MenuPausa DESACTIVADO");
        }

        // MOSTRAR elementos de juego
        SetGameUIVisibility(true);

        if (pauseMusic && backgroundMusic != null)
        {
            backgroundMusic.UnPause();
            Debug.Log("🔊 Música reanudada");
        }
        AudioListener.pause = false;
        isPaused = false;

        Debug.Log("▶️ ========== JUEGO REANUDADO ==========");
    }

    void VerifyPanelButtons()
    {
        if (pausePanel == null) return;

        Debug.Log("🔍 VERIFICANDO BOTONES DEL PANEL DE PAUSA...");

        Button[] panelButtons = pausePanel.GetComponentsInChildren<Button>(true);
        foreach (Button btn in panelButtons)
        {
            Debug.Log($"   - {btn.name}: " +
                     $"Activo={btn.gameObject.activeInHierarchy}, " +
                     $"Interactuable={btn.interactable}, " +
                     $"Listeners={btn.onClick.GetPersistentEventCount()}");

            // Forzar estado correcto
            btn.interactable = true;
            if (!btn.gameObject.activeInHierarchy)
            {
                btn.gameObject.SetActive(true);
            }
        }
    }

    void SetGameUIVisibility(bool visible)
    {
        Debug.Log($"🔄 SetGameUIVisibility({visible})");

        if (pauseButton != null)
        {
            pauseButton.gameObject.SetActive(visible);
            Debug.Log($"📱 Btn_Pausar → {(visible ? "VISIBLE" : "OCULTO")}");
        }

        if (healthBar != null)
        {
            healthBar.SetActive(visible);
            Debug.Log($"❤️ HealthBar → {(visible ? "VISIBLE" : "OCULTO")}");
        }

        if (additionalElementsToHide != null)
        {
            foreach (GameObject element in additionalElementsToHide)
            {
                if (element != null)
                {
                    element.SetActive(visible);
                    Debug.Log($"📦 {element.name} → {(visible ? "VISIBLE" : "OCULTO")}");
                }
            }
        }
    }

    public void RestartLevel()
    {
        Debug.Log("🔄 Reiniciando nivel...");
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OpenOptions()
    {
        Debug.Log("⚙️ Abriendo opciones...");
    }

    public void QuitToMenu()
    {
        Debug.Log("🏠 Saliendo al menú principal...");
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene("MenuPrincipal");
    }

    // === MÉTODOS PÚBLICOS PARA OTROS SCRIPTS ===

    public void HideGameUI()
    {
        Debug.Log("🔴 HideGameUI() llamado externamente");
        SetGameUIVisibility(false);
    }

    public void ShowGameUI()
    {
        Debug.Log("🟢 ShowGameUI() llamado externamente");
        if (!isPaused)
        {
            SetGameUIVisibility(true);
        }
    }

    public void ForceHidePauseButton()
    {
        if (pauseButton != null)
        {
            pauseButton.gameObject.SetActive(false);
            Debug.Log("🔴 Btn_Pausar OCULTADO forzadamente");
        }
    }

    public void ForceShowPauseButton()
    {
        if (pauseButton != null && !isPaused)
        {
            pauseButton.gameObject.SetActive(true);
            Debug.Log("🟢 Btn_Pausar MOSTRADO forzadamente");
        }
    }

    public void ForceHideHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.SetActive(false);
            Debug.Log("🔴 HealthBar OCULTADO forzadamente");
        }
    }

    public void ForceShowHealthBar()
    {
        if (healthBar != null && !isPaused)
        {
            healthBar.SetActive(true);
            Debug.Log("🟢 HealthBar MOSTRADO forzadamente");
        }
    }

    public bool IsGamePaused()
    {
        return isPaused;
    }

    public void ForceResume()
    {
        if (isPaused)
        {
            isPaused = false;
            ResumeGame();
        }
    }

    // Métodos estáticos para acceso global
    public static void PauseGameStatic()
    {
        if (Instance != null)
            Instance.TogglePause();
    }

    public static void ResumeGameStatic()
    {
        if (Instance != null && Instance.isPaused)
            Instance.TogglePause();
    }

    public static bool IsGamePausedStatic()
    {
        return Instance != null && Instance.isPaused;
    }

    // Métodos de debug
    [ContextMenu("🔧 Forzar Reconexión UI")]
    public void DebugForceReconnect()
    {
        ForceReconnectUI();
    }

    [ContextMenu("📊 Debug Estado Botones")]
    public void DebugButtonState()
    {
        VerifyAllButtons();
    }

    [ContextMenu("🔍 Debug Estado UI")]
    public void DebugUIState()
    {
        Debug.Log("=== DEBUG UI STATE ===");
        Debug.Log($"PausePanel: {pausePanel != null}");
        Debug.Log($"PauseButton: {pauseButton != null}");
        Debug.Log($"ResumeButton: {resumeButton != null}");
        Debug.Log($"RestartButton: {restartButton != null}");
        Debug.Log($"OptionsButton: {optionsButton != null}");
        Debug.Log($"MainMenuButton: {mainMenuButton != null}");
        Debug.Log($"HealthBar: {healthBar != null}");
        Debug.Log($"Juego pausado: {isPaused}");

        // ✅ NUEVO: Verificar EventSystem
        EventSystem eventSystem = FindAnyObjectByType<EventSystem>();
        Debug.Log($"EventSystem: {eventSystem != null}");
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}