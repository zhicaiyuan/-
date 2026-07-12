using System.Collections.Generic;
using UnityEngine;

public class DarkKingSummonHandsState : EnemyState
{
    private static readonly Color CommitWarningColor = new Color(1f, 0.05f, 0.05f, 0.75f);

    private DarkKing enemy;
    private float elapsed;
    private readonly List<GhostHand> hands = new List<GhostHand>();

    private class GhostHand
    {
        public Vector2 position;
        public float warnAt;
        public float commitAt;
        public float strikeAt;
        public GameObject warning;
        public SpriteRenderer warningRenderer;
        public bool warned;
        public bool committed;
        public bool struck;
    }

    public DarkKingSummonHandsState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, DarkKing darkKing)
        : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = darkKing;
    }

    public override void enter()
    {
        triggercalled = false;
        rb = enemybase.rb;
        enemy.isattack = true;
        enemy.zerovelocity();
        enemy.ResetAttackHitTracking();
        enemy.FacePlayer();

        elapsed = 0f;
        hands.Clear();
        ScheduleHands();

        enemy.anim.Play("spawn", 0, 0f);
        statetimer = enemy.summonHandsDuration;
        AudioManager.instance.PlaySFX(30, null);
    }

    public override void exit()
    {
        enemy.isattack = false;
        enemy.zerovelocity();
        CleanupWarnings();
        hands.Clear();
    }

    public override void update()
    {
        base.update();
        enemy.zerovelocity();
        elapsed += Time.deltaTime;
        UpdateHands();

        bool done = AllStruck();
        if (!triggercalled && done && (statetimer <= 0f || elapsed >= GetExpectedEnd()))
            triggercalled = true;

        if (!done)
            return;

        if (!triggercalled)
            return;

        triggercalled = false;
        statemachine.changestate(enemy.recoverystate);
    }

    public override void aniamtionfinishtrigger()
    {
        if (AllStruck())
            triggercalled = true;
    }

    private void ScheduleHands()
    {
        int count = Mathf.Max(1, enemy.summonHandsCount);
        float commitDelay = Mathf.Max(0f, enemy.summonHandsCommitDelay);

        for (int i = 0; i < count; i++)
        {
            float stagger = i * enemy.summonHandsStagger;
            float warnAt = enemy.summonHandsStartDelay + stagger;
            float commitAt = warnAt + enemy.summonHandsWarningDuration;
            Vector2 pos = enemy.GetGhostHandPositionNearPlayerX(enemy.summonHandsXRange);
            hands.Add(new GhostHand
            {
                position = pos,
                warnAt = warnAt,
                commitAt = commitAt,
                strikeAt = commitAt + commitDelay
            });
        }
    }

    private void UpdateHands()
    {
        Vector2 playerTarget = GetFollowTarget();

        for (int i = 0; i < hands.Count; i++)
        {
            GhostHand hand = hands[i];
            if (hand.struck)
                continue;

            if (!hand.warned && elapsed >= hand.warnAt)
            {
                hand.warned = true;
                hand.position = playerTarget;
                hand.warning = DarkKingAttackWarning.Show(
                    hand.position,
                    enemy.summonHandsWarningSize,
                    enemy.summonHandsWarningDuration + enemy.summonHandsCommitDelay + 0.5f,
                    enemy.warningColor);
                hand.warningRenderer = hand.warning != null
                    ? hand.warning.GetComponent<SpriteRenderer>()
                    : null;
            }

            // 追踪阶段：慢速追玩家
            if (hand.warned && !hand.committed && !hand.struck)
            {
                hand.position = Vector2.MoveTowards(
                    hand.position,
                    playerTarget,
                    enemy.summonHandsFollowSpeed * Time.deltaTime);

                if (hand.warning != null)
                    hand.warning.transform.position = new Vector3(hand.position.x, hand.position.y, 0f);
            }

            // 锁定：红色加深，停止追随，再等 commitDelay 出爪
            if (!hand.committed && elapsed >= hand.commitAt)
            {
                hand.committed = true;
                if (hand.warningRenderer != null)
                    hand.warningRenderer.color = CommitWarningColor;
            }

            if (elapsed >= hand.strikeAt)
            {
                hand.struck = true;
                if (hand.warning != null)
                {
                    Object.Destroy(hand.warning);
                    hand.warning = null;
                    hand.warningRenderer = null;
                }

                enemy.SpawnClawFx(hand.position);
                enemy.DealGhostHandDamage(
                    hand.position,
                    i,
                    enemy.summonHandsDamageMultiplier,
                    enemy.summonHandsHitRadius);
            }
        }
    }

    private Vector2 GetFollowTarget()
    {
        if (playermanger.instance == null || playermanger.instance.player == null)
            return enemy.transform.position;

        Transform player = playermanger.instance.player.transform;
        return new Vector2(player.position.x, enemy.transform.position.y);
    }

    private float GetExpectedEnd()
    {
        float end = enemy.summonHandsStartDelay
            + enemy.summonHandsWarningDuration
            + Mathf.Max(0f, enemy.summonHandsCommitDelay);
        if (enemy.summonHandsCount > 1)
            end += (enemy.summonHandsCount - 1) * enemy.summonHandsStagger;
        return end + 0.05f;
    }

    private bool AllStruck()
    {
        if (hands.Count == 0)
            return false;

        for (int i = 0; i < hands.Count; i++)
        {
            if (!hands[i].struck)
                return false;
        }

        return true;
    }

    private void CleanupWarnings()
    {
        for (int i = 0; i < hands.Count; i++)
        {
            if (hands[i].warning != null)
                Object.Destroy(hands[i].warning);
            hands[i].warning = null;
            hands[i].warningRenderer = null;
        }
    }
}
