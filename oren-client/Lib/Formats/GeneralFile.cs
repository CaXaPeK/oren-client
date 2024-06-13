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
}