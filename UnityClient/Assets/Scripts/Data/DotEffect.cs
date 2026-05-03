using Newtonsoft.Json;

[System.Serializable]
public class DotEffect
{
    [JsonProperty("damage")]   public int damage;
    [JsonProperty("duration")] public int duration;
    [JsonProperty("type")]     public string type;
}
