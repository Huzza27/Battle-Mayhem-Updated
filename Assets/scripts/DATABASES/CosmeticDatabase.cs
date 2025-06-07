using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CosmeticDataBase", menuName = "DataBase/CosmeticDataBase")]
public class CosmeticDatabase : ScriptableObject
{
    public List<CosmeticItem> cosmetics;

    public List<CosmeticItem> GetAllCosmeticsOfType(ItemType itemType)
    {
        if (cosmetics == null)
        {
            Debug.LogError("Cosmetic list is null!");
            return new List<CosmeticItem>();
        }

        return cosmetics.FindAll(item =>
        {
            if (item == null)
            {
                Debug.LogWarning("Null item found in cosmetics list.");
                return false;
            }
            return item.type == itemType;
        });
    }


    public CosmeticItem GetItem(ItemType type, int id)
    {
        List<CosmeticItem> filteredForType = GetAllCosmeticsOfType(type);

        return filteredForType.Find(item => item.ItemID == id);
    }

}
