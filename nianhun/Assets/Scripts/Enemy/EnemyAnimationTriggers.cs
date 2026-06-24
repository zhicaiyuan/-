using UnityEngine;

public class EnemyAnimationTriggers : MonoBehaviour
{
    private Enemy enemy => GetComponentInParent<Enemy>();

    private void aniamtiontrigger()
    {
        enemy.animationfinishtrigger();
    }

    private void attacktrigger()
    {
        enemy.DealDamageToDetectedPlayers();
    }

    private void opencounterwindow() => enemy.opencounterattackwindow();

    private void closecounterwindow() => enemy.closecounterattackwindow();
}
