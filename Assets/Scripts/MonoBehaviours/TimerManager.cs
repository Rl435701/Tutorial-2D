using System.Collections;
using TMPro;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI timerText;

    [Header("Timer Settings")]
    public int initialMinutes = 3;
    public int initialSeconds = 30;

    private float currentTime;
    private bool timerRunning = false;
    private GameManager gameManager;
    private GameOverManager gameOverManager;

    void Start()
    {
        InitializeReferences();
        InitializeTimer();
    }

    void InitializeReferences()
    {
        // Buscar GameManager
        gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("❌ GameManager no encontrado en la escena");
        }

        // Buscar GameOverManager
        gameOverManager = FindFirstObjectByType<GameOverManager>();
        if (gameOverManager == null)
        {
            Debug.LogWarning("⚠️ GameOverManager no encontrado en la escena");
        }

        // Buscar TimerText si no está asignado
        if (timerText == null)
        {
            GameObject timerObj = GameObject.Find("TimerText");
            if (timerObj != null)
            {
                timerText = timerObj.GetComponent<TextMeshProUGUI>();
            }

            // Si aún no se encuentra, buscar por nombres comunes
            if (timerText == null)
            {
                string[] posiblesNombres = { "Timer", "Tiempo", "Time", "Reloj" };
                foreach (string nombre in posiblesNombres)
                {
                    GameObject obj = GameObject.Find(nombre);
                    if (obj != null)
                    {
                        timerText = obj.GetComponent<TextMeshProUGUI>();
                        if (timerText != null) break;
                    }
                }
            }
        }

        if (timerText == null)
        {
            Debug.LogError("❌ TimerText no encontrado en la escena");
        }
    }

    void InitializeTimer()
    {
        currentTime = initialMinutes * 60 + initialSeconds;
        UpdateTimerDisplay();
        StartTimer();

        Debug.Log("✅ TimerManager inicializado - Tiempo: " + initialMinutes + ":" + initialSeconds.ToString("00"));
    }

    void Update()
    {
        if (timerRunning)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerDisplay();

            if (currentTime <= 0)
            {
                currentTime = 0;
                timerRunning = false;
                OnTimeUp();
            }
        }
    }

    void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            if (currentTime <= 30f)
            {
                timerText.color = Color.red;
            }
            else if (currentTime <= 60f)
            {
                timerText.color = Color.yellow;
            }
            else
            {
                timerText.color = Color.white;
            }
        }
    }

    public void StartTimer()
    {
        timerRunning = true;
    }

    public void StopTimer()
    {
        timerRunning = false;
    }

    public void ResetTimer()
    {
        currentTime = initialMinutes * 60 + initialSeconds;
        if (timerText != null) timerText.color = Color.white;
        UpdateTimerDisplay();
    }

    public void AddTime(float secondsToAdd)
    {
        currentTime += secondsToAdd;
    }

    private void OnTimeUp()
    {
        Debug.Log("⏰ ¡TIEMPO AGOTADO!");

        if (gameManager != null)
        {
            int objetosRecogidos = gameManager.GetObjetosRecogidos();
            int totalObjetos = gameManager.GetTotalObjetos();

            if (objetosRecogidos < totalObjetos)
            {
                Debug.Log($"❌ Game Over: {objetosRecogidos}/{totalObjetos} objetos");
                TriggerGameOver();
            }
            else
            {
                Debug.Log($"✅ Nivel completado a tiempo: {objetosRecogidos}/{totalObjetos} objetos");
            }
        }
        else
        {
            Debug.LogError("GameManager no encontrado");
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        if (gameOverManager != null)
        {
            gameOverManager.GameOverPorTiempo();
        }
        else
        {
            Debug.LogError("GameOverManager no encontrado - No se puede mostrar Game Over");
        }
    }

    public float GetCurrentTime()
    {
        return currentTime;
    }

    public bool IsTimerRunning()
    {
        return timerRunning;
    }
}