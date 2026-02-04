# 🍺 Order Up: Tavern

<img width="767" height="710" alt="Capture d&#39;écran 2026-02-04 155030" src="https://github.com/user-attachments/assets/ff4ebab4-8a1c-454d-93e3-809a8f097b23" />

**A First-Person Tavern Management Simulation developed with Unity 6.**

> **Project Type:** Technical Portfolio / Vertical Slice
> **Focus:** C# Architecture, AI State Machines, and Gameplay Loops.

[![Unity 6](https://img.shields.io/badge/Unity-6000.0-black.svg?style=flat&logo=unity)](https://unity.com/)
[![Language](https://img.shields.io/badge/Language-C%23-blue.svg)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Playable Demo](https://img.shields.io/badge/Itch.io-Play_Demo-fa5c5c.svg)](https://barredodavid.itch.io/order-up-tavern)

## 🎮 About The Project
**Order Up: Tavern** places the player in the shoes of a tavern keeper. The goal is to manage the daily flow of customers, serve drinks with physics-based interactions, maintain cleanliness, and reinvest profits into upgrades.

This project was built to demonstrate **clean coding patterns** applied to game development, moving away from "spaghetti code" to a modular, scalable architecture.

---

## ⚙️ Technical Showcase

This repository highlights several key game development concepts:

### 1. Finite State Machine (AI)
Customer behaviors are managed via a strict **FSM (Finite State Machine)** pattern in `CustomerAI.cs`, ensuring predictable and debuggable logic.
* **States:** `WaitingForCleaning` → `Walking` → `Waiting` → `Drinking` → `Leaving`.
* **Logic:** Customers autonomously evaluate their environment (seats available, cleanliness) before transitioning.

### 2. Data-Driven Design (ScriptableObjects)
Hard-coded values are avoided. The game uses **ScriptableObjects** to handle configuration and balancing, allowing designers to tweak values without touching the code.
* **`GameConfig.cs`**: Centralizes global settings (difficulty, spawn rates, patience multipliers).
* **`DrinkData.cs`**: Defines properties for items (prices, fill duration, colors).

### 3. Manager Architecture
The project utilizes singleton Managers to handle specific scopes of the application, ensuring a clear **Separation of Concerns**:
* **`GameManager`**: Handles global game state (Tutorial vs Gameplay) and economy.
* **`DayManager`**: Manages the game loop (Day/Night cycle, daily objectives, resetting the scene).
* **`ShopManager`**: Handles UI updates and real-time stat upgrades.

### 4. Raycast Interaction System
Instead of simple triggers, the player uses a generic Raycast system (`PlayerInteraction.cs`) to interact with the world. This allows for context-sensitive actions (Pouring beer, Washing glasses, Sweeping floors) based on the target object's component.

---

## 📂 Code Structure

Key scripts are organized for readability:

```text
Assets/Scripts/
├── AI/                 # Customer logic (FSM) and Spawning systems
├── Core/
│   ├── Managers/       # Game, Day, Shop, and Tutorial managers
│   └── Data/           # ScriptableObject definitions (Config)
├── Gameplay/
│   ├── Bar/            # Beer Keg and Sink logic
│   ├── Cleaning/       # Stain generation and cleaning mechanics
│   └── Items/          # Grabbable objects (Glass, Broom)
├── Player/             # FPS Controller and Interaction logic
└── UI/                 # HUD and Interface management
