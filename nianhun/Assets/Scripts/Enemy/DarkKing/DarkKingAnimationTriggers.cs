using UnityEngine;

public class DarkKingAnimationTriggers : MonoBehaviour
{
    private DarkKing enemy => GetComponentInParent<DarkKing>();

    private void aniamtiontrigger()
    {
        if (enemy != null)
            enemy.animationfinishtrigger();
    }

    private void attacktrigger()
    {
        if (enemy == null)
            return;

        if (enemy.statemachine.currentstate is DarkKingAttackState attackState)
            attackState.ApplyHit();
        else if (enemy.statemachine.currentstate is DarkKingTeleportState teleportState)
            teleportState.ApplyHit();
    }

    private void clawhit()
    {
        // 鬼手伤害由状态时间驱动；事件留作备份入口
        attacktrigger();
    }
    private void opencounterwindow() => enemy.opencounterattackwindow();

    private void closecounterwindow() => enemy.closecounterattackwindow();
}
