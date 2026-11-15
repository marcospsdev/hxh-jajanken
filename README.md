# JAJANKEN — Gon vs. Neferpitou

Duelo curto por turnos (fã-jogo de Hunter x Hunter, arco das Formigas Quimera) em Godot 4.7 .NET + C#.

## Como rodar
- **Godot:** abra a pasta na Godot 4.7 .NET, clique em *Build* e depois em *Play* (F5).
- **Rider:** abra `HxHJajanken.sln` para editar e compilar.
- **Linha de comando:** `dotnet build` e depois `godot --path .`
- **Testes (headless):**
  - `godot --headless --path . res://tests/TestRunner.tscn` roda 20 testes de regras.
  - `godot --headless --path . res://tests/BattleSmoke.tscn` simula uma batalha inteira.

> Se `godot` der "Assemblies not found", rode o executável real da pasta do winget
> (`...\WinGet\Packages\GodotEngine.GodotEngine.Mono_*\Godot_v4.7.2-stable_mono_win64\`).
> O atalho do winget não acha a pasta `GodotSharp`.

## Controles
- **Setas:** navegar no menu.
- **Enter/Espaço:** confirmar, avançar o texto e acionar os comandos cronometrados.

## Como se joga
- **Pedra:** dano alto. Custa 15 de aura.
- **Tesoura:** dano médio, causa Sangramento. Custa 8.
- **Papel:** projétil que ignora a defesa. Custa 10.
- **Carregar:** recupera aura; o próximo Jajanken causa dano dobrado.
- **Defender:** dano pela metade.
- **Comando cronometrado:** aperte quando o anel se fechar. No ataque, dá dano x1.5; na defesa, o dano cai para 25%.
- **Telegrafia:** quando Pitou se agacha, o próximo golpe é o Salto Felino.
- **Transformação:** com Pitou a 50% de HP, Gon vira adulto. Cada Golpe fica mais forte. Com Poder 4, o Jajanken: PEDRA final vira um comando cronometrado de 3 tempos (Jan, Ken, Pedra).
- **Ordem de turno:** definida pela Velocidade e mostrada no topo da tela.

## Estrutura
- `scripts/Battle/`: `BattleCore` (regras puras e testáveis), `BattleScene` (apresentação e efeitos), `TimingRing` (comando cronometrado).
- `scripts/UI/`: telas, HUD, efeitos, sprites provisórios (`PixelArt`) e cenário.
- `scripts/Story/` e `data/story.json`: telas de história e textos.
- `tests/`: testes headless.

## Pronto
- Fluxo completo: título, introdução, batalha, transformação, vitória, créditos, e derrota com "tentar novamente".
- Comandos cronometrados.
- Partículas de aura, números de dano, tremida de tela e flash.

## Falta
- **Sprites reais:** ver `assets/TODO_DOWNLOAD.md`. Os atuais são pixel art provisória gerada em código.
- Animações por quadros (hoje são tweens).
- Áudio.
- Exportação para `.exe`.
