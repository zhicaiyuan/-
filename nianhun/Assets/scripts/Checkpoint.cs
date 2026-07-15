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
        Player player = null;
        if (playermanger.instance != null)
            player = playermanger.instance.player;

        ApplyPrayReward(player);
    }

    public void ApplyPrayReward(Player player)
    {
        EnsureStableId();
        ActiveCheckpoint();

        CharaterStat stat = null;
        if (player != null)
            stat = player.Stat;
        if (stat == null && player != null)
            stat = player.GetComponent<PlayerStat>();

        if (stat != null)
            stat.IncreaseHealthBy(stat.Getmaxhealthvalue());
        else
            Debug.LogWarning("Checkpoint.ApplyPrayReward: 找不到玩家属性，跳过回血。", this);

        if (GameManager.instance != null)
            GameManager.instance.RegisterRespawnCheckpoint(id);

        if (SaveManager.instance != null)
            SaveManager.instance.SaveGame();
    }

    public void ActiveCheckpoint(bool playSound = true)
    {
        if (activated)
            return;

        if (playSound && AudioManager.instance != null)
            AudioManager.instance.PlaySFX(10, null);

        activated = true;
        if (anim != null)
            anim.SetBool("active", true);
    }
}
