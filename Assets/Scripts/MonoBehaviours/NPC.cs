using UnityEngine;

public class NPC : MonoBehaviour
{
    [Header("Configuración")]
    public string nombreNPC = "NPC";
    public string[] lineasDialogo = new string[] { "Hola, soy un NPC." };

    [Header("Referencias")]
    private DialogueManager dialogueManager;

    void Start()
    {
        // CAMBIA ESTA LÍNEA:
        // dialogueManager = FindObjectOfType<DialogueManager>();

        // POR ESTA (versión nueva):
        dialogueManager = FindFirstObjectByType<DialogueManager>();

        // O también puedes usar esta (otra alternativa):
        // dialogueManager = FindAnyObjectByType<DialogueManager>();
    }

    public void IniciarDialogo()
    {
        if (dialogueManager != null)
        {
            dialogueManager.IniciarDialogo(nombreNPC, lineasDialogo, this);
        }
    }

    public void TerminarDialogo()
    {
        // Vacío - se llama desde DialogueManager
    }

    // Detectar jugador
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // El MovementController manejará mostrar el botón
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // El MovementController manejará ocultar el botón
        }
    }
}