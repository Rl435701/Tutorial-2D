using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeController : MonoBehaviour
{
    public static FadeController Instance;

    [Header("Fade Settings")]
    public Image imagenFade;
    public float duracionFade = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeFadeImage();
    }

    void InitializeFadeImage()
    {
        if (imagenFade != null)
        {
            // Asegurar que esté activo
            imagenFade.gameObject.SetActive(true);

            // Configurar RectTransform para pantalla completa
            RectTransform rect = imagenFade.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.localScale = Vector3.one;
            }

            // Configurar color inicial (transparente)
            Color color = imagenFade.color;
            color.a = 0f;
            imagenFade.color = color;

            // Asegurar que tenga un material/sprite
            if (imagenFade.sprite == null)
            {
                // Crear un sprite blanco simple si no tiene
                Texture2D texture = new Texture2D(1, 1);
                texture.SetPixel(0, 0, Color.white);
                texture.Apply();
                imagenFade.sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
                imagenFade.type = Image.Type.Simple;
            }

            Debug.Log("FadeController inicializado - Alpha: " + imagenFade.color.a);
        }
        else
        {
            Debug.LogError("FadeController: 'imagenFade' no asignado en el Inspector!");
        }
    }

    public IEnumerator FadeIn(float duracion = -1f)
    {
        if (imagenFade == null)
        {
            Debug.LogError("FadeIn: imagenFade es null!");
            yield break;
        }

        float tiempo = duracion < 0 ? duracionFade : duracion;

        Debug.Log($"Iniciando FadeIn - Duración: {tiempo}, Alpha inicial: {imagenFade.color.a}");

        imagenFade.gameObject.SetActive(true);
        float elapsed = 0f;
        Color color = imagenFade.color;

        while (elapsed < tiempo)
        {
            elapsed += Time.unscaledDeltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsed / tiempo);
            imagenFade.color = color;
            yield return null;
        }

        color.a = 1f;
        imagenFade.color = color;

        Debug.Log($"FadeIn completado - Alpha final: {imagenFade.color.a}");
    }

    public IEnumerator FadeOut(float duracion = -1f)
    {
        if (imagenFade == null)
        {
            Debug.LogError("FadeOut: imagenFade es null!");
            yield break;
        }

        float tiempo = duracion < 0 ? duracionFade : duracion;

        Debug.Log($"Iniciando FadeOut - Duración: {tiempo}, Alpha inicial: {imagenFade.color.a}");

        float elapsed = 0f;
        Color color = imagenFade.color;

        while (elapsed < tiempo)
        {
            elapsed += Time.unscaledDeltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsed / tiempo);
            imagenFade.color = color;
            yield return null;
        }

        color.a = 0f;
        imagenFade.color = color;

        Debug.Log($"FadeOut completado - Alpha final: {imagenFade.color.a}");
    }
}