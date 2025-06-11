# 🚗 TrafficMonster - Unity Runner Racing Game

[![Unity Version](https://img.shields.io/badge/Unity-2022.3%20LTS-blue.svg)](https://unity3d.com/get-unity/download)
[![Platform](https://img.shields.io/badge/Platform-PC%20%7C%20Windows-lightgrey.svg)](https://github.com/erencivril/TrafficMonster)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

> **A 3D runner racing game with advanced police chase mechanics, strategic resource management, and dynamic difficulty scaling.**

## 🎥 Project Presentation

📺 **[Watch Our Project Presentation Video](https://youtu.be/MHMV2glUEes?feature=shared)**

## 📖 Project Overview

TrafficMonster is a sophisticated Unity 3D runner that transforms the traditional genre into a strategic racing experience. Players must navigate through dense traffic, manage fuel resources, evade police chases, and reach the "Safe Haven" while making tactical decisions about upgrades and power-ups.

### 🎯 Core Objective
Reach the **Safe Haven** (5000 meters) without running out of fuel or being caught by police, while avoiding traffic collisions and managing strategic resources.

## ✨ Key Features

### 🚔 Advanced Police Chase System
- **Automatic Triggers**: Police chases activate every 30 seconds
- **Dynamic AI**: Police speed scales with player upgrades and game time
- **Bust Meter**: Skill-based escape mechanics with visual feedback
- **Strategic Pit Stops**: Only way to despawn police and reset encounters

### ⛽ Resource Management
- **Fuel System**: Dynamic consumption based on speed and lane changes
- **Lane Change Penalties**: 1 fuel unit cost + speed reduction
- **Emergency Spawning**: Increased fuel pickup frequency when running low
- **Upgrade Integration**: Fuel tank capacity upgrades with bonus refills

### 🔧 Session-Based Progression
- **Engine Upgrades**: Speed improvements (72→108 km/h progression)
- **Fuel Upgrades**: Tank capacity increases with immediate refills
- **Handling Upgrades**: Lane change speed penalty reduction (50%→0%)
- **Strategic Economy**: Can only afford 1 upgrade at first pit stop

### 🎮 Power-Up Systems
- **Speed Boost** (2/3 spawns): 50% speed increase for 8 seconds - perfect for police escapes
- **Shield Protection** (1/3 spawns): 5 seconds full invincibility with visual/audio feedback
- **Coin Pickups**: 75 coins per collection with risk/reward lane changing

### 🚗 Traffic & Physics
- **Lane Management**: Predictive spawning algorithms prevent clipping
- **Dynamic Density**: Multi-car spawning with difficulty scaling
- **Collision Variety**: Different car models with size-appropriate colliders
- **Safe Zones**: Reduced traffic density around pit stops

### 📊 Real-Time UI
- **Speed Meter**: km/h display with effective speed including penalties
- **Fuel Gauge**: Dynamic color changes (yellow<10, red<5)
- **Distance Tracking**: Countdown to Safe Haven goal
- **Victory System**: Completion time tracking with best time records

## 🛠️ Technical Achievements

### 📁 Architecture
- **30+ C# Scripts** with singleton patterns
- **6,000+ Lines of Code** with clean, modular design
- **Cross-System Integration** with efficient communication patterns
- **Performance Optimization** with object pooling and smart spawning

### 🧠 Advanced Systems
- **Predictive Traffic Spawning**: Lane-based algorithms prevent car overlap
- **Dynamic Difficulty Scaling**: All systems adapt to player progress
- **Observer Pattern Implementation**: Event-driven architecture
- **Memory Management**: Automatic cleanup and resource optimization

## 🎮 How to Play

### Controls
- **W/S**: Accelerate/Brake (Shift+S for reverse)
- **A/D**: Lane changing (costs fuel and speed)
- **Space**: Emergency brake
- **Mouse**: Navigate menus

### Strategy Tips
1. **Fuel Management**: Monitor consumption and prioritize fuel pickups
2. **Police Escapes**: Use speed boosts when police get close
3. **Upgrade Planning**: Choose first upgrade wisely - you can only afford one!
4. **Risk/Reward**: Decide if coin collection is worth the lane change risk

## 🚀 Getting Started

### Prerequisites
- Unity 2022.3 LTS
- Visual Studio Community 2022
- Windows 10/11

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/erencivril/TrafficMonster.git
   ```
2. Open Unity Hub and add the project
3. Load `Assets/Scenes/SampleScene.unity`
4. Press Play to start the game

### Building
1. Go to `File > Build Settings`
2. Ensure `SampleScene` is in build
3. Select platform and click `Build`

## 🎯 Technical Highlights

- **Singleton Patterns** for centralized management
- **Predictive Algorithms** for traffic spawning
- **Dynamic Difficulty Scaling** across all systems
- **Session-Based Progression** with meaningful choices
- **UI/UX** with real-time feedback
- **Advanced AI** with behavioral patterns

