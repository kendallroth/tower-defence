public class LivesChangeData
{
    public int change;
    public int remaining;
    public int starting;
    public float percent => starting > 0 ? remaining / starting : 0;

    public LivesChangeData(int remaining, int change, int starting)
    {
        this.remaining = remaining;
        this.change = change;
        this.starting = starting;
    }
}

