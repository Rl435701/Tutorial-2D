using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour, IPointerClickHandler
{
    [Header("Referencias UI")]
    public GameObject panelDialogo;
    public TextMeshProUGUI textoNombreNPC;
    public TextMeshProUGUI textoDialogo;
    public GameObject indicadorContinuar; // Flecha o icono para continuar

    [Header("Configuración")]
    public float velocidadTexto = 0.05f; // Velocidad de escritura
    public bool usarEfectoEscritura = true;
    public bool tocarCualquierLugarContinuar = true; // Para móvil: tocar en cualquier lugar del panel

    [Header("Audio (Opcional)")]
    public AudioSource audioSource;
    public AudioClip sonidoTexto;
    public AudioClip sonidoFinLinea;

    [Header("Móvil - Botón Continuar")]
    public Button botonContinuarMovil; // Botón específico para móvil

    [Header("Botón Cerrar")]
    public Button botonCerrar; // Botón para cerrar diálogo en cualquier momento

    private string[] lineasActuales;
    private int indiceLineaActual = 0;
    private bool escribiendo = false;
    private NPC npcActual;
    private Coroutine coroutineEscritura;
    private bool isMobile = false;

    void Start()
    {
        // Detectar si estamos en móvil
        isMobile = Application.isMobilePlatform || SystemInfo.deviceType == DeviceType.Handheld;

        // Ocultar panel al inicio
        if (panelDialogo != null)
        {
            panelDialogo.SetActive(false);
        }

        if (indicadorContinuar != null)
        {
            indicadorContinuar.SetActive(false);
        }

        // Configurar botón para móvil
        if (isMobile && botonContinuarMovil != null)
        {
            botonContinuarMovil.gameObject.SetActive(true);
            botonContinuarMovil.onClick.AddListener(MostrarSiguienteLinea);
        }
        else if (botonContinuarMovil != null)
        {
            botonContinuarMovil.gameObject.SetActive(false);
        }

        // Configurar botón de cerrar
        if (botonCerrar != null)
        {
            botonCerrar.onClick.AddListener(ForzarCierreDialogo);
            botonCerrar.gameObject.SetActive(false); // Ocultar al inicio
        }

        // Si es para móvil, agregar componente Image al panel para detectar toques
        if (isMobile && tocarCualquierLugarContinuar && panelDialogo != null)
        {
            Image panelImage = panelDialogo.GetComponent<Image>();
            if (panelImage == null)
            {
                panelImage = panelDialogo.AddComponent<Image>();
                panelImage.color = new Color(0, 0, 0, 0); // Transparente
            }
        }
    }

    void Update()
    {
        // Solo usar input de teclado si NO es móvil
        if (!isMobile)
        {
            if (panelDialogo != null && panelDialogo.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Space) ||
                    Input.GetKeyDown(KeyCode.Return) ||
                    Input.GetMouseButtonDown(0))
                {
                    MostrarSiguienteLinea();
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    ForzarCierreDialogo();
                }
            }
        }
    }

    // Para detectar toques en el panel en móvil
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isMobile && tocarCualquierLugarContinuar && panelDialogo.activeSelf)
        {
            MostrarSiguienteLinea();
        }
    }

    public void IniciarDialogo(string nombreNPC, string[] lineas, NPC npc)
    {
        if (lineas == null || lineas.Length == 0) return;

        npcActual = npc;
        lineasActuales = lineas;
        indiceLineaActual = 0;

        // Mostrar panel
        if (panelDialogo != null)
        {
            panelDialogo.SetActive(true);
        }

        // Mostrar botón de cerrar
        if (botonCerrar != null)
        {
            botonCerrar.gameObject.SetActive(true);
        }

        // Establecer nombre del NPC
        if (textoNombreNPC != null)
        {
            textoNombreNPC.text = nombreNPC;
        }

        // Mostrar primera línea
        MostrarLinea(lineasActuales[indiceLineaActual]);
    }

    public void MostrarSiguienteLinea()
    {
        // Si está escribiendo, mostrar todo el texto inmediatamente
        if (escribiendo)
        {
            CompletarTextoInmediatamente();
            return;
        }

        indiceLineaActual++;

        // Verificar si hay más líneas
        if (indiceLineaActual < lineasActuales.Length)
        {
            MostrarLinea(lineasActuales[indiceLineaActual]);
        }
        else
        {
            // Terminar diálogo
            TerminarDialogo();
        }
    }

    private void MostrarLinea(string linea)
    {
        if (indicadorContinuar != null)
        {
            indicadorContinuar.SetActive(false);
        }

        if (isMobile && botonContinuarMovil != null)
        {
            botonContinuarMovil.gameObject.SetActive(false);
        }

        if (usarEfectoEscritura)
        {
            if (coroutineEscritura != null)
            {
                StopCoroutine(coroutineEscritura);
            }
            coroutineEscritura = StartCoroutine(EscribirTexto(linea));
        }
        else
        {
            if (textoDialogo != null)
            {
                textoDialogo.text = linea;
            }
            MostrarIndicadorContinuar();
        }
    }

    private IEnumerator EscribirTexto(string texto)
    {
        escribiendo = true;
        textoDialogo.text = "";

        foreach (char letra in texto)
        {
            textoDialogo.text += letra;

            // Reproducir sonido de texto
            if (audioSource != null && sonidoTexto != null && letra != ' ')
            {
                audioSource.PlayOneShot(sonidoTexto, 0.5f);
            }

            yield return new WaitForSeconds(velocidadTexto);
        }

        escribiendo = false;
        MostrarIndicadorContinuar();

        // Reproducir sonido de fin de línea
        if (audioSource != null && sonidoFinLinea != null)
        {
            audioSource.PlayOneShot(sonidoFinLinea);
        }
    }

    private void CompletarTextoInmediatamente()
    {
        if (coroutineEscritura != null)
        {
            StopCoroutine(coroutineEscritura);
        }

        if (textoDialogo != null && indiceLineaActual < lineasActuales.Length)
        {
            textoDialogo.text = lineasActuales[indiceLineaActual];
        }

        escribiendo = false;
        MostrarIndicadorContinuar();
    }

    private void MostrarIndicadorContinuar()
    {
        if (indicadorContinuar != null)
        {
            indicadorContinuar.SetActive(true);
        }

        if (isMobile && botonContinuarMovil != null)
        {
            botonContinuarMovil.gameObject.SetActive(true);
        }
    }

    private void TerminarDialogo()
    {
        // Ocultar panel
        if (panelDialogo != null)
        {
            panelDialogo.SetActive(false);
        }

        // Ocultar botón de cerrar
        if (botonCerrar != null)
        {
            botonCerrar.gameObject.SetActive(false);
        }

        // Notificar al NPC
        if (npcActual != null)
        {
            npcActual.TerminarDialogo();
        }

        // Limpiar referencias
        lineasActuales = null;
        npcActual = null;
        indiceLineaActual = 0;
    }

    // Método público para forzar cierre del diálogo
    public void ForzarCierreDialogo()
    {
        if (coroutineEscritura != null)
        {
            StopCoroutine(coroutineEscritura);
        }
        escribiendo = false;
        TerminarDialogo();
    }

    // Método para botón móvil de cerrar
    public void BotonCerrarDialogo()
    {
        ForzarCierreDialogo();
    }
}