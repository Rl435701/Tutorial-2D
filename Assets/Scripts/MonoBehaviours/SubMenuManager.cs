using UnityEngine;
using UnityEngine.SceneManagement;

public class SubMenuManager : MonoBehaviour
{
    // metodo para iniciar el juego
    public void PlayGame()
    {
        SceneManager.LoadScene("Nivel2");
    }

    // Metodo para salir del juego
    public void QuitGame()
    {
        SceneManager.LoadScene("MenuPrincipal");
    }
}