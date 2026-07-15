using UnityEngine;

public class ItemObjectTrigger : MonoBehaviour
{
    private ItemObject myItemObject;

    private void Awake()
    {
        myItemObject = GetComponentInParent<ItemObject>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (myItemObject == null)
            myItemObject = GetComponentInParent<ItemObject>();

        if (myItemObject == null)
            return;

        if (collision.GetComponent<Player>() != null || collision.GetComponentInParent<Player>() != null)
            myItemObject.PickUpItem();
    }
}
