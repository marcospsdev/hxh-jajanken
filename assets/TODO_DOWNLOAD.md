# Sprites para baixar manualmente

Não consegui baixar os sprites automaticamente: os sites bloqueiam acesso automatizado ou exigem login.
Pela regra do projeto, não tentei contornar os bloqueios.

| Site | Resultado |
|---|---|
| spriters-resource.com | 403 (bloqueado) |
| mugenarchive.com | 403 (exige login para baixar) |
| kodaika.com | 404 |

## Links encontrados

| Personagem | Link | Observação |
|---|---|---|
| Neferpitou | https://mugenarchive.com/forums/downloads.php?do=file&id=98157-neferpitou-lemonstream | Personagem MUGEN (estilo Jump Ultimate Stars), autor **lemonstream**. Precisa de conta no MUGEN Archive. |
| Neferpitou | https://kodaika.com/neferpitou-v-2-jus/ | "Neferpitou (V.2) JUS", autor **UraharaGreenHat**. A página deu 404 para mim; talvez abra no navegador. |
| Gon / Gon adulto | https://www.spriters-resource.com/ (buscar "Hunter x Hunter" e "Jump Ultimate Stars") | Folhas em PNG, mais fáceis de recortar. |
| Gon / Gon adulto | https://mugenarchive.com (buscar "Gon Freecss" e "Gon adult") | Personagens MUGEN de fãs. |

Personagens MUGEN vêm em `.sff`. Para extrair os quadros em PNG, use o **Fighter Factory** (gratuito).

## Onde colocar

Coloque **um PNG por personagem**, com fundo transparente, virado para a **direita**, pose parada (idle):

```
assets/sprites/gon.png
assets/sprites/gon_adulto.png
assets/sprites/pitou.png
assets/sprites/komugi.png   (opcional, aparece na história)
```

O jogo detecta os arquivos sozinho e troca o provisório pelo real, sempre com escala inteira e filtro Nearest.
Depois de colocar os arquivos, abra o projeto uma vez na Godot (ou rode `--import`) e me avise:
eu ajusto a escala e atualizo o `CREDITS.md`.
