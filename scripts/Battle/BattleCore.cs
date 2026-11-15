using System;
using System.Collections.Generic;

namespace HxH.Battle;

public enum GonMove { Pedra, Tesoura, Papel, Carregar, Defender, Golpe, JajankenFinal }
public enum PitouMove { Garra, Agachar, SaltoFelino, Terpsichora }
public enum Actor { Gon, Pitou }

public class Fighter
{
    public string Name;
    public int MaxHp, Hp, MaxAura, Aura, Atk, Def, Spd;
    public bool Alive => Hp > 0;
}

public record MoveInfo(string Label, int Cost, float Mult, bool IgnoreDef, string Desc);

public class ActionResult
{
    public int Damage;
    public string Text;
    public bool Perfect;
}

/// <summary>Regras do duelo Gon vs. Neferpitou, sem dependência de nós da Godot (testável).</summary>
public class BattleCore
{
    public const int AuraRegen = 4;
    public const int BleedDamage = 6;
    public const int BleedDuration = 3;
    public const int TerpDuration = 3;
    public const float TerpMult = 1.5f;
    public const int PowerForFinal = 4;
    public const float PerfectMult = 1.5f;
    public const float PerfectBlockFactor = 0.25f;

    public static readonly Dictionary<GonMove, MoveInfo> Moves = new()
    {
        [GonMove.Pedra] = new("Pedra", 15, 2.2f, false, "Soco de aura concentrada. Dano alto."),
        [GonMove.Tesoura] = new("Tesoura", 8, 1.4f, false, "Lâmina de aura. Dano médio e causa Sangramento."),
        [GonMove.Papel] = new("Papel", 10, 1.2f, true, "Projétil de aura. Ignora a defesa."),
        [GonMove.Carregar] = new("Carregar", 0, 0f, false, "Concentra aura (+15). O próximo Jajanken causa dano dobrado."),
        [GonMove.Defender] = new("Defender", 0, 0f, false, "Ken: reduz o dano pela metade e recupera aura."),
        [GonMove.Golpe] = new("Golpe", 0, 0f, false, "Cada golpe é mais forte que o anterior. Aumenta o Poder."),
        [GonMove.JajankenFinal] = new("Jajanken: PEDRA", 0, 50f, false, "Golpe final. Comando cronometrado longo."),
    };

    static readonly PitouMove[] KidPattern =
    {
        PitouMove.Garra, PitouMove.Agachar, PitouMove.SaltoFelino, PitouMove.Terpsichora,
        PitouMove.Garra, PitouMove.Garra, PitouMove.Agachar, PitouMove.SaltoFelino,
    };
    static readonly PitouMove[] AdultPattern =
    {
        PitouMove.Garra, PitouMove.Terpsichora, PitouMove.Agachar, PitouMove.SaltoFelino,
    };

    public Fighter Gon, Pitou;
    public bool Adult, Charged, Defending;
    public int Power, BleedTurns, TerpTurns, Round;
    public PitouMove NextPitou;
    int patternIdx;

    public BattleCore()
    {
        Gon = new Fighter { Name = "Gon", MaxHp = 140, Hp = 140, MaxAura = 60, Aura = 60, Atk = 14, Def = 6, Spd = 10 };
        Pitou = new Fighter { Name = "Neferpitou", MaxHp = 320, Hp = 320, MaxAura = 999, Aura = 999, Atk = 20, Def = 10, Spd = 14 };
        NextPitou = PlanNext();
    }

    public static int Damage(int atk, int def, float mult, bool ignoreDef, bool perfect)
    {
        float raw = atk * mult * (perfect ? PerfectMult : 1f);
        if (!ignoreDef) raw -= def * 0.5f;
        return Math.Max(1, (int)MathF.Round(raw));
    }

    public static bool IsJajanken(GonMove m) => m is GonMove.Pedra or GonMove.Tesoura or GonMove.Papel;

    public List<GonMove> AvailableMoves() => Adult
        ? new List<GonMove> { GonMove.Golpe, GonMove.JajankenFinal }
        : new List<GonMove> { GonMove.Pedra, GonMove.Tesoura, GonMove.Papel, GonMove.Carregar, GonMove.Defender };

    public bool CanUse(GonMove m)
    {
        if (m == GonMove.JajankenFinal) return Adult && Power >= PowerForFinal;
        return Gon.Aura >= Moves[m].Cost;
    }

    /// <summary>Ordem do turno por Velocidade (empate favorece o jogador).</summary>
    public List<Actor> TurnOrder() => Gon.Spd >= Pitou.Spd
        ? new List<Actor> { Actor.Gon, Actor.Pitou }
        : new List<Actor> { Actor.Pitou, Actor.Gon };

    public void BeginRound(GonMove chosen) => Defending = chosen == GonMove.Defender;

