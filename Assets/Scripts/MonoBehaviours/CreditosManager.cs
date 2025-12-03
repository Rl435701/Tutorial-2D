using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CreditosController : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private TextMeshProUGUI tituloText;
    [SerializeField] private TextMeshProUGUI autorText;
    [SerializeField] private TextMeshProUGUI rlText;
    [SerializeField] private TextMeshProUGUI masText;
    [SerializeField] private TextMeshProUGUI rl1Text;
    [SerializeField] private Button salirButton;

    [Header("Configuración")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float delayBetweenTexts = 0.5f;
    [SerializeField] private string menuPrincipalScene = "MenuPrincipal";

    private bool isTyping = false;

    void Start()
    {
        // Ocultar todo inicialmente
        tituloText.gameObject.SetActive(false);
        autorText.gameObject.SetActive(false);
        rlText.gameObject.SetActive(false);
        masText.gameObject.SetActive(false);
        rl1Text.gameObject.SetActive(false);
        salirButton.gameObject.SetActive(false);

        // Configurar el botón
        salirButton.onClick.AddListener(OnSalirButtonClicked);

        // Guardar los textos originales y limpiarlos
        StoreOriginalTexts();

        // Iniciar la secuencia de aparición
        StartCoroutine(ShowCreditosSequence());
    }

    void Update()
    {
        // Bloquear completamente cualquier tecla para saltar el texto
    }

    // Almacenar los textos originales y limpiar los campos
    private void StoreOriginalTexts()
    {
        // Los textos se toman directamente de los componentes TextMeshPro
        // No necesitamos almacenarlos porque ya están configurados en el inspector
    }

    private IEnumerator ShowCreditosSequence()
    {
        // Mostrar Titulo con efecto de escritura
        tituloText.gameObject.SetActive(true);
        yield return StartCoroutine(TypeText(tituloText));
        yield return new WaitForSeconds(delayBetweenTexts);

        // Mostrar Autor con efecto de escritura
        autorText.gameObject.SetActive(true);
        yield return StartCoroutine(TypeText(autorText));
        yield return new WaitForSeconds(delayBetweenTexts);

        // Mostrar RL (primero) con efecto de escritura
        rlText.gameObject.SetActive(true);
        yield return StartCoroutine(TypeText(rlText));
        yield return new WaitForSeconds(delayBetweenTexts);

        // Mostrar Mas con efecto de escritura
        masText.gameObject.SetActive(true);
        yield return StartCoroutine(TypeText(masText));
        yield return new WaitForSeconds(delayBetweenTexts);

        // Mostrar RL (segundo) con efecto de escritura
        rl1Text.gameObject.SetActive(true);
        yield return StartCoroutine(TypeText(rl1Text));
        yield return new WaitForSeconds(delayBetweenTexts);

        // Mostrar el botón Salir al final
        salirButton.gameObject.SetActive(true);
    }

    private IEnumerator TypeText(TextMeshProUGUI textComponent)
    {
        isTyping = true;
        string fullText = textComponent.text; // Tomar el texto actual del componente
        textComponent.text = ""; // Limpiar el texto

        // Escribir letra por letra
        foreach (char letter in fullText.ToCharArray())
        {
            textComponent.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void OnSalirButtonClicked()
    {
        // Cargar directamente el MenuPrincipal
        SceneManager.LoadScene(menuPrincipalScene);
    }
}