using Godot;

namespace HxH.UI;

/// <summary>Cenário: floresta à noite perto do palácio de East Gorteau (desenhado em código, provisório).</summary>
public partial class Background : Node2D
{
    public override void _Draw()
    {
        for (int y = 0; y < 360; y += 4)
        {
            float k = y / 360f;
            DrawRect(new Rect2(0, y, 640, 4), new Color(0.05f + 0.12f * k, 0.04f + 0.07f * k, 0.13f + 0.08f * k));
        }
        DrawCircle(new Vector2(530, 70), 22, new Color(0.95f, 0.92f, 0.8f));
        DrawCircle(new Vector2(522, 64), 20, new Color(0.08f, 0.06f, 0.16f, 0.25f));
        // silhueta do palácio
        DrawRect(new Rect2(250, 200, 140, 70), new Color(0.07f, 0.07f, 0.12f));
        DrawRect(new Rect2(300, 165, 40, 35), new Color(0.07f, 0.07f, 0.12f));
        DrawColoredPolygon(new[] { new Vector2(295, 165), new Vector2(320, 140), new Vector2(345, 165) }, new Color(0.07f, 0.07f, 0.12f));
        var rng = new RandomNumberGenerator { Seed = 7 };
        for (int i = 0; i < 26; i++)
        {
            float x = i * 26 + rng.RandfRange(-6, 6);
            float h = rng.RandfRange(50, 120);
            if (x > 240 && x < 400) h *= 0.5f;
            DrawColoredPolygon(new[] { new Vector2(x - 16, 280), new Vector2(x, 280 - h), new Vector2(x + 16, 280) },
                new Color(0.04f, 0.09f, 0.08f));
        }
        DrawRect(new Rect2(0, 270, 640, 90), new Color(0.11f, 0.14f, 0.09f));
        DrawRect(new Rect2(0, 270, 640, 2), new Color(0.22f, 0.27f, 0.14f));
        for (int i = 0; i < 60; i++)
            DrawRect(new Rect2(rng.RandfRange(0, 640), rng.RandfRange(275, 360), 2, 1), new Color(0.18f, 0.22f, 0.12f));
    }
}
