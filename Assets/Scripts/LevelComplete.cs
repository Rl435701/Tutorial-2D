using UnityEngine;

public class LevelComplete : MonoBehaviour
{
    [Header("Configuración")]
    public string nextSceneName;
    public bool isLevelEnd = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isLevelEnd)
        {
            CompleteLevel();
        }
    }

    public void CompleteLevel()
    {
        // Guardar progreso antes de cambiar de escena
        SaveSystem.SaveProgress(nextSceneName);

        // Cargar siguiente escena
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
    }
}