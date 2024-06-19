using System.Data;
using System.Text;
using Microsoft.VisualBasic;
using oren_client.Lib.Utils;

namespace oren_client.Lib.Formats;

public class MSBT : GeneralFile
{
    public MSBP Msbp;

    public Dictionary<object, Message> Messages = new();
    public string FileName { get; set; }
    public string Language { get; set; }

    public bool HasLBL1 = false;
    public bool HasNLI1 = false;
    public bool HasATO1 = false;
    public bool HasATR1 = false;
    public bool HasTSY1 = false;

    public bool UsesAttributeStrings { get; set; }

    public uint BytesPerAttribute { get; set; }
    
    public List<uint> ATO1Numbers { get; set; }
    
    public Header Header { get; set; }
    
    public MSBT(byte[] data, string? fileName = null, string? language = null)
    {
        FileName = fileName;
        Language = language;
        
        FileReader reader = new(new MemoryStream(data));
        
        LBL1 lbl1 = new();
        NLI1 nli1 = new();
        ATO1 ato1 = new();
        ATR1 atr1 = new();
        TSY1 tsy1 = new();
        TXT2 txt2 = new();
        
        Header = new(reader);

        for (int i = 0; i < Header.SectionCount; i++)
        {
            string sectionMagic = reader.ReadString(4, Encoding.ASCII);
            uint sectionSize = reader.ReadUInt32();
            reader.Skip(8);
            long startPosition = reader.Position;
            
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
                    HasATR1 = false;
                    atr1 = new(reader, sectionSize, this);
                    break;
                case "TSY1":
                    HasTSY1 = true;
                    tsy1 = new(reader, sectionSize);
                    break;
                case "TXT2":
                    txt2 = new(reader, HasATR1, atr1, HasTSY1, tsy1);
                    break;
                default:
                    throw new DataException("Unknown section magic!");
            }
            
            reader.JumpTo(startPosition);
            reader.Skip((int)sectionSize);
            reader.Align(0x10);
        }

        if (HasLBL1)
        {
            for (int i = 0; i < lbl1.Labels.Count; i++)
            {
                Messages.Add(lbl1.Labels[i], txt2.Messages[i]);
            }
        }
        else if (HasNLI1)
        {
            foreach (var pair in nli1.Indices)
            {
                Messages.Add(pair.Key.ToString(), txt2.Messages[(int)pair.Value]);
            }
        }

        if (HasATO1)
        {
            ATO1Numbers = ato1.Numbers;
        }
    }

    public void PrintAllMessages()
    {
        Console.WriteLine($"{Language} {FileName}.msbt");
        foreach (var Message in Messages)
        {
            Console.WriteLine($"[{Message.Key}] {Message.Value.Text}");
        }
    }

    #region msbt sections
    
    internal class LBL1
    {
        public List<String> Labels { get; set; }

        public LBL1() {}

        public LBL1(FileReader reader)
        {
            Labels = ReadLabels(reader);
        }
    }
    internal class NLI1
    {
        public Dictionary<uint, uint> Indices { get; set; }

        public NLI1() {}
        
        public NLI1(FileReader reader)
        {
            uint entryCount = reader.ReadUInt32();
            Indices = new();
            for (uint i = 0; i < entryCount; i++)
            {
                uint entryId = reader.ReadUInt32();
                uint messageIndex = reader.ReadUInt32();
                Indices.Add(entryId, messageIndex);
            }
        }
    }
    internal class ATO1
    {
        public List<uint> Numbers { get; set; }

        public ATO1() {}

        public ATO1(FileReader reader, long sectionSize)
        {
            Numbers = new();
            
            for (long i = 0; i < sectionSize / 4; i++)
            {
                Numbers.Add(reader.ReadUInt32());
            }
        }
    }
    internal class ATR1
    {
        public List<Attribute> Attributes { get; set; }

        public ATR1() {}

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

        public TSY1() {}

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
        public List<Message> Messages { get; set; }

        public TXT2() {}

        public TXT2(FileReader reader, bool hasATR1, ATR1 atr1, bool hasTSY1, TSY1 tsy1)
        {
            Messages = new();
            
            long startPosition = reader.Position;
            uint messageCount = reader.ReadUInt32();

            for (uint i = 0; i < messageCount; i++)
            {
                Message message = new();
                if (hasATR1)
                {
                    if (i < atr1.Attributes.Count)
                    {
                        message.Attribute = atr1.Attributes[(int)i];
                    }
                    else
                    {
                        throw new KeyNotFoundException("Numbers of ATR1 and TXT2 entries don't match!");
                    }
                }
                if (hasTSY1)
                {
                    if (i < tsy1.StyleIndices.Count)
                    {
                        message.StyleId = tsy1.StyleIndices[(int)i];
                    }
                    else
                    {
                        throw new KeyNotFoundException("Numbers of TSY1 and TXT2 entries don't match!");
                    }
                }

                uint stringOffset = reader.ReadUInt32();
                long nextOffsetPosition = reader.Position;
                
                reader.JumpTo(startPosition + stringOffset);

                bool reachedEnd = false;
                StringBuilder messageString = new();

                while (!reachedEnd)
                {
                    short character = reader.ReadInt16();
                    switch (character)
                    {
                        case 0x0E:
                            ushort tagGroup = reader.ReadUInt16();
                            ushort tagType = reader.ReadUInt16();
                            ushort argumentsLength = reader.ReadUInt16();
                            byte[] rawTagArguments = reader.ReadBytes(argumentsLength);

                            Tag tag = new(tagGroup, tagType, rawTagArguments);

                            messageString.Append(tag.Stringify());
                            break;
                        
                        case 0x0F:
                            ushort tagEndGroup = reader.ReadUInt16();
                            ushort tagEndType = reader.ReadUInt16();

                            TagEnd tagEnd = new(tagEndGroup, tagEndType);

                            messageString.Append(tagEnd.Stringify());
                            break;
                        
                        case 0x00:
                            reachedEnd = true;
                            break;
                        
                        default:
                            messageString.Append(Encoding.Unicode.GetString(BitConverter.GetBytes(character)));
                            break;
                    }
                }

                message.Text = messageString.ToString();
                Messages.Add(message);
                reader.JumpTo(nextOffsetPosition);
            }
        }
    }
    
    #endregion
}