using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using HxH.Story;
using HxH.UI;

namespace HxH.Battle;

/// <summary>Apresentação da batalha: sprites, HUD, comandos cronometrados e efeitos. As regras ficam em BattleCore.</summary>
public partial class BattleScene : Node2D
{
    [Signal] public delegate void BattleEndedEventHandler(bool won);
    [Signal] public delegate void MenuPickedEventHandler(int move);
    [Signal] public delegate void AdvancedEventHandler();

    static readonly Color GonColor = new(0.45f, 1f, 0.5f);
    static readonly Color PitouColor = new(0.75f, 0.5f, 1f);

    readonly BattleCore core = new();
    readonly Vector2 gonHome = new(180, 272), pitouHome = new(460, 272);

    Node2D world;
    Sprite2D gonSpr, pitouSpr;
    CpuParticles2D gonAuraFx, pitouAuraFx;
    Bar gonHp, gonAura, pitouHp;
    Label gonName, gonHpText, gonAuraText, gonStatus, pitouHpText, pitouStatus, queueLabel, warnLabel, menuLabel, textLabel, auraCaption;
    ColorRect flash;
    float shake;

    bool menuActive, waitingAdvance;
    int menuIndex;
    List<GonMove> menuMoves = new();

    public override void _Ready()
    {
        world = new Node2D();
        AddChild(world);
        world.AddChild(new Background());

        gonAuraFx = Fx.Aura(GonColor, new Vector2(22, 40));
        pitouAuraFx = Fx.Aura(PitouColor, new Vector2(24, 40));
        world.AddChild(gonAuraFx);
        world.AddChild(pitouAuraFx);
        gonSpr = MakeFighter("gon", gonHome, false, 96);
        pitouSpr = MakeFighter("pitou", pitouHome, true, 96);

        BuildHud();
        flash = UiKit.Box(this, new Rect2(0, 0, 640, 360), new Color(1, 1, 1, 0));
        RefreshHud();
        Run();
    }

    Sprite2D MakeFighter(string id, Vector2 pos, bool flip, float height)
    {
        var s = new Sprite2D { Position = pos, FlipH = flip };
        SetSprite(s, id, height);
        world.AddChild(s);
        return s;
    }

    static void SetSprite(Sprite2D s, string id, float height)
    {
        s.Texture = PixelArt.Get(id);
        float k = PixelArt.ScaleFor(s.Texture, height);
        s.Scale = new Vector2(k, k);
        s.Offset = new Vector2(0, -s.Texture.GetHeight() / 2f);
    }

    static Vector2 Chest(Sprite2D s) => s.Position + new Vector2(0, -s.Texture.GetHeight() * s.Scale.Y * 0.55f);

    // ---------------------------------------------------------------- HUD
    void BuildHud()
    {
        UiKit.Box(this, new Rect2(6, 6, 190, 58), new Color(0, 0, 0, 0.55f));
        gonName = UiKit.Label(this, "GON", new Vector2(12, 8), new Vector2(180, 14), 11, GonColor);
        gonHp = AddBar(new Vector2(12, 24), new Color(0.3f, 0.9f, 0.4f));
        gonHpText = UiKit.Label(this, "", new Vector2(130, 8), new Vector2(60, 14), 9, null, HorizontalAlignment.Right);
        gonAura = AddBar(new Vector2(12, 38), new Color(0.35f, 0.75f, 1f));
        auraCaption = UiKit.Label(this, "AURA", new Vector2(12, 45), new Vector2(60, 12), 8, new Color(0.6f, 0.85f, 1f));
        gonAuraText = UiKit.Label(this, "", new Vector2(130, 45), new Vector2(60, 12), 8, null, HorizontalAlignment.Right);
        gonStatus = UiKit.Label(this, "", new Vector2(8, 66), new Vector2(240, 14), 9, new Color(1f, 0.9f, 0.4f));

        UiKit.Box(this, new Rect2(444, 6, 190, 44), new Color(0, 0, 0, 0.55f));
        UiKit.Label(this, "NEFERPITOU", new Vector2(450, 8), new Vector2(180, 14), 11, PitouColor);
        pitouHp = AddBar(new Vector2(450, 24), new Color(0.9f, 0.3f, 0.35f));
        pitouHpText = UiKit.Label(this, "", new Vector2(568, 8), new Vector2(60, 14), 9, null, HorizontalAlignment.Right);
        pitouStatus = UiKit.Label(this, "", new Vector2(400, 52), new Vector2(234, 14), 9, new Color(1f, 0.7f, 0.9f), HorizontalAlignment.Right);

        queueLabel = UiKit.Label(this, "", new Vector2(200, 8), new Vector2(240, 14), 10, null, HorizontalAlignment.Center);
        warnLabel = UiKit.Label(this, "", new Vector2(120, 86), new Vector2(400, 30), 11, new Color(1f, 0.6f, 0.2f), HorizontalAlignment.Center);

        UiKit.Box(this, new Rect2(6, 290, 628, 64), new Color(0.03f, 0.03f, 0.07f, 0.9f));
        UiKit.Box(this, new Rect2(6, 290, 628, 2), GonColor);
        menuLabel = UiKit.Label(this, "", new Vector2(14, 296), new Vector2(170, 56), 10);
        textLabel = UiKit.Label(this, "", new Vector2(190, 298), new Vector2(436, 54), 11);
    }

