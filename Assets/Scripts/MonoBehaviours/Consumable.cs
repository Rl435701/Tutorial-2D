using UnityEngine;

public class Consumable : MonoBehaviour
{
    public Item item;

    // Método público para que otros scripts puedan consumir este objeto
    public bool Consumir(Player jugador)
    {
        if (item != null && jugador != null)
        {
            switch (item.itemType)
            {
                case Item.ItemType.HEALTH:
                    return jugador.AdjustHitPoints(item.quantity);
                case Item.ItemType.COIN:
                case Item.ItemType.TRASH:
                    return true;
                default:
                    return false;
            }
        }
        return false;
    }
}