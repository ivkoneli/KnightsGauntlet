using System.Collections.Generic;
using Newtonsoft.Json;

[System.Serializable]
public class HeroConfig
{
    [JsonProperty("id")]               public string id;
    [JsonProperty("name")]             public string name;
    [JsonProperty("base_stats")]       public BaseStats baseStats;
    [JsonProperty("stats_per_level")]  public BaseStats statsPerLevel;
    [JsonProperty("starter_moves")]    public List<string> starterMoves;
    [JsonProperty("max_moves")]        public int maxMoves;
}
