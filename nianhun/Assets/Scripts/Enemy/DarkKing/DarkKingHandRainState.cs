using System.Collections.Generic;
using UnityEngine;

public class DarkKingHandRainState : EnemyState
{
    private static readonly Color CommitWarningColor = new Color(1f, 0.05f, 0.05f, 0.75f);

    private enum Phase
    {
        Transport,
        Raining,
        Appear
    }

    private DarkKing enemy;
    private Phase phase;
    private float phaseElapsed;
    private float rainElapsed;
    private int handsSpawned;
    private int handsStruck;
    private bool visualsHidden;
    private readonly List<GhostHand> activeHands = new List<GhostHand>();

    private class GhostHand
    {
        public Vector2 position;
        public float warnAt;
        public float commitAt;
        public float strikeAt;
        public int index;
        public GameObject warning;
        public SpriteRenderer warningRenderer;
        public bool warned;
        public bool committed;
        public bool struck;
    }

    public DarkKingHandRainState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, DarkKing darkKing)
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

        phase = Phase.Transport;
        phaseElapsed = 0f;
        rainElapsed = 0f;
        handsSpawned = 0;
        handsStruck = 0;
        visualsHidden = false;
        activeHands.Clear();

        enemy.SetCombatVisible(true);
        enemy.anim.Play("transport", 0, 0f);
        AudioManager.instance.PlaySFX(32, null);
    }

    public override void exit()
    {
        enemy.isattack = false;
        enemy.zerovelocity();
        CleanupWarnings();
        if (visualsHidden)
            enemy.SetCombatVisible(true);
        if (enemy.Stat.isInvincible)
            enemy.Stat.MakeInvincible(false);
        activeHands.Clear();
    }

    public override void update()
    {
        base.update();
        enemy.zerovelocity();
        phaseElapsed += Time.deltaTime;

        switch (phase)
        {
            case Phase.Transport:
                if (triggercalled || phaseElapsed >= enemy.handRainOutDuration)
                    EnterRaining();
                break;

            case Phase.Raining:
                rainElapsed += Time.deltaTime;
                TryScheduleNextHand();
                UpdateHands();

                if (handsStruck >= enemy.handRainCount && activeHands.TrueForAll(h => h.struck))
                    EnterAppear();
                break;

            case Phase.Appear:
                if (triggercalled || phaseElapsed >= enemy.handRainAppearDuration)
                    statemachine.changestate(enemy.recoverystate);
                break;
        }
    }

    public override void aniamtionfinishtrigger()
    {
        triggercalled = true;
    }

    private void EnterRaining()
    {
        phase = Phase.Raining;
        phaseElapsed = 0f;
        rainElapsed = 0f;
        triggercalled = false;

        enemy.Stat.MakeInvincible(true);
        enemy.SetCombatVisible(false);
        visualsHidden = true;
    }

    private void TryScheduleNextHand()
    {
        if (handsSpawned >= enemy.handRainCount)
            return;

        float nextAt = handsSpawned * enemy.handRainInterval;
        if (rainElapsed < nextAt)
            return;

        Vector2 pos = enemy.GetGhostHandPositionNearPlayerX(enemy.handRainXJitter);
        int index = handsSpawned;
        handsSpawned++;

        float commitDelay = Mathf.Max(0f, enemy.handRainCommitDelay);
        float warnAt = rainElapsed;
        float commitAt = warnAt + enemy.handRainWarningDuration;

        activeHands.Add(new GhostHand
        {
            position = pos,
            index = index,
            warnAt = warnAt,
            commitAt = commitAt,
            strikeAt = commitAt + commitDelay
        });
    }

    private void UpdateHands()
    {
        for (int i = 0; i < activeHands.Count; i++)
        {
            GhostHand hand = activeHands[i];
            if (hand.struck)
                continue;

            if (!hand.warned && rainElapsed >= hand.warnAt)
            {
                hand.warned = true;
                hand.warning = DarkKingAttackWarning.Show(
                    hand.position,
                    enemy.handRainWarningSize,
                    enemy.handRainWarningDuration + enemy.handRainCommitDelay + 0.5f,
                    enemy.warningColor);
                hand.warningRenderer = hand.warning != null
                    ? hand.warning.GetComponent<SpriteRenderer>()
                    : null;
            }

            // 追踪阶段：红框跟着玩家
            if (hand.warned && !hand.committed && !hand.struck && hand.warning != null)
            {
                Vector2 track = enemy.GetGhostHandPositionNearPlayerX(enemy.handRainXJitter * 0.35f);
                hand.position = new Vector2(track.x, hand.position.y);
                hand.warning.transform.position = new Vector3(hand.position.x, hand.position.y, 0f);
            }

            // 锁定：停下 + 红色加深，再等 commitDelay 出爪
            if (!hand.committed && rainElapsed >= hand.commitAt)
            {
                hand.committed = true;
                if (hand.warningRenderer != null)
                    hand.warningRenderer.color = CommitWarningColor;
            }

            if (rainElapsed >= hand.strikeAt)
            {
                hand.struck = true;
                handsStruck++;
                if (hand.warning != null)
                {
                    Object.Destroy(hand.warning);
                    hand.warning = null;
                    hand.warningRenderer = null;
                }

                enemy.SpawnClawFx(hand.position);
                enemy.DealGhostHandDamage(
                    hand.position,
                    200 + hand.index,
                    enemy.handRainDamageMultiplier,
                    enemy.handRainHitRadius);
            }
        }
    }

    private void EnterAppear()
    {
        phase = Phase.Appear;
        phaseElapsed = 0f;
        triggercalled = false;
        CleanupWarnings();

        Vector2 land = enemy.GetGhostHandPositionNearPlayerX(0.5f);
        float groundY = enemy.GetCombatGroundY();
        enemy.transform.position = new Vector3(land.x, groundY, enemy.transform.position.z);
        enemy.SetCombatVisible(true);
        visualsHidden = false;
        enemy.Stat.MakeInvincible(false);
        enemy.FacePlayer();
        enemy.anim.Play("appear", 0, 0f);
        AudioManager.instance.PlaySFX(30, null);
    }

    private void CleanupWarnings()
    {
        for (int i = 0; i < activeHands.Count; i++)
        {
            if (activeHands[i].warning != null)
                Object.Destroy(activeHands[i].warning);
            activeHands[i].warning = null;
            activeHands[i].warningRenderer = null;
        }
    }
}
