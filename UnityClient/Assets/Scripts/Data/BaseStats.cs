using Newtonsoft.Json;

[System.Serializable]
public class BaseStats
{
    [JsonProperty("atk")] public int atk;
    [JsonProperty("def")] public int def;
    [JsonProperty("hp")]  public int hp;
    [JsonProperty("mag")] public int mag;
}