    Bar AddBar(Vector2 pos, Color fill)
    {
        var b = new Bar { Position = pos, Size = new Vector2(178, 7), Fill = fill };
        AddChild(b);
        return b;
    }

    void RefreshHud()
    {
        var g = core.Gon; var p = core.Pitou;
        gonName.Text = core.Adult ? "GON (ADULTO)" : "GON";
        gonHp.Value = (float)g.Hp / g.MaxHp;
        gonHpText.Text = $"{g.Hp}/{g.MaxHp}";
        if (core.Adult)
        {
            auraCaption.Text = "PODER";
            gonAura.Fill = new Color(1f, 0.8f, 0.2f);
            gonAura.Value = Mathf.Min(1f, (float)core.Power / BattleCore.PowerForFinal);
            gonAuraText.Text = $"{core.Power}";
        }
        else
        {
            gonAura.Value = (float)g.Aura / g.MaxAura;
            gonAuraText.Text = $"{g.Aura}/{g.MaxAura}";
        }
        gonStatus.Text = core.Charged ? "Aura concentrada: próximo Jajanken x2" : "";
        pitouHp.Value = (float)p.Hp / p.MaxHp;
        pitouHpText.Text = $"{p.Hp}/{p.MaxHp}";
        var st = new List<string>();
        if (core.TerpTurns > 0) st.Add($"Terpsichora ({core.TerpTurns})");
        if (core.BleedTurns > 0) st.Add($"Sangramento ({core.BleedTurns})");
        pitouStatus.Text = string.Join("  ", st);
        var order = core.TurnOrder();
        queueLabel.Text = "Fila: " + string.Join("  >  ", order.ConvertAll(a => a == Actor.Gon ? "GON" : "PITOU"));
        warnLabel.Text = core.NextPitou == PitouMove.SaltoFelino ? "! Próximo golpe: SALTO FELINO — defenda no tempo certo !" : "";
        gonAuraFx.Emitting = core.Charged || core.Adult;
        pitouAuraFx.Emitting = core.TerpTurns > 0;
    }

    void RenderMenu()
    {
        var lines = new List<string>();
        for (int i = 0; i < menuMoves.Count; i++)
        {
            var m = menuMoves[i];
            var info = BattleCore.Moves[m];
            string cost = info.Cost > 0 ? $" ({info.Cost})" : "";
            string off = core.CanUse(m) ? "" : " x";
            lines.Add((i == menuIndex ? "> " : "  ") + info.Label + cost + off);
        }
        menuLabel.Text = string.Join("\n", lines);
        var sel = menuMoves[menuIndex];
        string extra = !core.CanUse(sel)
            ? (sel == GonMove.JajankenFinal ? $"  [Precisa de Poder {BattleCore.PowerForFinal}]" : "  [Aura insuficiente]")
            : "";
        textLabel.Text = BattleCore.Moves[sel].Desc + extra;
    }

