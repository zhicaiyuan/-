using UnityEngine;

[DisallowMultipleComponent]
public class BattleRoomTrackedEnemy : MonoBehaviour
{
    private BattleRoomController room;
    private Enemy enemy;
    private bool notified;

    public void Init(BattleRoomController controller)
    {
        room = controller;
        enemy = GetComponent<Enemy>();
    }

    private void Update()
    {
        if (notified || enemy == null)
            return;

        if (enemy.isDead)
            NotifyDefeated();
    }

    private void OnDestroy()
    {
        if (!notified)
            NotifyDefeated();
    }

    private void NotifyDefeated()
    {
        if (notified)
            return;

        notified = true;
        room?.NotifyEnemyDefeated(this);
    }
}
