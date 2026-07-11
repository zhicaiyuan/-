using System.Collections.Generic;
using UnityEngine;

public class TimeKingSpawnState : EnemyState
{
    private TimeKing enemy;
    private float elapsed;
    private bool boltsStarted;
    private readonly List<LightningBolt> bolts = new List<LightningBolt>();

    private class LightningBolt
    {
        public Vector2 position;
        public float warnAt;
        public float strikeAt;
        public GameObject warning;
        public bool warned;
        public bool struck;
    }

    public TimeKingSpawnState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, TimeKing timeKing)
        : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = timeKing;
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
        boltsStarted = false;
        bolts.Clear();

        enemy.anim.Play("spawn", 0, 0f);
        statetimer = enemy.spawnDuration;
        AudioManager.instance.PlaySFX(30, null);

        ScheduleBolts();
    }

    public override void exit()
    {
        enemy.isattack = false;
        enemy.zerovelocity();
        CleanupWarnings();
        bolts.Clear();
    }

    public override void update()
    {
        base.update();

        if (enemy.TryStartPhaseTransition())
            return;

        enemy.zerovelocity();
        elapsed += Time.deltaTime;
        UpdateBolts();

        bool boltsDone = AllBoltsStruck();
        if (!triggercalled && boltsDone && (statetimer <= 0f || elapsed >= GetExpectedEndTime()))
            triggercalled = true;

        if (!boltsDone)
            return;

        if (!triggercalled)
            return;

        triggercalled = false;

        if (enemy.TryEnterChaseAfterAttack())
            return;

        if (enemy.AdvanceAttackCombo())
            enemy.EnterAttackForCurrentSkill();
        else
            statemachine.changestate(enemy.recoverystate);
    }

    public override void aniamtionfinishtrigger()
    {
        if (AllBoltsStruck())
            triggercalled = true;
    }

    private float GetExpectedEndTime()
    {
        float end = enemy.spawnStartDelay + enemy.spawnWarningDuration;
        if (enemy.spawnLightningCount > 1)
            end += (enemy.spawnLightningCount - 1) * enemy.spawnStaggerDelay;
        return end + 0.05f;
    }

    private void ScheduleBolts()
    {
        boltsStarted = true;
        int count = Mathf.Max(2, enemy.spawnLightningCount);
        // 保证偶数，左右各半
        if (count % 2 != 0)
            count++;

        int perSide = count / 2;
        for (int i = 0; i < count; i++)
        {
            bool leftSide = i < perSide;
            float side = leftSide ? -1f : 1f;
            int indexOnSide = leftSide ? i : i - perSide;

            float x = side * Mathf.Lerp(enemy.spawnSideMinDistance, enemy.spawnRadius, Random.Range(0.35f, 1f));
            float y = Random.Range(-enemy.spawnVerticalRange, enemy.spawnVerticalRange);
            Vector2 pos = (Vector2)enemy.transform.position + new Vector2(x, y);
            float stagger = indexOnSide * enemy.spawnStaggerDelay + (leftSide ? 0f : enemy.spawnStaggerDelay * 0.5f);

            bolts.Add(new LightningBolt
            {
                position = pos,
                warnAt = enemy.spawnStartDelay + stagger,
                strikeAt = enemy.spawnStartDelay + stagger + enemy.spawnWarningDuration,
                warning = null,
                warned = false,
                struck = false
            });
        }
    }

    private void UpdateBolts()
    {
        if (!boltsStarted)
            return;

        for (int i = 0; i < bolts.Count; i++)
        {
            LightningBolt bolt = bolts[i];
            if (bolt.struck)
                continue;

            if (!bolt.warned && elapsed >= bolt.warnAt)
            {
                bolt.warned = true;
                bolt.warning = TimeKingAttackWarning.Show(
                    bolt.position,
                    enemy.spawnWarningSize,
                    enemy.spawnWarningDuration + 0.1f,
                    enemy.warningColor);
            }

            if (elapsed >= bolt.strikeAt)
            {
                bolt.struck = true;
                if (bolt.warning != null)
                {
                    Object.Destroy(bolt.warning);
                    bolt.warning = null;
                }

                enemy.SpawnThunderFx(bolt.position);
                enemy.DealSpawnLightningDamage(bolt.position, i);
            }
        }
    }

    private bool AllBoltsStruck()
    {
        if (bolts.Count == 0)
            return false;

        for (int i = 0; i < bolts.Count; i++)
        {
            if (!bolts[i].struck)
                return false;
        }

        return true;
    }

    private void CleanupWarnings()
    {
        for (int i = 0; i < bolts.Count; i++)
        {
            if (bolts[i].warning != null)
                Object.Destroy(bolts[i].warning);
            bolts[i].warning = null;
        }
    }
}
