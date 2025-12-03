using UnityEngine;
using UnityEngine.SceneManagement;

public class ProgressTracker : MonoBehaviour
{
    private static ProgressTracker instance;

    private void Awake()
    {
        // Implementar patrón Singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Guardar progreso cuando se cargan ciertas escenas
        string currentScene = scene.name;

        if (currentScene == "MenuANivel2" || currentScene == "MenuANivel3" ||
            currentScene == "MenuFinal" || currentScene == "Nivel2" ||
            currentScene == "Nivel3")
        {
            SaveSystem.SaveProgress(currentScene);
            Debug.Log("💾 Progreso guardado automáticamente en: " + currentScene);
        }
    }

    // Método público para forzar guardado manual
    public void SaveCurrentProgress()
    {
        SaveSystem.SaveProgress(SceneManager.GetActiveScene().name);
    }
}