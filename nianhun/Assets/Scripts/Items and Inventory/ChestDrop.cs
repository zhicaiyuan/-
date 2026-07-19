using System.Collections.Generic;
using UnityEngine;

public class ChestDrop : MonoBehaviour, ISaveManager
{
    [Tooltip("默认留空。运行时按「场景名_坐标」自动生成，保证每个预制体实例互不串档。")]
    [SerializeField] private string id;
    [Tooltip("勾选后使用手动填写/生成的 id，不再按坐标自动覆盖。")]
    [SerializeField] private bool useManualId;

    [SerializeField] private int amountOfItem;
    [SerializeField] private ItemData[] Drop;
    private List<ItemData> dropList = new List<ItemData>();

    [SerializeField] private GameObject dropPrefab;
    [SerializeField] private Animator animator;
    private bool isOpen;
    private bool hasDropped;

    private void Awake()
    {
        EnsureStableId();
    }

    [ContextMenu("生成手动宝箱id")]
    private void GenerateManualId()
    {
        useManualId = true;
        id = System.Guid.NewGuid().ToString();
    }

    [ContextMenu("改为自动坐标id")]
    private void UseAutoId()
    {
        useManualId = false;
        id = string.Empty;
        EnsureStableId();
    }

    public void EnsureStableId()
    {
        if (useManualId && !string.IsNullOrEmpty(id))
            return;

        // 预制体资源本身不在有效场景里，避免把自动 id 写进 prefab
        if (!gameObject.scene.IsValid() || string.IsNullOrEmpty(gameObject.scene.name))
        {
            id = string.Empty;
            return;
        }

        id = $"{gameObject.scene.name}_{transform.position.x:F1}_{transform.position.y:F1}";
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
        EnsureStableId();
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
        EnsureStableId();

        if (string.IsNullOrEmpty(id) || data.openedChests == null)
            return;

        if (data.openedChests.TryGetValue(id, out bool opened) && opened)
            ApplyOpenedFromSave();
    }

    public void SaveData(ref GameData data)
    {
        EnsureStableId();

        if (string.IsNullOrEmpty(id) || !isOpen)
            return;

        if (data.openedChests == null)
            data.openedChests = new SerializableDictionary<string, bool>();

        data.openedChests[id] = true;
    }
}
