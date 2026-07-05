using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private Animator anim;
    public string id;
    public bool activated;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        EnsureStableId();
    }

    [ContextMenu("一般存档id")]
    private void GenerateId()
    {
        id = System.Guid.NewGuid().ToString();
    }

    public void EnsureStableId()
    {
        if (!string.IsNullOrEmpty(id))
            return;

        id = $"{gameObject.scene.name}_{transform.position.x:F1}_{transform.position.y:F1}";
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
        EnsureStableId();
        ActiveCheckpoint();

        PlayerStat stat = playermanger.instance.player.GetComponent<PlayerStat>();
        stat.IncreaseHealthBy(stat.Getmaxhealthvalue());

        if (GameManager.instance != null)
            GameManager.instance.RegisterRespawnCheckpoint(id);

        SaveManager.instance.SaveGame();
    }

    public void ActiveCheckpoint(bool playSound = true)
    {
        if (activated)
            return;

        if (playSound)
            AudioManager.instance.PlaySFX(10, null);

        activated = true;
        anim.SetBool("active", true);
    }
}
