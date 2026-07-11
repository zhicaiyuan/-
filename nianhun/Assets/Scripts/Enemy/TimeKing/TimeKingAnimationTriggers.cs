using UnityEngine;

public class TimeKingAnimationTriggers : MonoBehaviour
{
    private TimeKing enemy => GetComponentInParent<TimeKing>();

    private void aniamtiontrigger()
    {
        if (enemy != null)
            enemy.animationfinishtrigger();
    }

    private void attacktrigger()
    {
        if (enemy == null)
            return;

        if (enemy.statemachine.currentstate is TimeKingAttackState attackState)
            attackState.ApplySingleHit();
        else if (enemy.statemachine.currentstate is TimeKingJumpAttackState jumpState)
            jumpState.ApplyHit();
        else if (enemy.statemachine.currentstate is TimeKingDashState dashState)
            dashState.ApplyDashHit();
    }

    private void attack1hit1() => ApplySegmentHit(0);
    private void attack1hit2() => ApplySegmentHit(1);
    private void attack1hit3() => ApplySegmentHit(2);
    private void attack2hit1() => ApplySingleHit();
    private void attack3hit1() => ApplySegmentHit(0);
    private void attack3hit2() => ApplySegmentHit(1);
    private void attack3hit3() => ApplySegmentHit(2);
    private void attack3hit4() => ApplySegmentHit(3);
    private void attack3hit5() => ApplySegmentHit(4);
    private void attack4hit1() => ApplySegmentHit(0);
    private void attack4hit2() => ApplySegmentHit(1);
    private void attack4hit3() => ApplySegmentHit(2);
    private void attack4hit4() => ApplySegmentHit(3);
    private void attack5hit1() => ApplySegmentHit(0);
    private void attack5hit2() => ApplySegmentHit(1);
    private void attack5hit3() => ApplySegmentHit(2);
    private void attack6hit1() => ApplySegmentHit(0);
    private void attack6hit2() => ApplySegmentHit(1);
    private void attack7hit1() => ApplySegmentHit(0);
    private void attack7hit2() => ApplySegmentHit(1);
    private void strikehit1() => ApplySingleHit();
    private void dashhit1()
    {
        if (enemy != null && enemy.statemachine.currentstate is TimeKingDashState dashState)
            dashState.ApplyDashHit();
    }

    private void ApplySegmentHit(int segmentIndex)
    {
        if (enemy == null || enemy.statemachine.currentstate is not TimeKingAttackState attackState)
            return;

        attackState.ApplySegmentHit(segmentIndex);
    }

    private void ApplySingleHit()
    {
        if (enemy == null || enemy.statemachine.currentstate is not TimeKingAttackState attackState)
            return;

        attackState.ApplySingleHit();
    }

    private void opencounterwindow() => enemy.opencounterattackwindow();

    private void closecounterwindow() => enemy.closecounterattackwindow();
}
