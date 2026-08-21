# Rogueline

2D roguelike / survivor tarzı Unity oyunu.

## Gereksinimler

| | |
|---|---|
| Unity | **2022.3.62f3** (LTS) |
| Render Pipeline | Universal RP 14.0.12 |
| Sürüm | 0.0.2 |

> Projeyi açmadan önce Unity Hub'dan **2022.3.62f3** sürümünü kurun. Farklı bir
> sürümle açmak asset ve prefab bozulmalarına yol açabilir.

## Kurulum

```bash
git clone <repo-url>
```

Ardından Unity Hub → `Add` → klonladığınız klasörü seçin. İlk açılışta Unity
`Library/` klasörünü yeniden oluşturacağı için birkaç dakika sürebilir.

## Sahneler

| Sahne | Açıklama |
|---|---|
| `Assets/Scenes/MainMenu.unity` | Ana menü — buradan başlayın |
| `Assets/Scenes/GameScene.unity` | Asıl oyun sahnesi |
| `Assets/Scenes/SampleScene.unity` | Test / deneme sahnesi |

## Proje Yapısı

```
Assets/
├── Scripts/
│   ├── PlayerScripts/    # Oyuncu hareketi, kontroller
│   ├── EnemyScripts/     # Düşman AI ve davranışları
│   ├── CombatScripts/    # Silahlar, hasar, çarpışma
│   ├── ItemScripts/      # Toplanabilir eşyalar, iksirler
│   ├── World/            # Chunk / harita üretimi
│   ├── Systems/          # GameManager, ses, kayıt
│   ├── UIScripts/        # Menüler, HUD
│   └── Localization/     # Dil desteği
├── Prefabs/
├── Animations/
├── Sprites/  ·  SpriteSheets/
└── Sounds/
```

## Notlar

- `Library/`, `Logs/`, `UserSettings/` ve `.csproj`/`.sln` dosyaları git'e dahil
  değildir — Unity bunları ilk açılışta kendisi üretir.
