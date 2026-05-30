using UnityEngine;

public static class RootBossCombatPatterns
{
    public static RootBossAttackType[] PickMeleeCombo()//随机攻击连招
    {
        float roll = Random.value;

        if (roll < 0.18f)
            return new[] { RootBossAttackType.Attack1 };

        if (roll < 0.32f)
            return new[] { RootBossAttackType.Attack2 };

        if (roll < 0.52f)
            return new[] { RootBossAttackType.Attack1, RootBossAttackType.Attack2 };

        if (roll < 0.72f)
            return new[] { RootBossAttackType.Attack2, RootBossAttackType.Attack1 };

        if (roll < 0.88f)
            return new[] { RootBossAttackType.Attack1, RootBossAttackType.Attack2, RootBossAttackType.Attack4 };

        return new[] { RootBossAttackType.Attack4 };
    }
}
