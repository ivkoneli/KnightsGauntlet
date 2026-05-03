using System.Collections.Generic;
using Newtonsoft.Json;

[System.Serializable]
public class GameConfig
{
    [JsonProperty("hero")]      public HeroConfig hero;
    [JsonProperty("moves")]     public Dictionary<string, MoveData> moves;
    [JsonProperty("monsters")]  public List<MonsterData> monsters;
}
