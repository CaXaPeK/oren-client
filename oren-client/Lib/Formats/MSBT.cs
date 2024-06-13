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

    public bool UsesAttributeStrings { get; set; }

    public uint BytesPerAttribute { get; set; }

    private void Parse(Stream fileStream)
    {
        LBL1 lbl1;
        NLI1 nli1;
        ATO1 ato1;
        ATR1 atr1;
        TSY1 tsy1;
        TXT2 txt2;
        
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
                case "NLI1":
                    HasNLI1 = true;
                    nli1 = new(reader);
                    break;
                case "ATO1":
                    HasATO1 = true;
                    ato1 = new(reader, sectionSize);
                    break;
                case "ATR1":
                    HasATR1 = true;
                    atr1 = new(reader, sectionSize, this);
                    break;
                case "TSY1":
                    HasTSY1 = true;
                    tsy1 = new(reader, sectionSize);
                    break;
            }
        }
    }

    internal class LBL1
    {
        public List<String> Labels { get; set; }

        public LBL1(FileReader reader)
        {
            Labels = ReadLabels(reader);
        }
    }
    internal class NLI1
    {
        public Dictionary<uint, uint> LineCounts { get; set; }

        public NLI1(FileReader reader)
        {
            uint entryCount = reader.ReadUInt32();
            LineCounts = new();
            for (uint i = 0; i < entryCount; i++)
            {
                uint entryId = reader.ReadUInt32();
                uint lineIndex = reader.ReadUInt32();
                LineCounts.Add(entryId, lineIndex);
            }
        }
    }
    internal class ATO1
    {
        public List<uint> Numbers { get; set; }

        public ATO1(FileReader reader, long sectionSize)
        {
            for (long i = 0; i < sectionSize / 4; i++)
            {
                Numbers.Add(reader.ReadUInt32());
            }
        }
    }
    internal class ATR1
    {
        public List<Attribute> Attributes { get; set; }

        public ATR1(FileReader reader, long sectionSize, MSBT msbt)
        {
            Attributes = new();
            
            long startPosition = reader.Position;
            uint attributeCount = reader.ReadUInt32();
            uint bytesPerAttribute = reader.ReadUInt32();
            bool hasStrings = sectionSize > (8 + (attributeCount * bytesPerAttribute));
            
            msbt.BytesPerAttribute = bytesPerAttribute;
            msbt.UsesAttributeStrings = hasStrings;
            
            List<byte[]> attributeByteData = new();
            for (uint i = 0; i < attributeCount; i++)
            {
                attributeByteData.Add(reader.ReadBytes((int)bytesPerAttribute));
            }

            foreach (byte[] byteData in attributeByteData)
            {
                if (hasStrings)
                {
                    uint stringOffset = BitConverter.ToUInt32(byteData[0..4]);
                    string stringData = reader.ReadTerminatedStringAt(startPosition + stringOffset);
                
                    Attributes.Add(new(byteData, stringData));
                }
                else
                {
                    Attributes.Add(new(byteData));
                }
            }
        }
    }
    internal class TSY1
    {
        public List<int> StyleIndices { get; set; }

        public TSY1(FileReader reader, long sectionSize)
        {
            StyleIndices = new();
            
            for (uint i = 0; i < sectionSize / 4; i++)
            {
                StyleIndices.Add(reader.ReadInt32());
            }
        }
    }
    internal class TXT2
    {
        
    }
}