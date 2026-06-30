using System.Collections.Generic;
using UnityEngine;

public class ChestDrop : MonoBehaviour, ISaveManager
{
    [SerializeField] private string id;
    [SerializeField] private int amountOfItem;
    [SerializeField] private ItemData[] Drop;
    private List<ItemData> dropList = new List<ItemData>();

    [SerializeField] private GameObject dropPrefab;
    [SerializeField] private Animator animator;
    private bool isOpen;
    private bool hasDropped;

    [ContextMenu("生成宝箱id")]
    private void GenerateId()
    {
        id = System.Guid.NewGuid().ToString();
    }

    public void GenerateDrop()
    {
        for (int i = 0; i < Drop.Length; i++)
            dropList.Add(Drop[i]);

        if (dropList.Count == 0)
        {
            Debug.Log("本次没有物品掉落");
            return;
        }

        for (int i = 0; i < amountOfItem; i++)
        {
            ItemData randomItem = dropList[i];
            dropList.Remove(randomItem);
            DropItem(randomItem);

            if (dropList.Count == 0)
                break;
        }
    }

    public void DropItem(ItemData itemdata)
    {
        GameObject newDrop = Instantiate(dropPrefab, transform.position, Quaternion.identity);
        Vector2 randomVelocity = new Vector2(Random.Range(-8, 8), Random.Range(15, 20));
        newDrop.GetComponent<ItemObject>().SetupItem(itemdata, randomVelocity);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isOpen)
            return;

        if (collision.GetComponent<Player>() != null && Input.GetKeyDown(KeyCode.F))
            OpenChest();
    }

    private void OpenChest()
    {
        isOpen = true;
        animator.SetBool("Open", true);
        AudioManager.instance.PlaySFX(34, null);
    }

    public void DropAfterAnimation()
    {
        if (hasDropped)
            return;

        hasDropped = true;
        GenerateDrop();
        HideOpenedChest();
        SaveManager.instance?.SaveGame();
    }

    private void HideOpenedChest()
    {
        gameObject.SetActive(false);
    }

    private void ApplyOpenedFromSave()
    {
        isOpen = true;
        hasDropped = true;
        HideOpenedChest();
    }

    public void LoadData(GameData data)
    {
        if (string.IsNullOrEmpty(id) || data.openedChests == null)
            return;

        if (data.openedChests.TryGetValue(id, out bool opened) && opened)
            ApplyOpenedFromSave();
    }

    public void SaveData(ref GameData data)
    {
        if (string.IsNullOrEmpty(id) || !isOpen)
            return;

        if (data.openedChests == null)
            data.openedChests = new SerializableDictionary<string, bool>();

        if (data.openedChests.TryGetValue(id, out _))
            data.openedChests[id] = true;
        else
            data.openedChests.Add(id, true);
    }
}
