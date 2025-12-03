using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("Referencias")]
    public Player character;
    public Image meterImage;
    public Text hpText;

    [Header("Configuración")]
    public bool autoFindPlayer = true;

    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("UI");

        if (character == null && autoFindPlayer)
        {
            FindPlayer();
        }
    }

    void Update()
    {
        if (character != null && character.hitPoints != null)
        {
            UpdateHealthBar();
        }
        else if (autoFindPlayer && character == null)
        {
            FindPlayer();
        }
    }

    void UpdateHealthBar()
    {
        if (meterImage != null)
        {
            float healthPercentage = character.hitPoints.value / (float)character.maxHitPoints;
            meterImage.fillAmount = healthPercentage;

            if (hpText != null)
            {
                hpText.text = "HP:" + Mathf.RoundToInt(healthPercentage * 100);
            }
        }
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            character = playerObj.GetComponent<Player>();
        }
    }
}