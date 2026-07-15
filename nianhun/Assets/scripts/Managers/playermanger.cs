using System.Collections;
using Cinemachine;
using UnityEngine;

public class playermanger : MonoBehaviour, ISaveManager
{
    public static playermanger instance;

    public Player player;

    public int currency;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            return;
        }

        if (instance == this)
            return;

        // 保留已挂好 Player 的那个实例，避免空 playermanger 抢走单例
        if (instance.player == null && player != null)
        {
            Destroy(instance.gameObject);
            instance = this;
            return;
        }

        if (instance.player != null && player == null)
        {
            Destroy(gameObject);
            return;
        }

        // 两边都有或都没有：合并必要引用后销毁重复体
        if (instance.player == null && player != null)
            instance.player = player;

        Destroy(gameObject);
    }

    private void Start()
    {
        // 若序列化引用丢失，尝试在场景里找回 Player
        if (player == null)
            player = FindObjectOfType<Player>();

        StartCoroutine(BindCameraFollowWhenReady());
    }

    private IEnumerator BindCameraFollowWhenReady()
    {
        for (int i = 0; i < 60; i++)
        {
            if (player == null)
                player = FindObjectOfType<Player>();

            if (player != null && TryBindCinemachineFollow(player.transform))
                yield break;

            yield return null;
        }
    }

    public static bool TryBindCinemachineFollow(Transform target)
    {
        if (target == null)
            return false;

        CinemachineVirtualCamera vcam = Object.FindObjectOfType<CinemachineVirtualCamera>();
        if (vcam == null)
            return false;

        if (vcam.Follow != target)
            vcam.Follow = target;

        return true;
    }

    public bool HaveEnoughMoney(int price)
    {
        if (price > currency)
        {
            Debug.Log("没有足够的钱");
            return false;
        }

        currency -= price;
        return true;
    }

    public int CurrentCurrencyAmount()
    {
        return currency;
    }

    public void LoadData(GameData data)
    {
        currency = data.currency;
    }

    public void SaveData(ref GameData data)
    {
        data.currency = currency;
    }
}