    public ActionResult GonAct(GonMove m, bool perfect)
    {
        var info = Moves[m];
        var r = new ActionResult { Perfect = perfect };
        Gon.Aura -= info.Cost;
        switch (m)
        {
            case GonMove.Carregar:
                Charged = true;
                Gon.Aura = Math.Min(Gon.MaxAura, Gon.Aura + 15);
                r.Text = "Gon concentra a aura... o próximo Jajanken causará dano dobrado!";
                return r;
            case GonMove.Defender:
                Gon.Aura = Math.Min(Gon.MaxAura, Gon.Aura + 6);
                r.Text = "Gon se protege com Ken.";
                return r;
        }

        float mult = info.Mult;
        bool wasCharged = false;
        if (IsJajanken(m) && Charged) { mult *= 2f; Charged = false; wasCharged = true; }
        if (m == GonMove.Golpe) { mult = 1f + 0.5f * Power; Power++; }

        r.Damage = Damage(Gon.Atk, Pitou.Def, mult, info.IgnoreDef, perfect);
        ApplyToPitou(r.Damage, m == GonMove.JajankenFinal);

        r.Text = m switch
        {
            GonMove.Pedra => "PEDRA!",
            GonMove.Tesoura => "TESOURA! Pitou começa a sangrar.",
            GonMove.Papel => "PAPEL! A aura atravessa a defesa.",
            GonMove.Golpe => Pitou.Hp == 1 ? $"Golpe! Poder {Power}. Terpsichora mantém Pitou de pé!" : $"Golpe! Poder {Power}.",
            GonMove.JajankenFinal => "PEDRA!!!",
            _ => "",
        };
        if (wasCharged) r.Text += " (carregado x2)";
        if (m == GonMove.Tesoura) BleedTurns = BleedDuration;
        return r;
    }

    void ApplyToPitou(int dmg, bool final)
    {
        Pitou.Hp -= dmg;
        // Antes da transformação, ou sem o golpe final, Pitou nunca cai: Terpsichora a sustenta.
        if (!final && Pitou.Hp < 1) Pitou.Hp = 1;
        if (Pitou.Hp < 0) Pitou.Hp = 0;
    }

    public bool PitouAttacks(PitouMove m) => m is PitouMove.Garra or PitouMove.SaltoFelino;

    public string PitouAnnounce(PitouMove m) => m switch
    {
        PitouMove.Garra => "Pitou ataca com as garras!",
        PitouMove.Agachar => "Pitou se agacha, as pernas tensas como molas... PREPARE A DEFESA!",
        PitouMove.SaltoFelino => "SALTO FELINO!",
        PitouMove.Terpsichora => "Terpsichora! Pitou controla o próprio corpo como um boneco. Ataque aumentado!",
        _ => "",
    };

    public ActionResult PitouAct(bool perfectBlock)
    {
        var m = NextPitou;
        var r = new ActionResult { Perfect = perfectBlock, Text = PitouAnnounce(m) };
        if (m == PitouMove.Terpsichora) { TerpTurns = TerpDuration; return r; }
        if (!PitouAttacks(m)) return r;

        float mult = m == PitouMove.SaltoFelino ? 2.4f : 1f;
        if (TerpTurns > 0) mult *= TerpMult;
        int dmg = Damage(Pitou.Atk, Gon.Def, mult, false, false);
        if (Defending) dmg /= 2;
        if (perfectBlock) dmg = (int)MathF.Round(dmg * PerfectBlockFactor);
        r.Damage = dmg;
        Gon.Hp = Math.Max(0, Gon.Hp - dmg);
        r.Text = perfectBlock ? "Defesa perfeita!" : (Defending ? "Gon segura o golpe com Ken." : "");
        return r;
    }

    public List<string> EndRound()
    {
        var msgs = new List<string>();
        if (BleedTurns > 0)
        {
            Pitou.Hp = Math.Max(1, Pitou.Hp - BleedDamage);
            BleedTurns--;
            msgs.Add($"Pitou sangra: -{BleedDamage}");
        }
        if (!Adult) Gon.Aura = Math.Min(Gon.MaxAura, Gon.Aura + AuraRegen);
        if (TerpTurns > 0) TerpTurns--;
        Defending = false;
        Round++;
        NextPitou = PlanNext();
        return msgs;
    }

    PitouMove PlanNext()
    {
        var pattern = Adult ? AdultPattern : KidPattern;
        var m = pattern[patternIdx % pattern.Length];
        patternIdx++;
        if (m == PitouMove.Terpsichora && TerpTurns > 0) m = PitouMove.Garra;
        return m;
    }

    public bool ShouldTransform => !Adult && Pitou.Hp <= Pitou.MaxHp / 2;
    public bool GonDefeated => Gon.Hp <= 0;
    public bool PitouDefeated => Pitou.Hp <= 0;

    public void Transform()
    {
        Adult = true;
        Charged = false;
        Power = 1;
        BleedTurns = 0;
        Gon = new Fighter { Name = "Gon (adulto)", MaxHp = 999, Hp = 999, MaxAura = 999, Aura = 999, Atk = 60, Def = 40, Spd = 30 };
        patternIdx = 0;
        NextPitou = PlanNext();
    }
}
