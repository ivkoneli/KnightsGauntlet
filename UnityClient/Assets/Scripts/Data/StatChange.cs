using Newtonsoft.Json;

[System.Serializable]
public class StatChange
{
    [JsonProperty("stat")]     public string stat;
    [JsonProperty("amount")]   public int amount;
    [JsonProperty("duration")] public int duration;
}
