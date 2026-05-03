using UnityEngine;

public static class MoveEffectProcessor
{
    public static int ResolvePhysicalDamage(MoveData move, BattleStats atk, BattleStats def)
        => Mathf.Max(1, Mathf.RoundToInt(atk.currentAtk * move.atkMultiplier - def.currentDef * 0.5f));

    public static int ResolveMagicDamage(MoveData move, BattleStats atk)
        => Mathf.Max(1, Mathf.RoundToInt(atk.currentMag * move.magMultiplier));

    public static int ResolveHeal(MoveData move, BattleStats target)
        => Mathf.RoundToInt(target.maxHp * move.healPercent);

    public static void ApplyStatChanges(MoveData move, BattleStats target)
    {
        if (move.statChanges == null) return;
        foreach (var change in move.statChanges)
            target.ApplyModifier(change);
    }

    public static bool RollStun(MoveData move)
        => Random.value < move.stunChance;
}
