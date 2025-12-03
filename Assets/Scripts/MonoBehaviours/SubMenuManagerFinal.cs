using UnityEngine;
using UnityEngine.SceneManagement;

public class SubMenuManagerFinal : MonoBehaviour
{
    // metodo para iniciar el juego
    public void PlayGame()
    {
        SceneManager.LoadScene("Creditos");
    }

    // Metodo para salir del juego
    public void QuitGame()
    {
        SceneManager.LoadScene("MenuPrincipal");
    }
}