    // ---------------------------------------------------------------- input
    public override void _UnhandledInput(InputEvent e)
    {
        if (waitingAdvance && e.IsActionPressed("ui_accept"))
        {
            GetViewport().SetInputAsHandled();
            waitingAdvance = false;
            EmitSignal(SignalName.Advanced);
            return;
        }
        if (!menuActive) return;
        if (e.IsActionPressed("ui_down")) { menuIndex = (menuIndex + 1) % menuMoves.Count; RenderMenu(); }
        else if (e.IsActionPressed("ui_up")) { menuIndex = (menuIndex + menuMoves.Count - 1) % menuMoves.Count; RenderMenu(); }
        else if (e.IsActionPressed("ui_accept"))
        {
            GetViewport().SetInputAsHandled();
            var m = menuMoves[menuIndex];
            if (!core.CanUse(m)) { Fx.FloatText(this, new Vector2(100, 280), "Indisponível", new Color(1, 0.4f, 0.4f), 10); return; }
            menuActive = false;
            menuLabel.Text = "";
            EmitSignal(SignalName.MenuPicked, (int)m);
        }
    }

    public override void _Process(double delta)
    {
        if (shake > 0)
        {
            shake -= (float)delta;
            float a = 6f * Mathf.Clamp(shake / 0.3f, 0, 1);
            world.Position = new Vector2((float)GD.RandRange(-a, a), (float)GD.RandRange(-a, a));
        }
        else world.Position = Vector2.Zero;
        gonAuraFx.Position = Chest(gonSpr);
        pitouAuraFx.Position = Chest(pitouSpr);
    }

    // ---------------------------------------------------------------- fluxo
    async void Run()
    {
        await Say("Neferpitou assume posição de combate. Os olhos brilham.", 1.4f);
        while (true)
        {
            RefreshHud();
            var move = await ChooseMove();
            core.BeginRound(move);
            bool transformed = false;
            foreach (var actor in core.TurnOrder())
            {
                if (actor == Actor.Gon) await GonTurn(move);
                else await PitouTurn();
                RefreshHud();
                if (core.PitouDefeated) { await Victory(); return; }
                if (core.GonDefeated) { await Defeat(); return; }
                if (core.ShouldTransform) { await TransformGon(); transformed = true; break; }
            }
            foreach (var msg in core.EndRound())
            {
                Fx.FloatText(world, Chest(pitouSpr), $"-{BattleCore.BleedDamage}", new Color(1f, 0.3f, 0.3f), 12);
                await Say(msg, 0.7f);
            }
            if (!transformed && core.ShouldTransform) await TransformGon();
        }
    }

    async Task<GonMove> ChooseMove()
    {
        menuMoves = core.AvailableMoves();
        menuIndex = core.CanUse(GonMove.JajankenFinal) ? menuMoves.IndexOf(GonMove.JajankenFinal) : 0;
        menuActive = true;
        RenderMenu();
        var r = await ToSignal(this, SignalName.MenuPicked);
        return (GonMove)r[0].AsInt32();
    }

    public static bool Verbose;

    async Task Say(string text, float seconds)
    {
        textLabel.Text = text;
        if (Verbose) GD.Print("[batalha] " + text);
        await Wait(seconds);
    }

    async Task SayWait(string speaker, string text)
    {
        textLabel.Text = (speaker != "" ? speaker + ": " : "") + text + "   [Enter]";
        waitingAdvance = true;
        await ToSignal(this, SignalName.Advanced);
    }

    async Task Wait(float s) => await ToSignal(GetTree().CreateTimer(s), SceneTreeTimer.SignalName.Timeout);

