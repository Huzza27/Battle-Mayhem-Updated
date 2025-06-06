using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CosmeticDataBase", menuName = "DataBase/CosmeticDataBase")]
public class CosmeticDatabase : ScriptableObject
{
    public List<CosmeticItem> cosmetics = new List<CosmeticItem>();

    public List<CosmeticItem> GetAllCosmeticsOfType(ItemType itemType)
    {
        return cosmetics.FindAll(item => item.type == itemType);
    }

    public CosmeticItem GetItem(ItemType type, int id)
    {
        List<CosmeticItem> filteredForType = GetAllCosmeticsOfType(type);

        return filteredForType.Find(item => item.ItemID == id);
    }

}
