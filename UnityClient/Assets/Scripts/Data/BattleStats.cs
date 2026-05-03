using System.Collections.Generic;
using UnityEngine;

public class BattleStats
{
    public int  currentHp;
    public int  maxHp;
    public int  currentAtk;
    public int  currentDef;
    public int  currentMag;
    public int  stunTurnsRemaining;
    public bool isStunned => stunTurnsRemaining > 0;
    public List<ActiveModifier> activeModifiers = new List<ActiveModifier>();
    public DotEffect activeDot;

    public void Reset(BaseStats baseStats)
    {
        maxHp      = baseStats.hp;
        currentHp  = baseStats.hp;
        currentAtk = baseStats.atk;
        currentDef = baseStats.def;
        currentMag = baseStats.mag;
        stunTurnsRemaining = 0;
        activeModifiers.Clear();
        activeDot  = null;
    }

    public void ApplyModifier(StatChange change)
    {
        activeModifiers.Add(new ActiveModifier(change.stat, change.amount, change.duration));
        AdjustStat(change.stat, change.amount);
    }

    public void TickModifiers()
    {
        for (int i = activeModifiers.Count - 1; i >= 0; i--)
        {
            activeModifiers[i].turnsRemaining--;
            if (activeModifiers[i].turnsRemaining <= 0)
            {
                AdjustStat(activeModifiers[i].stat, -activeModifiers[i].amount);
                activeModifiers.RemoveAt(i);
            }
        }
    }

    public void TickDot()
    {
        if (activeDot == null) return;
        currentHp = Mathf.Max(0, currentHp - activeDot.damage);
        activeDot.duration--;
        if (activeDot.duration <= 0) activeDot = null;
    }

    public int GetEffectiveStat(string stat) => stat switch
    {
        "atk" => currentAtk,
        "def" => currentDef,
        "mag" => currentMag,
        "hp"  => currentHp,
        _ => 0
    };

    private void AdjustStat(string stat, int amount)
    {
        switch (stat)
        {
            case "atk": currentAtk = Mathf.Max(0, currentAtk + amount); break;
            case "def": currentDef = Mathf.Max(0, currentDef + amount); break;
            case "mag": currentMag = Mathf.Max(0, currentMag + amount); break;
            case "hp":  currentHp  = Mathf.Clamp(currentHp + amount, 0, maxHp); break;
        }
    }
}
