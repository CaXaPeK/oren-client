namespace oren_client.Models;

public class MessageModel
{
    public string Text { get; set; }
    
    public string? Style { get; set; }
    
    public Dictionary<string, string>? Attributes { get; set; }
}