namespace D1Equities.Sim;

public class CandleUpdatedEventArgs : EventArgs
{
    public CandleStick Candle { get; }

    public CandleUpdatedEventArgs(CandleStick candle)
    {
        Candle = candle;
    }
}
