using System;
using System.Collections.Generic;
using Godot;
using HxH.Battle;
using HxH.Story;

namespace HxH.Tests;

/// <summary>Testes das regras de combate. Rodar: godot --headless --path . res://tests/TestRunner.tscn</summary>
public partial class TestRunner : Node
{
    int passed, failed;

    public override void _Ready()
    {
        var tests = new List<(string, Action)>
        {
            ("Fórmula de dano com defesa", () => Eq(26, BattleCore.Damage(14, 10, 2.2f, false, false))),
            ("Golpe perfeito multiplica por 1.5", () => Eq(41, BattleCore.Damage(14, 10, 2.2f, false, true))),
            ("Papel ignora defesa", () => Eq(17, BattleCore.Damage(14, 10, 1.2f, true, false))),
            ("Dano mínimo é 1", () => Eq(1, BattleCore.Damage(1, 99, 1f, false, false))),
            ("Ordem de turno: Pitou (Vel 14) age antes de Gon (Vel 10)", () =>
            {
                var c = new BattleCore();
                Eq(Actor.Pitou, c.TurnOrder()[0]);
            }),
            ("Ordem de turno: Gon adulto (Vel 30) age primeiro", () =>
            {
                var c = new BattleCore(); c.Transform();
                Eq(Actor.Gon, c.TurnOrder()[0]);
            }),
            ("Pedra gasta aura; sem aura fica indisponível", () =>
            {
                var c = new BattleCore();
                c.GonAct(GonMove.Pedra, false);
                Eq(45, c.Gon.Aura);
                c.Gon.Aura = 14;
                True(!c.CanUse(GonMove.Pedra));
                True(c.CanUse(GonMove.Tesoura));
            }),
            ("Carregar dobra o próximo Jajanken e é consumido", () =>
            {
                var c = new BattleCore();
                c.GonAct(GonMove.Carregar, false);
                True(c.Charged);
                var r = c.GonAct(GonMove.Pedra, false);
                Eq(BattleCore.Damage(14, 10, 4.4f, false, false), r.Damage);
                True(!c.Charged);
            }),
            ("Tesoura causa Sangramento no fim do turno", () =>
            {
                var c = new BattleCore();
                c.GonAct(GonMove.Tesoura, false);
                int hp = c.Pitou.Hp;
                c.EndRound();
                Eq(hp - BattleCore.BleedDamage, c.Pitou.Hp);
                Eq(BattleCore.BleedDuration - 1, c.BleedTurns);
            }),
            ("Telegrafia: Agachar é sempre seguido de Salto Felino", () =>
            {
                var c = new BattleCore();
                for (int i = 0; i < 30; i++)
                {
                    var now = c.NextPitou;
                    c.PitouAct(false); c.Gon.Hp = 999; c.EndRound();
                    if (now == PitouMove.Agachar) Eq(PitouMove.SaltoFelino, c.NextPitou);
                }
            }),
            ("Defesa perfeita reduz o dano a 25%", () =>
            {
                var a = new BattleCore(); var b = new BattleCore();
                int normal = a.PitouAct(false).Damage;
                int blocked = b.PitouAct(true).Damage;
                Eq((int)MathF.Round(normal * BattleCore.PerfectBlockFactor), blocked);
            }),
            ("Defender reduz o dano pela metade", () =>
            {
                var a = new BattleCore(); var b = new BattleCore();
                b.BeginRound(GonMove.Defender);
                Eq(a.PitouAct(false).Damage / 2, b.PitouAct(false).Damage);
            }),
            ("Terpsichora aumenta o ataque de Pitou", () =>
            {
                var a = new BattleCore(); var b = new BattleCore();
                b.TerpTurns = 2;
                True(b.PitouAct(false).Damage > a.PitouAct(false).Damage);
            }),
            ("Transformação ocorre com HP de Pitou <= 50%", () =>
            {
                var c = new BattleCore();
                c.Pitou.Hp = 161; True(!c.ShouldTransform);
                c.Pitou.Hp = 160; True(c.ShouldTransform);
                c.Transform(); True(c.Adult && !c.ShouldTransform);
            }),
            ("Gon adulto: cada golpe é mais forte que o anterior", () =>
            {
                var c = new BattleCore(); c.Transform();
                c.Pitou.Hp = 100000; c.Pitou.MaxHp = 100000;
                int last = 0;
                for (int i = 0; i < 5; i++)
                {
                    int d = c.GonAct(GonMove.Golpe, false).Damage;
                    True(d > last); last = d;
                }
            }),
            ("Terpsichora mantém Pitou de pé até o Jajanken final", () =>
            {
                var c = new BattleCore(); c.Transform();
                for (int i = 0; i < 10; i++) c.GonAct(GonMove.Golpe, false);
                Eq(1, c.Pitou.Hp);
                True(!c.PitouDefeated);
                True(c.CanUse(GonMove.JajankenFinal));
                c.GonAct(GonMove.JajankenFinal, true);
                True(c.PitouDefeated);
            }),
            ("Jajanken final exige Poder suficiente", () =>
            {
                var c = new BattleCore(); c.Transform();
                True(!c.CanUse(GonMove.JajankenFinal));
            }),
            ("Comando cronometrado: janela de acerto", () =>
            {
                True(TimingRing.IsPerfect(0.7f, 0.7f));
                True(TimingRing.IsPerfect(0.62f, 0.7f));
                True(!TimingRing.IsPerfect(0.5f, 0.7f));
                True(!TimingRing.IsPerfect(0.85f, 0.7f));
            }),
            ("Batalha simulada termina em vitória", () =>
            {
                var c = new BattleCore();
                for (int round = 0; round < 200 && !c.PitouDefeated; round++)
                {
                    GonMove m = c.Adult ? (c.CanUse(GonMove.JajankenFinal) ? GonMove.JajankenFinal : GonMove.Golpe)
                                        : (c.CanUse(GonMove.Pedra) ? GonMove.Pedra : GonMove.Carregar);
                    c.BeginRound(m);
                    foreach (var a in c.TurnOrder())
                    {
                        if (a == Actor.Gon) c.GonAct(m, true); else c.PitouAct(true);
                        if (c.PitouDefeated || c.ShouldTransform) break;
                    }
                    if (c.ShouldTransform) c.Transform();
                    c.EndRound();
                    True(!c.GonDefeated);
                }
                True(c.PitouDefeated);
            }),
            ("Textos da história carregam do JSON", () =>
            {
                foreach (var key in new[] { "intro", "transform", "victory", "credits" })
                    True(StoryData.Get(key).Count > 0);
            }),
        };

        foreach (var (name, fn) in tests)
        {
            try { fn(); passed++; GD.Print($"  PASS  {name}"); }
            catch (Exception e) { failed++; GD.PrintErr($"  FAIL  {name}: {e.Message}"); }
        }
        GD.Print($"\n{passed} passaram, {failed} falharam.");
        GetTree().Quit(failed == 0 ? 0 : 1);
    }

    static void Eq<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new Exception($"esperado {expected}, obtido {actual}");
    }

    static void True(bool cond)
    {
        if (!cond) throw new Exception("condição falsa");
    }
}
