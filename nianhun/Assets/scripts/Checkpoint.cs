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

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() != null && Input.GetKey(KeyCode.F))
        {
            ActiveCheckpoint();
            
            SaveManager.instance.SaveGame();
        }
    }

    public void ActiveCheckpoint()
    {
        if (activated)
            return;
        AudioManager.instance.PlaySFX(10, null);
        activated = true;
        anim.SetBool("active", true);
    }
}
