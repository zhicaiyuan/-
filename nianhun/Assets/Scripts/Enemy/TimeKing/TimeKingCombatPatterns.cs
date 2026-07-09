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

    public static bool TryPickCombo(TimeKing boss, out TimeKingAttackType[] combo)
    {
        combo = null;

        TimeKingAttackType[][] comboPool = boss.IsPhase2 ? ComboPoolPhase2 : ComboPoolPhase1;
        TimeKingAttackType[] soloPool = boss.IsPhase2 ? SoloPoolPhase2 : SoloPoolPhase1;

        List<TimeKingAttackType[]> readyCombos = new List<TimeKingAttackType[]>();
        foreach (TimeKingAttackType[] candidate in comboPool)
        {
            if (boss.IsComboExecutable(candidate))
                readyCombos.Add(candidate);
        }

        // 二阶段更偏向连招（Attack6 只出现在连招里）；一阶段保持约 40%
        float comboChance = boss.IsPhase2 ? 0.7f : 0.4f;
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
            combo = new[] { readySolos[Random.Range(0, readySolos.Count)] };
            return true;
        }

        // 单放都不可用时，仍尝试可用连招，避免空等
        if (readyCombos.Count > 0)
        {
            combo = readyCombos[Random.Range(0, readyCombos.Count)];
            return true;
        }

        return false;
    }
}
