# TrafficMonster

[![Unity](https://img.shields.io/badge/Unity-2022.3%20LTS-blue.svg)](https://unity3d.com/get-unity/download)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

A 3D endless-runner racing game made in Unity. You drive through traffic, keep an eye on your fuel, shake off police chases and try to reach the "Safe Haven" 5000 meters away. Upgrades are bought at pit stops during the run.

Project presentation video: https://youtu.be/MHMV2glUEes

## Objective

Reach the Safe Haven (5000 m) without running out of fuel or getting busted by the police.

## Features

**Police chase**
- A chase starts automatically every 30 seconds.
- Police speed scales with your upgrades and elapsed time.
- A bust meter fills while a police car is close to you; you have to get away before it fills.
- A pit stop is the only thing that despawns the police and resets the chase.

**Fuel**
- Consumption depends on speed and lane changes.
- A lane change costs 1 fuel unit and slows the car down for a moment.
- Fuel pickups spawn more often when the tank is low.

**Upgrades (per run)**
- Engine: higher top speed.
- Fuel tank: bigger capacity and an immediate refill.
- Handling: smaller speed penalty on lane changes.
- You can only afford one upgrade at the first pit stop, so the first choice matters.

**Power-ups and pickups**
- Speed boost (two thirds of power-up spawns): +50% speed for 8 seconds.
- Shield (one third of spawns): 5 seconds of invincibility.
- Coins: 75 per pickup, usually placed so you have to change lanes to get them.

**Traffic**
- Cars spawn per lane and the spawner checks for overlap before placing a new one.
- Traffic density rises with difficulty and is lower around pit stops.

**UI**
- Speed (km/h), fuel gauge (yellow below 10, red below 5), distance left to the goal, and a finish screen with your time and best time.

## Code

26 C# scripts, about 3,200 lines, in `Assets/Scripts/` (game manager, car controller, police AI and manager, fuel, traffic and lane managers, spawners, power-ups, pit stop and upgrade shop UI). Several managers (`GameManager`, `PoliceManager`, `UpgradeManager`, ...) are singletons that the other scripts talk to directly.

## Controls

- W / S: accelerate / brake (Shift+S to reverse)
- A / D: change lane
- Space: emergency brake
- Mouse: menus

## Open the project

You need Unity 2022.3 LTS (the project was last saved with 2022.3.62f1).

```bash
git clone https://github.com/erencivril/TrafficMonster.git
```

Add the folder in Unity Hub, open `Assets/Scenes/SampleScene.unity` and press Play. To make a build, use File > Build Settings with `SampleScene` in the list.

## Licenses and credits

The code in this repository is MIT licensed (see `LICENSE`). The art and audio assets are third-party packs (Simple Vehicle Pack, Simple Retro Car, Free Sports Car, Simple Nature Pack, "Car Engine Sound - i6 German Free", TextMesh Pro) and keep their own licenses.
