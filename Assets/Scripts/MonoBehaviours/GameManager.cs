using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI References")]
    public TextMeshProUGUI puntajeText;
    public TextMeshProUGUI objetosRecogidosText;
    public GameObject panelVictoria;

    [Header("Game Stats")]
    private int puntaje = 0;
    private int objetosRecogidos = 0;
    public int totalObjetos = 10; // Este será el valor por defecto, se sobrescribirá por nivel

    [Header("Scene Settings")]
    public string nombreSiguienteEscena = "MenuANivel2"; // Este valor será sobrescrito dinámicamente
    public string nombreEscenaActual = "";

    [Header("Timing Settings")]
    public float tiempoPausaAntesDeFade = 1f;
    public float duracionFadeInicial = 2f;
    public float tiempoMostrarVictoria = 5f;
    public float duracionFadeFinal = 2f;

    [Header("Fade Controller")]
    public FadeController fadeController;

    private bool juegoTerminado = false;
    private TimerManager timerManager;
    private string escenaAnterior = "";

    // Variables para manejar puntaje correctamente
    private int puntajeAcumulado = 0;
    private int puntajeInicioNivel = 0;

    // Mapeo de transiciones de escenas
    private Dictionary<string, string> mapaTransiciones;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Inicializar el mapeo de transiciones
            InicializarMapaTransiciones();

            // Cargar puntaje inmediatamente al crear el GameManager
            CargarPuntajeGuardado();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void InicializarMapaTransiciones()
    {
        mapaTransiciones = new Dictionary<string, string>
        {
            // Mapeo de niveles a sus respectivos menús siguientes
            {"Nivel1", "MenuANivel2"},
            {"Nivel2", "MenuANivel3"},
            {"Nivel3", "MenuFinal"},
            
            // También considerar nombres alternativos o variantes
            {"Level1", "MenuANivel2"},
            {"Level2", "MenuANivel3"},
            {"Level3", "MenuFinal"}
        };
    }

    // Método para determinar la siguiente escena basada en la actual
    private string ObtenerSiguienteEscena(string escenaActual)
    {
        // Primero verificar si tenemos un mapeo directo
        if (mapaTransiciones.ContainsKey(escenaActual))
        {
            return mapaTransiciones[escenaActual];
        }

        // Si no está en el diccionario, usar lógica basada en nombres
        if (escenaActual.Contains("Nivel1") || escenaActual.Contains("Level1"))
        {
            return "MenuANivel2";
        }
        else if (escenaActual.Contains("Nivel2") || escenaActual.Contains("Level2"))
        {
            return "MenuANivel3";
        }
        else if (escenaActual.Contains("Nivel3") || escenaActual.Contains("Level3"))
        {
            return "MenuFinal";
        }

        // Si no reconocemos el nivel, devolver el valor por defecto
        return nombreSiguienteEscena;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"🔄 Escena cargada: {scene.name}. Anterior: {escenaAnterior}");

        Time.timeScale = 1f;
        juegoTerminado = false;

        // Actualizar la siguiente escena basada en la escena actual
        nombreSiguienteEscena = ObtenerSiguienteEscena(scene.name);
        Debug.Log($"🎯 Siguiente escena configurada: {nombreSiguienteEscena}");

        if (EsEscenaDeNivel(scene.name))
        {
            bool esNuevoNivel = (scene.name != escenaAnterior);

            if (esNuevoNivel)
            {
                // NUEVO NIVEL: Usar el puntaje acumulado
                puntaje = puntajeAcumulado;
                puntajeInicioNivel = puntajeAcumulado;
                objetosRecogidos = 0;
                Debug.Log($"🔄 Nuevo nivel - Puntaje acumulado: {puntajeAcumulado}, Puntaje inicio: {puntajeInicioNivel}");

                // Buscar el LevelController específico de esta escena
                ActualizarTotalObjetosDesdeScene(scene.name);

                // GUARDAR: Al iniciar nuevo nivel también
                GuardarProgreso();
            }
            else
            {
                // REINICIO: Volver al puntaje de inicio de este nivel
                puntaje = puntajeInicioNivel;
                objetosRecogidos = 0;
                Debug.Log($"🔄 Reinicio - Puntaje vuelve a: {puntajeInicioNivel}");
            }
        }

        escenaAnterior = scene.name;
        nombreEscenaActual = scene.name;

        FindAllReferences();
        ActualizarUI();

        Debug.Log($"✅ Escena {scene.name} preparada. Puntaje: {puntaje}, Objetos: {objetosRecogidos}/{totalObjetos}");
    }

    // NUEVO MÉTODO: Buscar el total de objetos desde la escena actual
    void ActualizarTotalObjetosDesdeScene(string nombreEscena)
    {
        // Buscar todos los GameManagers en la escena (el persistente y los locales)
        GameManager[] todosManagers = FindObjectsOfType<GameManager>(true);

        foreach (GameManager manager in todosManagers)
        {
            // Si encontramos un GameManager que NO es el persistente (Instance)
            if (manager != Instance && manager.gameObject.scene.name == nombreEscena)
            {
                // Usar el valor de totalObjetos de ese GameManager local
                totalObjetos = manager.totalObjetos;
                Debug.Log($"🎯 TotalObjetos actualizado desde escena {nombreEscena}: {totalObjetos}");

                // Destruir el GameManager local ya que solo necesitábamos su configuración
                if (manager.gameObject != null)
                {
                    Destroy(manager.gameObject);
                }
                break;
            }
        }

        // Si no encontramos un GameManager local, mantener el valor actual
        Debug.Log($"📊 TotalObjetos final para {nombreEscena}: {totalObjetos}");
    }

    // Método para cargar puntaje guardado
    private void CargarPuntajeGuardado()
    {
        if (SaveSystem.SaveExists())
        {
            SaveSystem.PlayerProgress progress = SaveSystem.LoadProgress();
            if (progress != null)
            {
                puntajeAcumulado = progress.puntajeTotal;
                Debug.Log($"💰 Puntaje cargado desde guardado: {puntajeAcumulado}");
            }
        }
        else
        {
            Debug.Log("📭 No hay archivo de guardado, empezando con puntaje 0");
            puntajeAcumulado = 0;
        }
    }

    // NUEVO: Método para guardar progreso manualmente
    private void GuardarProgreso()
    {
        if (!string.IsNullOrEmpty(nombreEscenaActual))
        {
            SaveSystem.SaveProgress(nombreEscenaActual);
            Debug.Log($"💾 Progreso guardado manualmente - Escena: {nombreEscenaActual}, Puntaje: {puntajeAcumulado}");
        }
    }

    private bool EsEscenaDeNivel(string nombreEscena)
    {
        if (string.IsNullOrEmpty(nombreEscena)) return false;

        return nombreEscena.Contains("Nivel") ||
               nombreEscena.Contains("Level") ||
               nombreEscena == "Nivel1" ||
               nombreEscena == "Nivel2" ||
               nombreEscena == "Nivel3";
    }

    void Start()
    {
        if (escenaAnterior == "")
        {
            nombreEscenaActual = SceneManager.GetActiveScene().name;
            escenaAnterior = nombreEscenaActual;

            // Configurar la siguiente escena basada en la escena inicial
            nombreSiguienteEscena = ObtenerSiguienteEscena(nombreEscenaActual);

            // Primer nivel, si no hay puntaje guardado, inicia en 0
            if (puntajeAcumulado == 0)
            {
                puntaje = 0;
                puntajeInicioNivel = 0;
            }
            else
            {
                puntaje = puntajeAcumulado;
                puntajeInicioNivel = puntajeAcumulado;
            }

            // Buscar el total de objetos para la escena inicial
            ActualizarTotalObjetosDesdeScene(nombreEscenaActual);

            // Guardar progreso inicial
            GuardarProgreso();
        }

        FindAllReferences();
        ActualizarUI();
    }

    void OnApplicationQuit()
    {
        // NUEVO: Guardar progreso cuando se cierra el juego
        Debug.Log("🚪 Cerrando juego - Guardando progreso final");
        GuardarProgreso();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        // NUEVO: Guardar progreso cuando el juego se pausa (móvil/alt-tab)
        if (pauseStatus)
        {
            Debug.Log("⏸️ Juego pausado - Guardando progreso");
            GuardarProgreso();
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    public void ReiniciarContadoresCompleto()
    {
        puntaje = 0;
        puntajeAcumulado = 0;
        puntajeInicioNivel = 0;
        objetosRecogidos = 0;
        juegoTerminado = false;
        Debug.Log("🔄🔄🔄 CONTADORES COMPLETAMENTE REINICIADOS 🔄🔄🔄");

        // Guardar el reset
        GuardarProgreso();
    }

    public void ReiniciarContadores()
    {
        ReiniciarContadoresCompleto();
        ActualizarUI();
    }

    public void ReiniciarNivelActual()
    {
        Debug.Log("🔄 Reiniciando nivel actual...");
        puntaje = puntajeInicioNivel;
        objetosRecogidos = 0;
        juegoTerminado = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void CargarEscenaConReset(string nombreEscena)
    {
        Debug.Log($"🔄 Cargando escena {nombreEscena} con reset...");
        ReiniciarContadoresCompleto();
        SceneManager.LoadScene(nombreEscena);
    }

    // Método público para obtener el puntaje acumulado
    public int GetPuntajeAcumulado()
    {
        return puntajeAcumulado;
    }

    private void GuardarPuntajeParaSiguienteNivel()
    {
        puntajeAcumulado = puntaje;
        Debug.Log($"💰 Puntaje guardado para siguiente nivel: {puntaje} -> {puntajeAcumulado}");

        // GUARDAR inmediatamente después de actualizar el puntaje acumulado
        GuardarProgreso();
    }

    void FindAllReferences()
    {
        FindUITexts();
        FindTimerManager();
        FindFadeController();
        FindVictoryPanel();
    }

    void FindUITexts()
    {
        string[] posiblesNombresPuntaje = { "Puntaje", "PuntajeText", "Score", "ScoreText", "TextoPuntaje" };
        string[] posiblesNombresObjetos = { "ObjRecogidos", "ObjetosText", "Objetos", "Items", "ObjetosRecogidos" };

        if (puntajeText == null)
        {
            foreach (string nombre in posiblesNombresPuntaje)
            {
                GameObject obj = GameObject.Find(nombre);
                if (obj != null)
                {
                    puntajeText = obj.GetComponent<TextMeshProUGUI>();
                    if (puntajeText != null) break;
                }
            }
        }

        if (objetosRecogidosText == null)
        {
            foreach (string nombre in posiblesNombresObjetos)
            {
                GameObject obj = GameObject.Find(nombre);
                if (obj != null)
                {
                    objetosRecogidosText = obj.GetComponent<TextMeshProUGUI>();
                    if (objetosRecogidosText != null) break;
                }
            }
        }

        if (puntajeText == null || objetosRecogidosText == null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                TextMeshProUGUI[] textos = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);

                foreach (TextMeshProUGUI texto in textos)
                {
                    string textoLower = texto.text.ToLower();

                    if (puntajeText == null && (textoLower.Contains("puntaje") || textoLower.Contains("score")))
                    {
                        puntajeText = texto;
                    }

                    if (objetosRecogidosText == null && (textoLower.Contains("objeto") || textoLower.Contains("item")))
                    {
                        objetosRecogidosText = texto;
                    }
                }
            }
        }
    }

    void FindVictoryPanel()
    {
        if (panelVictoria == null)
        {
            string[] posiblesNombresPanel = { "PanelVictoria", "VictoryPanel", "WinPanel", "PanelWin" };

            foreach (string nombre in posiblesNombresPanel)
            {
                GameObject obj = GameObject.Find(nombre);
                if (obj != null)
                {
                    panelVictoria = obj;
                    panelVictoria.SetActive(false);
                    break;
                }
            }
        }
    }

    void FindTimerManager()
    {
        timerManager = FindFirstObjectByType<TimerManager>();
        if (timerManager != null)
        {
            Debug.Log("✅ TimerManager encontrado y referenciado");
        }
    }

    void FindFadeController()
    {
        if (fadeController != null) return;

        FadeController existingFade = FindFirstObjectByType<FadeController>();
        if (existingFade != null)
        {
            fadeController = existingFade;
        }
    }

    public int GetObjetosRecogidos()
    {
        return objetosRecogidos;
    }

    public int GetTotalObjetos()
    {
        return totalObjetos;
    }

    public int GetPuntajeActual()
    {
        return puntaje;
    }

    public float GetProgresoObjetos()
    {
        return (float)objetosRecogidos / totalObjetos;
    }

    public bool IsJuegoTerminado()
    {
        return juegoTerminado;
    }

    public void RecogerObjeto(int puntos, bool contarComoObjetoRecogido)
    {
        if (juegoTerminado) return;

        puntaje += puntos;
        Debug.Log($"📦 Objeto recogido! +{puntos} puntos. Puntaje total: {puntaje}");

        if (contarComoObjetoRecogido)
        {
            objetosRecogidos++;
            Debug.Log($"🎯 Objetos recogidos en este nivel: {objetosRecogidos}/{totalObjetos}");
        }

        ActualizarUI();

        if (objetosRecogidos >= totalObjetos)
        {
            Debug.Log($"🎉 ¡Nivel completado! Objetos: {objetosRecogidos}/{totalObjetos}");
            GuardarPuntajeParaSiguienteNivel();
            OnLevelCompleted();
        }
    }

    private void OnLevelCompleted()
    {
        if (timerManager != null)
        {
            timerManager.StopTimer();
            Debug.Log("⏹️ Cronómetro detenido - Nivel completado");
        }

        StartCoroutine(SecuenciaVictoria());
    }

    public void AñadirPuntos(int puntos)
    {
        if (juegoTerminado) return;
        puntaje += puntos;
        ActualizarUI();
    }

    public void ContarObjeto()
    {
        if (juegoTerminado) return;
        objetosRecogidos++;
        ActualizarUI();

        if (objetosRecogidos >= totalObjetos)
        {
            GuardarPuntajeParaSiguienteNivel();
            OnLevelCompleted();
        }
    }

    void ActualizarUI()
    {
        if (puntajeText != null)
        {
            puntajeText.text = "Puntaje: " + puntaje.ToString("000");
        }

        if (objetosRecogidosText != null)
        {
            objetosRecogidosText.text = $"Objetos: {objetosRecogidos}/{totalObjetos}";

            if (objetosRecogidos >= totalObjetos)
            {
                objetosRecogidosText.color = Color.green;
            }
            else if (objetosRecogidos >= totalObjetos * 0.7f)
            {
                objetosRecogidosText.color = Color.yellow;
            }
            else
            {
                objetosRecogidosText.color = Color.white;
            }
        }
    }

    IEnumerator SecuenciaVictoria()
    {
        juegoTerminado = true;
        Debug.Log("=== INICIANDO SECUENCIA DE VICTORIA ===");

        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(tiempoPausaAntesDeFade);

        if (fadeController == null)
        {
            Debug.LogError("❌ FadeController es NULL!");
            FindFadeController();
        }

        if (fadeController != null && fadeController.imagenFade != null)
        {
            Debug.Log("✅ FadeController encontrado, iniciando secuencia de fade...");

            yield return StartCoroutine(fadeController.FadeIn(duracionFadeInicial));

            if (panelVictoria != null)
            {
                panelVictoria.SetActive(true);
                Debug.Log("✅ Panel de victoria activado");
            }

            yield return StartCoroutine(fadeController.FadeOut(duracionFadeInicial));

            yield return new WaitForSecondsRealtime(tiempoMostrarVictoria);

            yield return StartCoroutine(fadeController.FadeIn(duracionFadeFinal));
        }
        else
        {
            Debug.LogError("❌ No se pudo encontrar FadeController, cargando escena directamente");
            yield return new WaitForSecondsRealtime(tiempoMostrarVictoria);
        }

        Time.timeScale = 1f;

        CargarSiguienteEscena();
    }

    void CargarSiguienteEscena()
    {
        // Asegurarnos de que el nombre de la siguiente escena esté actualizado
        nombreSiguienteEscena = ObtenerSiguienteEscena(nombreEscenaActual);

        if (string.IsNullOrEmpty(nombreSiguienteEscena))
        {
            Debug.LogError("Nombre de escena siguiente está vacío.");
            return;
        }

        if (Application.CanStreamedLevelBeLoaded(nombreSiguienteEscena))
        {
            SaveSystem.SaveProgress(nombreSiguienteEscena);

            Debug.Log($"🎯 Cargando siguiente escena: {nombreSiguienteEscena}");
            SceneManager.LoadScene(nombreSiguienteEscena);
        }
        else
        {
            Debug.LogError($"La escena '{nombreSiguienteEscena}' no está en Build Settings.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    // Método público para obtener la siguiente escena (útil para debug)
    public string GetSiguienteEscena()
    {
        return ObtenerSiguienteEscena(nombreEscenaActual);
    }
}