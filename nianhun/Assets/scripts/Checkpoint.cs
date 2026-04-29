using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private Animator anim;
    public string id;
    public bool activated;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    [ContextMenu("一般存档id")]
    private void GenerateId()
    {
        id = System.Guid.NewGuid().ToString();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() != null)
        {
            ActiveCheckpoint();
            SaveManager.instance.SaveGame();
        }
    }

    public void ActiveCheckpoint()
    {
        if (activated)
            return;

        activated = true;
        anim.SetBool("active", true);
    }
}
