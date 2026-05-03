public class ActiveModifier
{
    public string stat;
    public int amount;
    public int turnsRemaining;

    public ActiveModifier(string stat, int amount, int duration)
    {
        this.stat = stat;
        this.amount = amount;
        turnsRemaining = duration;
    }
}
