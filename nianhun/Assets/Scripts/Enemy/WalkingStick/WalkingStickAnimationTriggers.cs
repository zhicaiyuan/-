using UnityEngine;

public class WalkingStickAnimationTriggers : MonoBehaviour
{
    private WalkingStick enemy => GetComponentInParent<WalkingStick>();

    private void aniamtiontrigger()
    {
        enemy.animationfinishtrigger();
    }

    private void attacktrigger()
    {
        if (enemy != null)
            enemy.DealAttackDamage(enemy.CurrentAttack);
    }

    private void opencounterwindow() => enemy.opencounterattackwindow();

    private void closecounterwindow() => enemy.closecounterattackwindow();
}