    async Task<bool> Ring(Vector2 at, float duration, string caption = "")
    {
        var ring = new TimingRing(duration, caption) { Position = at, ZIndex = 30 };
        world.AddChild(ring);
        var r = await ToSignal(ring, TimingRing.SignalName.Finished);
        bool perfect = r[0].AsBool();
        Fx.FloatText(world, at + new Vector2(0, -48), perfect ? "PERFEITO!" : "...", perfect ? new Color(0.4f, 1f, 0.5f) : new Color(1, 1, 1, 0.6f), perfect ? 14 : 10);
        return perfect;
    }

    Tween MoveTo(Sprite2D s, Vector2 target, float t, Tween.TransitionType trans = Tween.TransitionType.Quad)
    {
        var tw = CreateTween();
        tw.TweenProperty(s, "position", target, t).SetTrans(trans).SetEase(Tween.EaseType.Out);
        return tw;
    }

    void Impact(Sprite2D target, int dmg, bool heavy, Color color)
    {
        Vector2 at = Chest(target);
        Fx.FloatText(world, at + new Vector2(0, -20), dmg.ToString(), heavy ? new Color(1f, 0.85f, 0.3f) : Colors.White, heavy ? 22 : 16);
        Fx.Burst(world, at, color, heavy ? 40 : 20, heavy ? 220 : 130);
        shake = heavy ? 0.35f : 0.15f;
        if (heavy) Flash(0.7f, 0.25f);
        target.Modulate = new Color(1f, 0.4f, 0.4f);
        var tw = CreateTween();
        tw.TweenProperty(target, "modulate", Colors.White, 0.3f);
    }

    void Flash(float alpha, float time)
    {
        flash.Color = new Color(1, 1, 1, alpha);
        var tw = CreateTween();
        tw.TweenProperty(flash, "color:a", 0f, time);
    }

    async Task GonTurn(GonMove move)
    {
        var info = BattleCore.Moves[move];
        if (move is GonMove.Carregar or GonMove.Defender)
        {
            var res0 = core.GonAct(move, false);
            Fx.Burst(world, Chest(gonSpr), move == GonMove.Carregar ? GonColor : new Color(0.5f, 0.8f, 1f), 30, 90);
            RefreshHud();
            await Say(res0.Text, 1.0f);
            return;
        }

        await Say(move == GonMove.Golpe ? "Gon avança, a aura queimando ao redor do corpo..." : "Saisho wa guu... Jan... Ken...", 0.4f);
        bool perfect;
        if (move == GonMove.JajankenFinal) perfect = await FinalSequence();
        else if (move == GonMove.Papel)
        {
            // Papel: projétil de aura que viaja até Pitou
            var orb = new CpuParticles2D
            {
                Amount = 40, Lifetime = 0.3, Gravity = Vector2.Zero, Spread = 180f, InitialVelocityMin = 5f, InitialVelocityMax = 20f,
                ScaleAmountMin = 3f, ScaleAmountMax = 5f, Color = new Color(0.7f, 1f, 0.6f), Position = Chest(gonSpr), ZIndex = 20,
            };
            world.AddChild(orb);
            var tw = CreateTween();
            tw.TweenProperty(orb, "position", Chest(pitouSpr), 0.7f);
            perfect = await Ring(Chest(pitouSpr), 0.7f);
            orb.Emitting = false;
            GetTree().CreateTimer(0.5).Timeout += orb.QueueFree;
        }
        else
        {
            float dur = move == GonMove.Golpe ? 0.55f : 0.7f;
            MoveTo(gonSpr, pitouHome + new Vector2(-60, 0), dur * 0.85f);
            perfect = await Ring(Chest(pitouSpr), dur);
        }

        var res = core.GonAct(move, perfect);
        bool heavy = move is GonMove.Pedra or GonMove.JajankenFinal || res.Damage >= 50;
        Impact(pitouSpr, res.Damage, heavy, move == GonMove.Tesoura ? new Color(1f, 0.4f, 0.4f) : GonColor);
        RefreshHud();
        MoveTo(gonSpr, gonHome, 0.35f);
        await Say(res.Text, 1.0f);
    }

