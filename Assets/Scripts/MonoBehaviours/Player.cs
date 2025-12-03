using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    public HealthBar healthBarPrefab;
    private HealthBar healthBar;

    // ✅ NUEVO: Evento para notificar la muerte del jugador
    public event Action OnDeath;

    // Evento para notificar cambios en la vida
    public System.Action<int> OnHealthChanged;

    protected override void Start()
    {
        base.Start();

        // En lugar de instanciar, buscar la HealthBar existente en la escena
        HealthBar existingHealthBar = FindAnyObjectByType<HealthBar>();
        if (existingHealthBar != null)
        {
            healthBar = existingHealthBar;
            healthBar.character = this;
            Debug.Log("✅ HealthBar existente asignada al jugador");
        }
        else
        {
            Debug.LogError("❌ No se encontró HealthBar en la escena");
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("CanBePickedUp"))
        {
            Consumable consumable = collision.gameObject.GetComponent<Consumable>();
            if (consumable != null && consumable.item != null)
            {
                Item hitObject = consumable.item;
                Debug.Log("Recogiendo: " + hitObject.objectName);

                bool shouldDisappear = false;

                switch (hitObject.itemType)
                {
                    case Item.ItemType.COIN:
                        shouldDisappear = true;
                        break;

                    case Item.ItemType.HEALTH:
                        Debug.Log("Cantidad a Incrementar: " + hitObject.quantity);
                        shouldDisappear = AdjustHitPoints(hitObject.quantity);
                        break;

                    case Item.ItemType.TRASH:
                        shouldDisappear = true;
                        break;
                }

                if (shouldDisappear)
                {
                    Destroy(collision.gameObject);
                }
            }
        }
    }

    public bool AdjustHitPoints(int amount)
    {
        if (hitPoints == null)
        {
            Debug.LogError("Error: hitPoints no está inicializado");
            return false;
        }

        if (maxHitPoints <= 0)
        {
            Debug.LogError("Error: maxHitPoints no tiene un valor válido");
            return false;
        }

        int vidaAnterior = hitPoints.value;

        if (amount > 0) // Curar
        {
            if (hitPoints.value < maxHitPoints)
            {
                hitPoints.value = Mathf.Min(hitPoints.value + amount, maxHitPoints);
                Debug.Log("Curando: " + amount + ". Nuevo Valor: " + hitPoints.value);

                OnHealthChanged?.Invoke(hitPoints.value);
                return true;
            }
            else
            {
                Debug.Log("Vida ya está al máximo, no se puede curar más");
                return false;
            }
        }
        else if (amount < 0) // Recibir daño
        {
            hitPoints.value = Mathf.Max(hitPoints.value + amount, 0);
            Debug.Log("Recibiendo daño: " + Mathf.Abs(amount) + ". Nuevo Valor: " + hitPoints.value);

            OnHealthChanged?.Invoke(hitPoints.value);

            // ✅ NUEVO: Verificar si murió después del daño
            if (hitPoints.value <= 0)
            {
                Morir();
            }

            return true;
        }

        return false;
    }

    public void AjustarVida(int cantidad)
    {
        AdjustHitPoints(cantidad);
    }

    // Método para recibir daño desde otros componentes
    public void RecibirDano(int cantidadDano)
    {
        AjustarVida(-cantidadDano);
    }

    // ✅ NUEVO: Método de muerte mejorado
    private void Morir()
    {
        Debug.Log("Player ha muerto");

        // Deshabilitar movimiento
        MovementController movement = GetComponent<MovementController>();
        if (movement != null)
        {
            movement.Morir();
        }

        // ✅ DISPARAR EVENTO DE MUERTE
        OnDeath?.Invoke();

        // Opcional: agregar efectos de muerte
        StartCoroutine(EfectosMuerte());
    }

    // ✅ NUEVO: Corrutina para efectos de muerte
    private IEnumerator EfectosMuerte()
    {
        // Ejemplo: hacer que el personaje parpadee
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            for (int i = 0; i < 5; i++)
            {
                spriteRenderer.enabled = false;
                yield return new WaitForSeconds(0.1f);
                spriteRenderer.enabled = true;
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}