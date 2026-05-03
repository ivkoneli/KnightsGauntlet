using System.Collections.Generic;
using Newtonsoft.Json;

[System.Serializable]
public class MoveData
{
    [JsonProperty("id")]              public string id;
    [JsonProperty("name")]            public string name;
    [JsonProperty("description")]     public string description;
    [JsonProperty("type")]            public string type;
    [JsonProperty("target")]          public string target;
    [JsonProperty("atk_multiplier")]  public float atkMultiplier;
    [JsonProperty("mag_multiplier")]  public float magMultiplier;
    [JsonProperty("heal_percent")]    public float healPercent;
    [JsonProperty("stat_changes")]    public List<StatChange> statChanges;
    [JsonProperty("dot")]             public DotEffect dot;
    [JsonProperty("lifesteal")]       public bool lifesteal;
    [JsonProperty("self_heal")]       public int selfHeal;
    [JsonProperty("self_damage")]     public int selfDamage;
    [JsonProperty("stun_chance")]     public float stunChance;
    [JsonProperty("cooldown")]        public int cooldown;
    [JsonProperty("icon")]            public string icon;
}
