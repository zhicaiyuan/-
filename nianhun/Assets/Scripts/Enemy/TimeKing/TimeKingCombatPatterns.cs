using System.Collections.Generic;
using UnityEngine;

public static class TimeKingCombatPatterns
{
    private static readonly TimeKingAttackType[][] ComboPoolPhase1 =
    {
        new[] { TimeKingAttackType.Attack1, TimeKingAttackType.Attack2 },
        new[] { TimeKingAttackType.Attack4, TimeKingAttackType.Attack2 },
        new[] { TimeKingAttackType.Attack4, TimeKingAttackType.Attack1 },
        new[] { TimeKingAttackType.Attack3, TimeKingAttackType.Attack1 }
    };

    private static readonly TimeKingAttackType[] SoloPoolPhase1 =
    {
        TimeKingAttackType.Attack1,
        TimeKingAttackType.Attack2,
        TimeKingAttackType.Attack3,
        TimeKingAttackType.Attack4
    };

    private static readonly TimeKingAttackType[][] ComboPoolPhase2 =
    {
        new[] { TimeKingAttackType.Attack1, TimeKingAttackType.Attack7 },
        new[] { TimeKingAttackType.Attack2, TimeKingAttackType.Attack6 },
        new[] { TimeKingAttackType.Attack3, TimeKingAttackType.Attack2, TimeKingAttackType.Attack6 },
        new[] { TimeKingAttackType.Attack5, TimeKingAttackType.Attack2 },
        new[] { TimeKingAttackType.Attack7, TimeKingAttackType.Attack6 },
        new[] { TimeKingAttackType.Attack4, TimeKingAttackType.Attack7 }
    };

    private static readonly TimeKingAttackType[] SoloPoolPhase2 =
    {
        TimeKingAttackType.Attack1,
        TimeKingAttackType.Attack2,
        TimeKingAttackType.Attack3,
        TimeKingAttackType.Attack5,
        TimeKingAttackType.Attack7
    };

    private static readonly TimeKingAttackType[] SoloPoolPhase3 =
    {
        TimeKingAttackType.Attack1,
        TimeKingAttackType.Attack2,
        TimeKingAttackType.Attack3,
        TimeKingAttackType.Attack5,
        TimeKingAttackType.Attack7,
        TimeKingAttackType.Strike,
        TimeKingAttackType.Dash,
        TimeKingAttackType.Spawn
    };

    // 三阶段单放权重：Strike 更高，更容易被选中
    private static int GetPhase3SoloWeight(TimeKingAttackType attackType)
    {
        switch (attackType)
        {
            case TimeKingAttackType.Strike:
                return 5;
            case TimeKingAttackType.Attack5:
            case TimeKingAttackType.Attack7:
                return 2;
            default:
                return 1;
        }
    }

    public static bool TryPickCombo(TimeKing boss, out TimeKingAttackType[] combo)
    {
        combo = null;

        TimeKingAttackType[][] comboPool = boss.IsPhase3 || boss.IsPhase2 ? ComboPoolPhase2 : ComboPoolPhase1;
        TimeKingAttackType[] soloPool =
            boss.IsPhase3 ? SoloPoolPhase3 :
            boss.IsPhase2 ? SoloPoolPhase2 :
            SoloPoolPhase1;

        List<TimeKingAttackType[]> readyCombos = new List<TimeKingAttackType[]>();
        foreach (TimeKingAttackType[] candidate in comboPool)
        {
            if (boss.IsComboExecutable(candidate))
                readyCombos.Add(candidate);
        }

        // 三阶段略降连招占比，给 Strike 等单放更多机会
        float comboChance = boss.IsPhase3 ? 0.5f : boss.IsPhase2 ? 0.7f : 0.4f;
        if (readyCombos.Count > 0 && Random.value < comboChance)
        {
            combo = readyCombos[Random.Range(0, readyCombos.Count)];
            return true;
        }

        List<TimeKingAttackType> readySolos = new List<TimeKingAttackType>();
        foreach (TimeKingAttackType solo in soloPool)
        {
            if (boss.IsSkillReady(solo))
                readySolos.Add(solo);
        }

        if (readySolos.Count > 0)
        {
            TimeKingAttackType picked = boss.IsPhase3
                ? PickWeightedSolo(readySolos)
                : readySolos[Random.Range(0, readySolos.Count)];
            combo = new[] { picked };
            return true;
        }

        if (readyCombos.Count > 0)
        {
            combo = readyCombos[Random.Range(0, readyCombos.Count)];
            return true;
        }

        return false;
    }

    private static TimeKingAttackType PickWeightedSolo(List<TimeKingAttackType> readySolos)
    {
        int totalWeight = 0;
        for (int i = 0; i < readySolos.Count; i++)
            totalWeight += GetPhase3SoloWeight(readySolos[i]);

        int roll = Random.Range(0, totalWeight);
        for (int i = 0; i < readySolos.Count; i++)
        {
            roll -= GetPhase3SoloWeight(readySolos[i]);
            if (roll < 0)
                return readySolos[i];
        }

        return readySolos[readySolos.Count - 1];
    }
}
