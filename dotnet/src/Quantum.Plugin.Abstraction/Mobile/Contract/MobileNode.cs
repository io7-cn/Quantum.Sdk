namespace Quantum.Plugin.Abstraction;

public sealed record MobileNode
{
    public required string Type { get; set; }
    public required string Id { get; set; }
    public string? Text { get; set; }
    public string? Placeholder { get; set; }
    public string Style { get; set; } = "body";
    public int MaxLength { get; set; } = 500;
    public bool Enabled { get; set; } = true;
    public MobileRpcAction? Action { get; set; }
    public MobileNode[] Children { get; set; } = [];
    public MobileNode Label(string text)
    {
        return this with { Text = text };
    }

    public MobileNode Hint(string text)
    {
        return this with { Placeholder = text };
    }

    public MobileNode Limit(int length)
    {
        return this with { MaxLength = length };
    }

    public MobileNode Appearance(string style)
    {
        return this with { Style = style };
    }

    public MobileNode OnClick(MobileRpcAction action)
    {
        return this with { Action = action };
    }
}
