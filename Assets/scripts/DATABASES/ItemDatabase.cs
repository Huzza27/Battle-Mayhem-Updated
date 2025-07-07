using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDataBase", menuName = "DataBase/ItemDataBase")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<Item> crateItems;
    [SerializeField] private List<Item> defaultItems;

    public Item GetRandomCrateItem()
    {
        return crateItems[Random.Range(0, crateItems.Count-1)];
    }

    public int GetIdexOfCrateItem(Item item)
    {
        return crateItems.IndexOf(item);
    }
    public Item GetItemAtIndex(int index)
    {
        return crateItems[index];
    }
}
