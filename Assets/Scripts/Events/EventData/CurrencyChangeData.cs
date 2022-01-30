public class CurrencyChangeData
{
    public int balance { get; private set; }
    public int change { get; private set; }

    public CurrencyChangeData(int balance, int change)
    {
        this.balance = balance;
        this.change = change;
    }
}
