namespace oren_client.Models;

public class MsbtSheet
{
    public string FileName { get; set; }
    
    public int Version { get; set; }
    
    public bool IsBigEndian { get; set; }
    
    public bool UseIndices { get; set; }
    
    public bool UseStyles { get; set; }
    
    public bool UseAttributes { get; set; }
    
    public int BytesPerAttribute { get; set; }
    
    public string Encoding { get; set; }
    
    public List<string> Labels { get; set; }
    
    public List<List<string>> Columns { get; set; }
    
    public List<string> Attributes { get; set; }
}