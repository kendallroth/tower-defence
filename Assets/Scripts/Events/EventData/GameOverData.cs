public enum GameResult
{
    WON,
    LOST
}

public class GameOverData
{
    public int roundsSurvived { get; private set; }
    public GameResult result { get; private set; }

    public GameOverData(GameResult result, int roundsSurvived)
    {
        this.result = result;
        this.roundsSurvived = roundsSurvived;
    }
}

