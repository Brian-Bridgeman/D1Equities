namespace D1Equities.Sim;

public class NewCandleEventArgs : EventArgs
{
    public CandleStick Candle { get; }

    public NewCandleEventArgs(CandleStick candle)
    {
        Candle = candle;
    }
}