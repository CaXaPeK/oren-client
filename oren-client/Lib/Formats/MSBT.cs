namespace oren_client.Lib.Formats;

public class MSBT
{
    public bool HasLBL1 { get; set; }
    public bool HasNLI1 { get; set; }
    public bool HasATO1 { get; set; }
    public bool HasATR1 { get; set; }
    public bool HasTSY1 { get; set; }

    private void Parse(Stream fileStream)
    {
        BinaryReader reader = new(fileStream);

        LBL1 lbl1 = new();
        NLI1 nli1 = new();
        ATO1 ato1 = new();
        ATR1 atr1 = new();
        TSY1 tsy1 = new();
        TXT2 txt2 = new();

            
    }

    internal class LBL1
    {

    }
    internal class NLI1
    {

    }
    internal class ATO1
    {

    }
    internal class ATR1
    {

    }
    internal class TSY1
    {

    }
    internal class TXT2
    {

    }
}