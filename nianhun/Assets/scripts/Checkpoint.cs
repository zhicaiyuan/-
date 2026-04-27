using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private Animator anim;
    public string id;
    public bool activated;

    private void Start()
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
        }
    }

    public void ActiveCheckpoint()
    {
        activated = true;
        anim.SetBool("active", true);
    }
}
