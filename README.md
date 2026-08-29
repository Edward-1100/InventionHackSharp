# InventionHackSharp
Built on [InventionHackSharp](https://github.com/dustinandrews/InventionHackSharp) by Dustin Andrews, which was itself built on top of the [Invention API](https://gitlab.com/hodgskin-callan/Invention) by Callan Hodgskin and [RogueSharp](https://github.com/FaronBracy/RogueSharp) by FaronBracy


## Prerequisites

- [.NET Framework 4.6.1 Developer Pack](https://dotnet.microsoft.com/download/dotnet-framework/net461) (needed to build the WPF UI)
- [.NET 8 SDK](https://dotnet.microsoft.com/download) or later (needed to build and test the core engine)

## Installing

git clone https://github.com/Edward-1100/InventionHackSharp

cd InventionHackSharp

dotnet restore


## Build and test

dotnet build

dotnet test


## Running

Run this command in PowerShell from the project root after building:

.\bin\Debug\InventionUiWpf\net461\InventionUiWpf.exe


## Modding

### Monsters

Drop additional monster definitions into a `Mods/Monsters/` folder (same JSON shape as `Monsters/*.json`, keyed by monster name):

```json
{
  "Cave Troll": {
    "Name":{"NameString": "Cave Troll"},

    "Actor":{"Speed": 6.0, "Gold": 15, "Energy": 0.0},
    "AttackStat":{"Power": 8, "Accuracy": 55},
    "DefenseStat":{"Chance": 15},
    "Life":{"Health": 40, "MaxHealth": 40},

    "Glyph":{"glyph": 0},
    "Faction":{"Type": 2},

    "WanderingMonster":{},
    "AIProfile":{"Rules":[{"Condition": "LowHealth", "Behavior": "Flee"},{"Condition": "NearPlayer", "Behavior": "Attack"},{"Condition": "Default", "Behavior": "Wander"}]}
  }
}
```

`Mods/Monsters/` is loaded after `Monsters/`, and merged with it. If a mod entry shares a name with an included monster (or another mod), what ever mod is loaded last is used.
