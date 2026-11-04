namespace Quantum.Plugin.Abstraction;

/// <summary>Declares pages in desktop plugins; the mobile client renders host-generated views.</summary>
public static class Mobile
{
    public static MobilePage Page(string id, string title, MobileNode root)
    {
        return new() { Id = id, Title = title, Root = root, SchemaVersion = HasAttachments(root) ? 2 : 1 };
    }

    public static MobileNode VStack(string id, params MobileNode[] children)
    {
        return new() { Type = "VStack", Id = id, Children = children };
    }

    public static MobileNode HStack(string id, params MobileNode[] children)
    {
        return new() { Type = "HStack", Id = id, Children = children };
    }

    public static MobileNode Scroll(string id, MobileNode child)
    {
        return new() { Type = "Scroll", Id = id, Children = [child] };
    }

    public static MobileNode Text(string id, string text)
    {
        return new() { Type = "Text", Id = id, Text = text };
    }

    public static MobileNode TextField(string id)
    {
        return new() { Type = "TextField", Id = id };
    }

    private static bool HasAttachments(MobileNode node)
    {
        return node.Type == "Attachments" || node.Children.Any(HasAttachments);
    }

    public static MobileNode Attachments(string id)
    {
        return new() { Type = "Attachments", Id = id };
    }

    public static MobileNode Toggle(string id, string text)
    {
        return new() { Type = "Toggle", Id = id, Text = text };
    }

    public static MobileNode Button(string id, string text)
    {
        return new() { Type = "Button", Id = id, Text = text };
    }

    public static MobileNode Divider(string id)
    {
        return new() { Type = "Divider", Id = id };
    }

    public static MobileNode Spacer(string id)
    {
        return new() { Type = "Spacer", Id = id };
    }

    public static MobileNode Progress(string id)
    {
        return new() { Type = "Progress", Id = id };
    }

    public static MobileRpcAction Rpc(string qualifiedName)
    {
        return new() { RpcName = qualifiedName };
    }
}
