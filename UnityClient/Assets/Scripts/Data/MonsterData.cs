using System.Collections.Generic;
using Newtonsoft.Json;

[System.Serializable]
public class MonsterData
{
    [JsonProperty("id")]          public string id;
    [JsonProperty("name")]        public string name;
    [JsonProperty("description")] public string description;
    [JsonProperty("sprite_row")]  public int spriteRow;
    [JsonProperty("sprite_col")]  public int spriteCol;
    [JsonProperty("stats")]       public BaseStats stats;
    [JsonProperty("moves")]       public List<string> moves;
    [JsonProperty("xp_reward")]   public int xpReward;
    [JsonProperty("difficulty")]  public int difficulty;
}
