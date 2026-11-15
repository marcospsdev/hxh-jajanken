using Godot;

namespace HxH.UI;

public static class Fx
{
    public static void FloatText(Node parent, Vector2 pos, string text, Color c, int size = 16)
    {
        var l = UiKit.Label(parent, text, pos - new Vector2(70, 10), new Vector2(140, 24), size, c, HorizontalAlignment.Center);
        l.ZIndex = 50;
        var tw = l.CreateTween();
        tw.SetParallel();
        tw.TweenProperty(l, "position:y", l.Position.Y - 32, 0.9f);
        tw.TweenProperty(l, "modulate:a", 0f, 0.5f).SetDelay(0.5f);
        tw.Chain().TweenCallback(Callable.From(l.QueueFree));
    }

    public static void Burst(Node parent, Vector2 pos, Color c, int amount = 24, float speed = 140)
    {
        var p = new CpuParticles2D
        {
            Position = pos, Amount = amount, OneShot = true, Explosiveness = 1f, Lifetime = 0.55,
            Spread = 180f, InitialVelocityMin = speed * 0.4f, InitialVelocityMax = speed, Gravity = Vector2.Zero,
            ScaleAmountMin = 2f, ScaleAmountMax = 4f, Color = c, ZIndex = 40,
        };
        parent.AddChild(p);
        p.Emitting = true;
        p.Finished += p.QueueFree;
    }

    public static CpuParticles2D Aura(Color c, Vector2 extents)
    {
        return new CpuParticles2D
        {
            Amount = 32, Lifetime = 0.8, EmissionShape = CpuParticles2D.EmissionShapeEnum.Rectangle,
            EmissionRectExtents = extents, Direction = Vector2.Up, Spread = 12f,
            InitialVelocityMin = 20f, InitialVelocityMax = 55f, Gravity = Vector2.Zero,
            ScaleAmountMin = 1.5f, ScaleAmountMax = 3f, Color = c, Emitting = false, ZIndex = -1,
        };
    }
}
