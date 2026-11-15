using Godot;

namespace HxH.UI;

public static class UiKit
{
    public static Label Label(Node parent, string text, Vector2 pos, Vector2 size, int fontSize = 10,
        Color? color = null, HorizontalAlignment align = HorizontalAlignment.Left, bool outline = true)
    {
        var l = new Label
        {
            Text = text, Position = pos, Size = size, HorizontalAlignment = align,
            AutowrapMode = TextServer.AutowrapMode.WordSmart, MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        l.AddThemeFontSizeOverride("font_size", fontSize);
        l.AddThemeColorOverride("font_color", color ?? Colors.White);
        if (outline)
        {
            l.AddThemeColorOverride("font_outline_color", new Color(0, 0, 0, 0.9f));
            l.AddThemeConstantOverride("outline_size", 3);
        }
        parent.AddChild(l);
        return l;
    }

    public static ColorRect Box(Node parent, Rect2 rect, Color color)
    {
        var b = new ColorRect { Position = rect.Position, Size = rect.Size, Color = color, MouseFilter = Control.MouseFilterEnum.Ignore };
        parent.AddChild(b);
        return b;
    }
}

