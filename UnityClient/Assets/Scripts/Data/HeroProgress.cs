using System.Collections.Generic;

public class HeroProgress
{
    public int level = 1;
    public int xp    = 0;

    private HashSet<string> _unlockedIds = new();
    private List<string>    _equippedIds = new();

    public IReadOnlyList<string>       EquippedMoveIds => _equippedIds.AsReadOnly();
    public IReadOnlyCollection<string> UnlockedMoveIds => _unlockedIds;
    public int  XpToNextLevel => 50 * level;
    public bool HasFreeSlot   => _equippedIds.Count < 4;

    public void Initialize(List<string> starterMoves)
    {
        _unlockedIds = new HashSet<string>(starterMoves);
        _equippedIds = new List<string>(starterMoves);
    }

    public void AddXp(int amount)
    {
        xp += amount;
        while (xp >= XpToNextLevel) { xp -= XpToNextLevel; level++; }
    }

    public bool UnlockMove(string id)  => _unlockedIds.Add(id);
    public bool IsUnlocked(string id)  => _unlockedIds.Contains(id);
    public bool IsEquipped(string id)  => _equippedIds.Contains(id);

    public bool EquipMove(string id)
    {
        if (!HasFreeSlot || _equippedIds.Contains(id) || !_unlockedIds.Contains(id)) return false;
        _equippedIds.Add(id);
        return true;
    }

    public void UnequipMove(string id) => _equippedIds.Remove(id);

    public void LoadFrom(int savedLevel, int savedXp,
                         IEnumerable<string> unlocked, IEnumerable<string> equipped)
    {
        level        = savedLevel;
        xp           = savedXp;
        _unlockedIds = new HashSet<string>(unlocked);
        _equippedIds = new List<string>(equipped);
    }
}
