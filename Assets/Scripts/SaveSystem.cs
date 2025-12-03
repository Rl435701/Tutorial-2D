using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSystem
{
    private static string savePath = Application.persistentDataPath + "/playerProgress.save";

    [System.Serializable]
    public class PlayerProgress
    {
        public string lastSceneUnlocked;
        public bool nivel1Completed;
        public bool nivel2Completed;
        public bool nivel3Completed;
        public bool juegoCompletado;
        public int puntajeTotal; // NUEVO: Guardar el puntaje total

        // Constructor para valores por defecto
        public PlayerProgress()
        {
            lastSceneUnlocked = "Nivel1";
            nivel1Completed = false;
            nivel2Completed = false;
            nivel3Completed = false;
            juegoCompletado = false;
            puntajeTotal = 0;
        }
    }

    public static void SaveProgress(string currentScene)
    {
        // Cargar progreso existente o crear uno nuevo
        PlayerProgress progress = LoadProgress() ?? new PlayerProgress();

        // ACTUALIZAR: Guardar el puntaje actual del GameManager
        if (GameManager.Instance != null)
        {
            progress.puntajeTotal = GameManager.Instance.GetPuntajeAcumulado();
            Debug.Log($"💰 Guardando puntaje total: {progress.puntajeTotal}");
        }

        // Guardar progreso cuando realmente se complete un nivel
        switch (currentScene)
        {
            case "Nivel2":
                progress.nivel1Completed = true;
                progress.lastSceneUnlocked = "Nivel2";
                break;
            case "Nivel3":
                progress.nivel1Completed = true;
                progress.nivel2Completed = true;
                progress.lastSceneUnlocked = "Nivel3";
                break;
            case "MenuFinal":
                progress.nivel1Completed = true;
                progress.nivel2Completed = true;
                progress.nivel3Completed = true;
                progress.juegoCompletado = true;
                progress.lastSceneUnlocked = "MenuFinal";
                break;
            case "MenuANivel2":
                progress.nivel1Completed = true;
                progress.lastSceneUnlocked = "MenuANivel2";
                break;
            case "MenuANivel3":
                progress.nivel1Completed = true;
                progress.nivel2Completed = true;
                progress.lastSceneUnlocked = "MenuANivel3";
                break;
        }

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath, FileMode.Create);

        formatter.Serialize(stream, progress);
        stream.Close();

        Debug.Log("💾 Progreso guardado: " + currentScene);
        Debug.Log($"📊 Nivel1: {progress.nivel1Completed}, Nivel2: {progress.nivel2Completed}, Nivel3: {progress.nivel3Completed}");
        Debug.Log($"🏆 Puntaje Total Guardado: {progress.puntajeTotal}");
    }

    public static PlayerProgress LoadProgress()
    {
        if (File.Exists(savePath))
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(savePath, FileMode.Open);

                PlayerProgress progress = formatter.Deserialize(stream) as PlayerProgress;
                stream.Close();

                Debug.Log("📂 Progreso cargado: " + progress.lastSceneUnlocked);
                Debug.Log($"🏆 Puntaje Total Cargado: {progress.puntajeTotal}");

                return progress;
            }
            catch (System.Exception e)
            {
                Debug.LogError("❌ Error cargando progreso: " + e.Message);
                return new PlayerProgress();
            }
        }
        else
        {
            Debug.Log("📭 No se encontró archivo de guardado. Usando progreso por defecto.");
            return null;
        }
    }

    public static void DeleteSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("🗑️ Progreso eliminado");
        }
    }

    public static bool SaveExists()
    {
        return File.Exists(savePath);
    }

    public static void ResetForNewGame()
    {
        PlayerProgress newProgress = new PlayerProgress();

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath, FileMode.Create);

        formatter.Serialize(stream, newProgress);
        stream.Close();

        Debug.Log("🆕 Nueva partida iniciada - progreso resetado");
    }
}