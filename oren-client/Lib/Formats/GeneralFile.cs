using oren_client.Lib.Utils;

namespace oren_client.Lib.Formats;

public class GeneralFile
{
    public static uint CalculateLabelCount(FileReader reader)
    {
        long startPosition = reader.Position;
        uint hashTableSlotCount = reader.ReadUInt32();
        uint labelCount = 0;
        for (int i = 0; i < hashTableSlotCount; i++)
        {
            labelCount += reader.ReadUInt32();
            reader.Skip(4);
        }

        reader.Position = startPosition;
        return labelCount;
    }

    public static List<string> ReadLabels(FileReader reader)
    {
        List<string> labels = new(new string[CalculateLabelCount(reader)]);

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
                labels[reader.ReadInt32()] = labelString;
            }

            reader.JumpTo(nextSlotPosition);
        }

        return labels;
    }
}