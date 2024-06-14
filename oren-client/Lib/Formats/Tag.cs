namespace oren_client.Lib.Formats;

public class Tag
{
    public ushort Group { get; set; }
    public ushort Type { get; set; }
    public byte[] RawArguments { get; set; }
    //public List<Argument> Arguments { get; set; }

    public Tag(ushort group, ushort type, byte[] rawArguments)
    {
        Group = group;
        Type = type;
        RawArguments = rawArguments;
    }

    public string Stringify(MSBP? msbp = null)
    {
        return $"<{Group}.{Type}:{BitConverter.ToString(RawArguments)}>";
    }
    //public Tag(ushort group, ushort type, byte[] rawArguments, MSBP msbp)
}

public class TagEnd
{
    public ushort Group { get; set; }
    public ushort Type { get; set; }
    
    public TagEnd(ushort group, ushort type)
    {
        Group = group;
        Type = type;
    }
    
    public string Stringify(MSBP? msbp = null)
    {
        return $"</{Group}.{Type}>";
    }
}