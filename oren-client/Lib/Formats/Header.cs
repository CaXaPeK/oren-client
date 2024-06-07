using System.Text;
using oren_client.Lib.Utils;

namespace oren_client.Lib.Formats;

public class Header
{
    public FileType FileType { get; set; }
    public Endianness Endianness { get; set; }
    public EncodingType EncodingType { get; set; }
    public byte Version { get; set; }
    public short SectionCount { get; set; }
    public int FileSize { get; set; }

    public Header(FileReader reader)
    {
        string magic = reader.ReadString(8, Encoding.ASCII);
        switch (magic)
        {
            case "MsgStdBn":
                FileType = FileType.MSBT;
                break;
            case "MsgPrjBn":
                FileType = FileType.MSBP;
                break;
        }

        Endianness = (Endianness)reader.ReadInt16();
        reader.Endianness = Endianness;
        
        reader.Skip(2);

        EncodingType = (EncodingType)reader.ReadByte();
        Version = reader.ReadByte();
        SectionCount = reader.ReadInt16();
        
        reader.Skip(2);

        //FileSize = reader.ReadInt32();
    }
}