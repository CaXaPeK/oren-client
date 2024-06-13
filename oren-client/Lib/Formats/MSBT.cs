using System.Text;
using oren_client.Lib.Utils;

namespace oren_client.Lib.Formats;

public class MSBT : GeneralFile
{
    public bool HasLBL1 { get; set; }
    public bool HasNLI1 { get; set; }
    public bool HasATO1 { get; set; }
    public bool HasATR1 { get; set; }
    public bool HasTSY1 { get; set; }
    
    public Header Header { get; set; }

    private void Parse(Stream fileStream)
    {
        LBL1 lbl1;
        NLI1 nli1 = new();
        ATO1 ato1 = new();
        ATR1 atr1 = new();
        TSY1 tsy1 = new();
        TXT2 txt2 = new();
        
        FileReader reader = new(fileStream);
        Header = new(reader);

        for (int i = 0; i < Header.SectionCount; i++)
        {
            string sectionMagic = reader.ReadString(4, Encoding.ASCII);
            uint sectionSize = reader.ReadUInt32();
            reader.Skip(8);
            
            switch (sectionMagic)
            {
                case "LBL1":
                    HasLBL1 = true;
                    lbl1 = new(reader);
                    break;
            }
        }
    }

    internal class LBL1
    {
        public List<String> Labels { get; set; }

        public LBL1(FileReader reader)
        {
            Labels = new(new string[CalculateLabelCount(reader)]);

            long startPosition = reader.Position;
            uint hashTableSlotCount = reader.ReadUInt32();
            for (uint i = 0; i < hashTableSlotCount; i++)
            {
                uint labelCount = reader.ReadUInt32();
                uint labelOffset = reader.ReadUInt32();
                long nextSlotPosition = reader.Position;

                reader.JumpTo(startPosition + labelOffset);
                for (uint j = 0; j < labelCount; j++)
                {
                    byte length = reader.ReadByte();
                    string labelString = reader.ReadString(length);
                    Labels[reader.ReadInt32()] = labelString;
                }

                reader.JumpTo(nextSlotPosition);
            }
        }
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