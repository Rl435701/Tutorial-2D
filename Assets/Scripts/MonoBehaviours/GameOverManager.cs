using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    [Header("UI Elements - Asignar Manualmente")]
    public GameObject gameOverPanel;
    public Button restartButton;
    public Button menuPrincipalButton;
    public Text gameOverText;

    [Header("Elementos a Ocultar")]
    public GameObject healthBar;
    public Button pauseButton;

    [Header("Configuración")]
    public string gameOverMessage = "GAME OVER";
    public string menuPrincipalSceneName = "MenuPrincipal";

    [Header("Timing Settings")]
    [Tooltip("Duración del fade inicial antes de mostrar game over")]
    public float duracionFadeInicial = 1f;

    [Tooltip("Tiempo que permanece visible el panel de game over")]
    public float tiempoMostrarGameOver = 5f;

    [Header("Fade Controller")]
    public FadeController fadeController;

    private Player player;
    private PauseManager pauseManager;
    private TimerManager timerManager;
    private bool isGameOver = false;

    void Start()
    {
        pauseManager = FindFirstObjectByType<PauseManager>();
        player = FindFirstObjectByType<Player>();
        timerManager = FindFirstObjectByType<TimerManager>();

        // Verificar asignaciones críticas
        if (gameOverPanel == null) Debug.LogError("❌ gameOverPanel NO ASIGNADO");
        if (healthBar == null) Debug.LogError("❌ healthBar NO ASIGNADO");
        if (pauseButton == null) Debug.LogError("❌ pauseButton NO ASIGNADO");

        // Buscar el FadeController existente
        EnsureFadeControllerExists();

        if (player != null)
        {
            Player playerComponent = player.GetComponent<Player>();
            if (playerComponent != null)
            {
                playerComponent.OnDeath += StartGameOverSequence;
                Debug.Log("✅ GameOverManager conectado al evento OnDeath del Player");
            }
        }
        else
        {
            Debug.LogError("❌ GameOverManager: No se encontró el jugador");
        }

        SetupButtonListeners();

        if (gameOverText != null)
            gameOverText.text = gameOverMessage;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        Debug.Log("✅ GameOverManager inicializado");
    }

    // NUEVO: Método para game over por tiempo agotado
    public void GameOverPorTiempo()
    {
        if (isGameOver) return;

        Debug.Log("⏰ Game Over por tiempo agotado");

        // Cambiar mensaje para tiempo agotado
        if (gameOverText != null)
        {
            gameOverText.text = "TIEMPO AGOTADO";
        }

        StartGameOverSequence();
    }

    void EnsureFadeControllerExists()
    {
        if (fadeController != null)
        {
            Debug.Log("GameOverManager: Usando FadeController asignado en el Inspector.");
            return;
        }

        FadeController existingFade = FindFirstObjectByType<FadeController>();
        if (existingFade != null)
        {
            fadeController = existingFade;
            Debug.Log("GameOverManager: FadeController encontrado en la escena.");
        }
        else
        {
            Debug.LogError("GameOverManager: No se encontró FadeController en la escena.");
        }
    }

    void SetupButtonListeners()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
            Debug.Log("✅ Listener asignado a restartButton");
        }

        if (menuPrincipalButton != null)
        {
            menuPrincipalButton.onClick.RemoveAllListeners();
            menuPrincipalButton.onClick.AddListener(LoadMenuPrincipal);
            Debug.Log("✅ Listener asignado a menuPrincipalButton");
        }
    }

    public void StartGameOverSequence()
    {
        if (isGameOver) return;

        Debug.Log("💀 ========== INICIANDO SECUENCIA GAME OVER ==========");
        isGameOver = true;

        // Detener el cronómetro si existe
        if (timerManager != null)
        {
            timerManager.StopTimer();
            Debug.Log("⏹️ Cronómetro detenido por Game Over");
        }

        StartCoroutine(SecuenciaDerrota());
    }

    IEnumerator SecuenciaDerrota()
    {
        Debug.Log("🔄 Iniciando secuencia de derrota...");

        // PASO 1: PAUSAR/CONGELAR EL JUEGO
        Time.timeScale = 0f;
        Debug.Log("1. Juego pausado");

        // PASO 2: ESPERAR 1 SEGUNDO (en tiempo real)
        yield return new WaitForSecondsRealtime(duracionFadeInicial);
        Debug.Log("2. Pausa completada, iniciando fade...");

        // PASO 3: FADE IN GRADUAL hasta pantalla completamente negra
        if (fadeController != null)
        {
            yield return StartCoroutine(fadeController.FadeIn(duracionFadeInicial));
            Debug.Log("3. Fade In completado - pantalla negra");
        }
        else
        {
            Debug.LogError("FadeController no disponible.");
            yield return new WaitForSecondsRealtime(duracionFadeInicial);
        }

        // PASO 4: PREPARAR EL JUEGO PARA EL GAME OVER
        PrepararJuegoParaGameOver();

        // PASO 5: MOSTRAR PANEL DE GAME OVER (sobre el fondo negro)
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Debug.Log("4. Panel de Game Over activado");
        }
        else
        {
            Debug.LogWarning("gameOverPanel es null.");
        }

        // PASO 6: FADE OUT para revelar el panel de game over sobre fondo negro
        if (fadeController != null)
        {
            yield return StartCoroutine(fadeController.FadeOut(duracionFadeInicial));
            Debug.Log("5. Fade Out completado - panel visible");
        }

        Debug.Log("🎮 Secuencia completada - Juego en estado Game Over esperando input del usuario");
    }

    private void PrepararJuegoParaGameOver()
    {
        // Forzar que el juego no esté pausado (pero luego lo pausamos nosotros)
        if (pauseManager != null)
        {
            pauseManager.ForceResume();
        }

        // OCULTAR elementos del juego
        SetGameUIVisibility(false);

        // Deshabilitar scripts del jugador
        if (player != null)
        {
            MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                if (script != this && script.enabled)
                {
                    script.enabled = false;
                    Debug.Log($"🔴 Script {script.GetType().Name} deshabilitado");
                }
            }
        }

        Debug.Log("🎮 Juego preparado para estado Game Over");
    }

    public void ShowGameOverMenu()
    {
        StartGameOverSequence();
    }

    void SetGameUIVisibility(bool visible)
    {
        if (pauseButton != null)
        {
            pauseButton.gameObject.SetActive(visible);
            Debug.Log($"📱 Btn_Pausar → {(visible ? "VISIBLE" : "OCULTO")}");
        }
        else
        {
            Debug.LogError("❌ pauseButton es NULL en SetGameUIVisibility");
        }

        if (healthBar != null)
        {
            healthBar.SetActive(visible);
            Debug.Log($"❤️ BackgroundHealth → {(visible ? "VISIBLE" : "OCULTO")}");
        }
        else
        {
            Debug.LogError("❌ healthBar es NULL en SetGameUIVisibility");
        }

        if (pauseManager != null)
        {
            if (visible)
            {
                pauseManager.ShowGameUI();
            }
            else
            {
                pauseManager.HideGameUI();
            }
        }
    }

    public void HideGameOverMenu()
    {
        if (!isGameOver) return;

        Debug.Log("🟢 Ocultando menú Game Over");

        Time.timeScale = 1f;
        isGameOver = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        SetGameUIVisibility(true);

        if (player != null)
        {
            MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                if (script != this)
                {
                    script.enabled = true;
                    Debug.Log($"🟢 Script {script.GetType().Name} habilitado");
                }
            }
        }
    }

    public void RestartGame()
    {
        Debug.Log("🔄 Reiniciando juego desde Game Over...");

        Time.timeScale = 1f;
        AudioListener.pause = false;
        isGameOver = false;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void LoadMenuPrincipal()
    {
        Debug.Log("🏠 Cargando Menu Principal desde Game Over...");

        Time.timeScale = 1f;
        AudioListener.pause = false;
        isGameOver = false;

        if (!string.IsNullOrEmpty(menuPrincipalSceneName))
        {
            SceneManager.LoadScene(menuPrincipalSceneName);
        }
        else
        {
            SceneManager.LoadScene("MenuPrincipal");
        }
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            Player playerComponent = player.GetComponent<Player>();
            if (playerComponent != null)
            {
                playerComponent.OnDeath -= StartGameOverSequence;
            }
        }
    }
}