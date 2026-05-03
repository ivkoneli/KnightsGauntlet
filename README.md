# Turn-Based RPG Game ( kNIGHTS GAUNTLET )

## Overview

This is a full-stack turn-based RPG game where the player progresses through a series of battles against five different monsters.  
The core idea is to gradually build a stronger moveset by learning abilities from defeated enemies and adapting strategy between fights.

## Gameplay

- Turn-based combat between the player and a monster
- Player selects an action each turn, followed by the monster’s response calculated by the backend logic
- Each monster has unique abilities and behavior
- After winning a fight, the player learns one random ability from that monster and gains experience
- Before the next fight, the player can adjust their moveset
- Fights are replayable to experiment with different outcomes and builds

## Tech Stack

- Frontend: Unity Engine (C#)
- Backend: FastAPI (Python)
- Hosting: Railway

## Structure

- Unity client handles gameplay, UI, and player interaction
- FastAPI backend provides game configuration and data
- Clear separation between client logic and backend services
- Possible to alter and expand game logic in the future 