    async Task<bool> FinalSequence()
    {
        gonAuraFx.Amount = 64;
        int perfects = 0;
        MoveTo(gonSpr, gonHome + new Vector2(-10, 0), 0.3f);
        if (await Ring(Chest(gonSpr), 0.9f, "JAN...")) perfects++;
        if (await Ring(Chest(gonSpr), 0.75f, "KEN...")) perfects++;
        Flash(0.3f, 0.4f);
        MoveTo(gonSpr, pitouHome + new Vector2(-60, 0), 1.0f, Tween.TransitionType.Expo);
        if (await Ring(Chest(pitouSpr), 1.2f, "PEDRA!")) perfects++;
        Flash(1f, 1.2f);
        shake = 0.8f;
        return perfects >= 2;
    }

    async Task PitouTurn()
    {
        var m = core.NextPitou;
        await Say(core.PitouAnnounce(m), m == PitouMove.SaltoFelino ? 0.3f : 0.5f);
        switch (m)
        {
            case PitouMove.Agachar:
            {
                var tw = CreateTween();
                tw.TweenProperty(pitouSpr, "scale:y", pitouSpr.Scale.Y * 0.8f, 0.3f);
                core.PitouAct(false);
                await Wait(1.0f);
                warnLabel.Text = "! Próximo golpe: SALTO FELINO — defenda no tempo certo !";
                return;
            }
            case PitouMove.Terpsichora:
                core.PitouAct(false);
                Fx.Burst(world, Chest(pitouSpr), PitouColor, 40, 110);
                RefreshHud();
                await Wait(1.0f);
                return;
        }

        bool salto = m == PitouMove.SaltoFelino;
        float dur = salto ? 0.8f : 0.6f;
        SetSprite(pitouSpr, "pitou", 96); // desfaz o agachamento
        var target = gonHome + new Vector2(60, 0);
        if (salto)
        {
            var tw = CreateTween();
            tw.TweenProperty(pitouSpr, "position", (pitouSpr.Position + target) / 2 + new Vector2(0, -90), dur * 0.5f).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
            tw.TweenProperty(pitouSpr, "position", target, dur * 0.45f).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);
        }
        else MoveTo(pitouSpr, target, dur * 0.85f);

        bool perfect = await Ring(Chest(gonSpr), dur);
        var res = core.PitouAct(perfect);
        Impact(gonSpr, res.Damage, salto && !perfect, PitouColor);
        RefreshHud();
        MoveTo(pitouSpr, pitouHome, 0.4f);
        if (res.Text != "") await Say(res.Text, 0.8f);
        else await Wait(0.5f);
    }

    async Task TransformGon()
    {
        warnLabel.Text = "";
        menuLabel.Text = "";
        var lines = StoryData.Get("transform");
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].Portrait == "gon_adulto")
            {
                Flash(1f, 1.5f);
                shake = 0.8f;
                core.Transform();
                SetSprite(gonSpr, "gon_adulto", 124);
                gonAuraFx.EmissionRectExtents = new Vector2(30, 56);
                gonAuraFx.Color = new Color(1f, 0.85f, 0.35f);
                gonAuraFx.Amount = 48;
                Fx.Burst(world, Chest(gonSpr), new Color(1f, 0.9f, 0.4f), 80, 260);
                RefreshHud();
            }
            await SayWait(lines[i].Speaker, lines[i].Text);
        }
        if (!core.Adult) { core.Transform(); SetSprite(gonSpr, "gon_adulto", 124); }
        RefreshHud();
    }

    async Task Victory()
    {
        warnLabel.Text = "";
        var tw = CreateTween();
        tw.TweenProperty(pitouSpr, "modulate:a", 0f, 1.5f);
        tw.Parallel().TweenProperty(pitouSpr, "rotation", -0.4f, 1.5f);
        await Say("Neferpitou cai.", 2.0f);
        EmitSignal(SignalName.BattleEnded, true);
    }

    async Task Defeat()
    {
        var tw = CreateTween();
        tw.TweenProperty(gonSpr, "rotation", -1.4f, 0.6f);
        await Say("Gon não consegue mais se levantar...", 2.0f);
        EmitSignal(SignalName.BattleEnded, false);
    }
}
