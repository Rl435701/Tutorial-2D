using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Botones")]
    public Button playButton;
    public Button optionsButton;
    public Button quitButton;

    private void Start()
    {
        // Configurar listeners de botones
        if (playButton != null)
            playButton.onClick.AddListener(PlayGame);

        if (optionsButton != null)
            optionsButton.onClick.AddListener(OptionsMenu);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        // Verificar si hay progreso guardado y actualizar el texto del botón
        CheckSavedProgress();
    }

    private void CheckSavedProgress()
    {
        if (playButton != null)
        {
            if (SaveSystem.SaveExists())
            {
                SaveSystem.PlayerProgress progress = SaveSystem.LoadProgress();
                if (progress != null)
                {
                    // Verificar si el juego está completado
                    Text buttonText = playButton.GetComponentInChildren<Text>();
                    if (buttonText != null)
                    {
                        if (progress.juegoCompletado)
                        {
                            buttonText.text = "Nueva Partida";
                        }
                        else
                        {
                            buttonText.text = "Continuar";
                        }
                    }
                }
            }
        }
    }

    // Método para iniciar/continuar el juego
    public void PlayGame()
    {
        if (SaveSystem.SaveExists())
        {
            SaveSystem.PlayerProgress progress = SaveSystem.LoadProgress();

            // Verificar si el juego está completado
            if (progress.juegoCompletado)
            {
                // Resetear progreso para nueva partida
                SaveSystem.ResetForNewGame();
                Debug.Log("Iniciando nueva partida desde Nivel1 (juego completado)");
                SceneManager.LoadScene("Nivel1");
                return;
            }

            // Determinar a qué escena ir basado en el progreso
            string sceneToLoad = "Nivel1"; // Por defecto

            if (progress.nivel3Completed || progress.lastSceneUnlocked == "MenuFinal")
            {
                sceneToLoad = "Nivel3";
            }
            else if (progress.nivel2Completed || progress.lastSceneUnlocked == "MenuANivel2" || progress.lastSceneUnlocked == "Nivel2" || progress.lastSceneUnlocked == "MenuANivel3")
            {
                sceneToLoad = "Nivel2";
            }

            Debug.Log("Cargando escena: " + sceneToLoad);
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            // No hay progreso guardado, empezar desde el principio
            Debug.Log("Iniciando nuevo juego desde Nivel1");
            SceneManager.LoadScene("Nivel1");
        }
    }

    // Método para cargar la escena de opciones
    public void OptionsMenu()
    {
        SceneManager.LoadScene("Options");
    }

    // Método para salir del juego
    public void QuitGame()
    {
        Debug.Log("Salir del juego");
        Application.Quit();
    }

    // Método para reiniciar progreso (útil para testing)
    public void ResetProgress()
    {
        SaveSystem.DeleteSave();
        Debug.Log("Progreso reiniciado");

        // Recargar la escena actual para actualizar la UI
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}