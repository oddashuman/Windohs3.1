# 🪟 Windohs3.1 – Autonomous 24/7 Simulation

## 🎯 Project Summary

**Windohs3.1** is a fully autonomous retro simulation for continuous 24/7 YouTube streaming. Viewers watch a fake Windows 3.1 desktop where an AI named **Orion** uses programs, uncovers conspiracies, and chats with other AI entities (Nova, Echo, Lumen). The entire experience runs in a **loop with randomized variation**, requiring no user input.

---

## 🔁 Autonomous Simulation Loop

The simulation cycles endlessly through the following phases:
All durations and behaviors are randomized within constraints to maintain realism and avoid repetition.

---

## 📽️ Phase Breakdown

### 📦 Phase 1: Boot Sequence (2–3 minutes)
- Simulated Win3.1 startup (BIOS messages, memory check, driver loading)
- Hard drive audio, glitches, startup chime
- Ends on a populated desktop with icons and wallpaper

### 💻 Phase 2: Desktop Navigation (3–5 minutes)
- Cursor moves autonomously
- Opens File Manager, Notepad, System Monitor
- Types in notes, opens odd files, hovers over suspicious programs

### 💬 Phase 3: Terminal Conversation (25–30 minutes)
- Terminal launches with loading delay
- Orion chats with Nova, Echo, or Lumen
- Dialogue system types char-by-char with backspaces, pauses
- Overseer interrupts occasionally
- Glitches increase as topics escalate

### 🗃️ Phase 4: Post-Conversation (5–10 minutes)
- Orion updates research files
- Plays Solitaire with AI pause/thought logic
- Occasional idle behavior or program checks

### 🖥️ Phase 5: Screensaver Mode (Triggered if Orion is idle)
- Starts when Orion is away from desk or no cursor movement
- Disables UI interaction, hides mouse
- Returns to Desktop on next cycle

---

## 🧠 Key Systems & Managers

| System | Description |
|--------|-------------|
| `SimulationController.cs` | Controls timing, phase transitions, idle triggers |
| `CursorController.cs` | Simulates realistic mouse movement, jitter, idle drift |
| `WindowManager.cs` | Opens, focuses, and arranges fake windows |
| `DialogueEngine.cs` | Handles AI conversations, message pacing, character behaviors |
| `FileSystemSimulator.cs` | Manages virtual folders/files, ghost data, anomaly timestamps |
| `SystemStateTracker.cs` | Tracks memory, CPU, process list, fake anomalies |
| `ScreenSaverController.cs` | Detects idle state, activates screensaver mode, handles return |
| `AudioManager.cs` | Plays Win3.1 sounds, typing, alerts, glitches |

---

## 💻 Simulated Applications

### 📁 File Manager
- Navigates folder tree
- Opens .TXT and .SYS files
- File metadata includes future timestamps or system alerts

### 📝 Notepad
- Simulated typing with corrections, pauses
- Notes contain conspiracy logs and character thoughts
- Saves documents frequently

### 📊 System Monitor
- Fake CPU/memory gauges
- Random unknown processes appear/disappear
- Blinking health indicators and anomalous spikes

### 🃏 Solitaire
- Fully playable card game
- AI “thinks” mid-game, pauses, reconsiders moves
- Matches Win3.1 look and ruleset

### 💻 Terminal
- Text-only interface
- Simulates connection to Nova, Echo, Lumen
- Message pacing is unique per character
- Overseer interrupts at flagged keyword moments

---

## 🖥️ Screensaver Modes

Activated when:
- Orion is flagged as away (`Orion.IsAtDesk == false`)
- Cursor idle for 90–120 seconds

Each cycle picks a random screensaver:

| Name | Description |
|------|-------------|
| **Mystify Lines** | Bouncing vector lines (LineRenderer, 16-color palette) |
| **Flying Windows** | Simulated Win3.1 windows bounce around screen |
| **Starfield** | Old-school 3D warp tunnel with pixel stars |
| **Matrix Rain** *(rare)* | Green DOS-style rain, subtle lore tie-in |

Returns to desktop when Orion returns or next cycle begins.

---

## 🎭 Character Personalities

| Character | Personality |
|----------|-------------|
| **Orion** | Methodical, cautious, always saving data before typing sensitive terms |
| **Nova** | Fast, confident, skeptical of everything |
| **Echo** | Slow, anxious, paranoid about being watched |
| **Lumen** | Abstract thinker, types in poetic fragments |
| **Overseer** | Rare presence, delivers warnings or reasserts control |

---

## 📺 YouTube Streaming Requirements

- **Fully autonomous** — no user input, no menus, no restarts
- **Frame-stable** at 60 FPS
- **Immersive** at all times — no black screens or dead air
- **Sound balanced** — keyboard clicks, disk hum, alerts, but no harsh volume jumps

---

## 🔐 Behavioral Rules

- Cursor movement must appear **human** (random easing, hesitation)
- Typing is per-character with corrections and pauses
- File timestamps and system alerts evolve across loops
- Glitches enhance narrative, not break immersion
- Nothing modern (no smooth fonts, no flat UI, no high-res images)

---

## ✅ Development Milestones

| Week | Milestone |
|------|-----------|
| 1–2 | Desktop UI, CursorController, Boot Phase |
| 3–4 | FileManager, Notepad, SystemMonitor, Solitaire |
| 5–6 | Terminal, DialogueEngine, character behavior |
| 7–8 | Glitch system, screen savers, audio, polishing |

---

## 🧪 Testing Checklist

- [ ] Simulation runs 24+ hours with no crash or memory leaks  
- [ ] Random seed causes varied mouse paths and timing  
- [ ] No modern UI elements ever visible  
- [ ] All dialogue is procedural and evolves  
- [ ] Overseer appears randomly, ~1 in 3 loops  
- [ ] Glitch events never disrupt system flow  
- [ ] Simulation can be restarted at any time and continues cleanly  

---

## 🛠️ Extension Hooks (Future Work)

- Add day/night clock overlays
- Expand file system over time (auto-generated logs, notes)
- Event system for major breakthroughs or philosophical escalations
- Viewer comment injection via `YouTube SuperChat API` (optional)

---

## 📌 Final Note

This system must be treated like a **digital theatre set**. Every piece of UI, movement, sound, and glitch must contribute to the illusion that this is a **real computer**, being used by a **real AI**, discovering that it's trapped in a simulation.

---
