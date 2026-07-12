using System.Collections.Generic;
using UnityEngine;

public static class DarkKingCombatPatterns
{
    public static bool TryPickSkill(DarkKing boss, float distanceToPlayer, out DarkKingAttackType skill)
    {
        skill = DarkKingAttackType.Attack;

        // Phase3 HandRain（偏远时更优先）
        if (boss.IsPhase3 &&
            boss.IsSkillReady(DarkKingAttackType.HandRain) &&
            distanceToPlayer > boss.attackcheckdistance)
        {
            skill = DarkKingAttackType.HandRain;
            return true;
        }

        // Phase3 SummonHands
        if (boss.IsPhase3 &&
            boss.IsSkillReady(DarkKingAttackType.SummonHands) &&
            distanceToPlayer <= boss.summonHandsMaxDistance)
        {
            skill = DarkKingAttackType.SummonHands;
            return true;
        }

        // Phase2+ Teleport（近身/远程均可，CD 好就放）
        if (boss.IsPhase2 &&
            boss.IsSkillReady(DarkKingAttackType.Teleport))
        {
            skill = DarkKingAttackType.Teleport;
            return true;
        }

        // Melee attack
        if (distanceToPlayer <= boss.attackcheckdistance &&
            boss.IsSkillReady(DarkKingAttackType.Attack))
        {
            skill = DarkKingAttackType.Attack;
            return true;
        }

        return false;
    }
}
