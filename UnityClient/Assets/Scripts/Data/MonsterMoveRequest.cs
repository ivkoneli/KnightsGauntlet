using System.Collections.Generic;
using Newtonsoft.Json;

public class MonsterMoveRequest
{
    public string       monsterId;
    public int          monsterHp;
    public int          monsterMaxHp;
    public int          heroHp;
    public int          heroMaxHp;
    public int          heroAtk;
    public int          heroDef;
    public int          heroMag;
    public string       lastMove;
    public int          turn;
    public List<string> cooldownMoves;   // move ids the monster cannot use this turn

    public string ToQueryString()
    {
        string last = string.IsNullOrEmpty(lastMove) ? "" : lastMove;
        string cd   = (cooldownMoves != null && cooldownMoves.Count > 0)
                        ? "&cooldown_moves=" + string.Join(",", cooldownMoves)
                        : "";
        return $"?monster_id={monsterId}" +
               $"&monster_hp={monsterHp}" +
               $"&monster_max_hp={monsterMaxHp}" +
               $"&hero_hp={heroHp}" +
               $"&hero_max_hp={heroMaxHp}" +
               $"&hero_atk={heroAtk}" +
               $"&hero_def={heroDef}" +
               $"&hero_mag={heroMag}" +
               $"&last_move={last}" +
               $"&turn={turn}" +
               cd;
    }
}

public class MonsterMoveResponse
{
    [JsonProperty("move_id")] public string moveId;
}
