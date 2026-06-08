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
        Player player = collision.GetComponent<Player>();
        if (player != null)
            player.SetNearbyCheckpoint(this);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        if (player != null)
            player.ClearNearbyCheckpoint(this);
    }

    public void ApplyPrayReward()
    {
        ActiveCheckpoint();
        PlayerStat stat = playermanger.instance.player.GetComponent<PlayerStat>();
        stat.IncreaseHealthBy(stat.Getmaxhealthvalue());
        SaveManager.instance.SaveGame();
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
