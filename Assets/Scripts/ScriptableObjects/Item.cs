using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string objectName;
    public Sprite sprite;
    public int quantity = 1;
    public bool stackable;
    public enum ItemType
    {
        COIN,
        HEALTH,
        TRASH
    }
    public ItemType itemType;
}