using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionManager : MonoBehaviour
{
    // metodo para iniciar el juego
    public void MenuPrincipal()
    {
        SceneManager.LoadScene("MenuPrincipal");
    }
}