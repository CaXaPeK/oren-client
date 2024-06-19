namespace oren_client.Lib.Formats;

public class Attribute
{
    public byte[] ByteData;
    public string StringData;
    
    public Attribute(byte[] byteData)
    {
        ByteData = byteData;
    }
    public Attribute(byte[] byteData, string stringData)
    {
        ByteData = byteData;
        StringData = stringData;
    }
}