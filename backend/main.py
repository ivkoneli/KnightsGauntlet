from fastapi import FastAPI, Query
from fastapi.middleware.cors import CORSMiddleware
from typing import Optional
from game_data import GAME_CONFIG, get_monster_ai_move

app = FastAPI(
    title="Gauntlet of Runes API",
    description="Backend for the Gauntlet of Runes turn-based RPG",
    version="1.0.0"
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["GET"],
    allow_headers=["*"],
)


@app.get("/run/config")
def get_run_config():
    """
    Returns the full game configuration: hero base data, all moves with
    their stats/effects, and the 5 monsters in gauntlet order.
    Unity calls this once on startup to populate all game data.
    """
    return GAME_CONFIG


@app.get("/monster/move")
def get_monster_move(
    monster_id: str = Query(..., description="Monster's id string (e.g. 'goblin_warrior')"),
    monster_hp: int = Query(..., description="Monster's current HP"),
    monster_max_hp: int = Query(..., description="Monster's maximum HP"),
    hero_hp: int = Query(..., description="Hero's current HP"),
    hero_max_hp: int = Query(..., description="Hero's maximum HP"),
    hero_atk: int = Query(..., description="Hero's current ATK (including active buffs/debuffs)"),
    hero_def: int = Query(..., description="Hero's current DEF (including active buffs/debuffs)"),
    hero_mag: int = Query(..., description="Hero's current MAG (including active buffs/debuffs)"),
    last_move: Optional[str] = Query(None, description="The monster's last used move id (for variety)"),
    turn: int = Query(1, ge=1, description="Current turn number in this fight"),
    cooldown_moves: Optional[str] = Query(None, description="Comma-separated move ids currently on cooldown"),
):
    """
    Returns the move id the monster should use this turn.
    The AI weighs moves based on HP thresholds, hero stats, and move history.
    Unity calls this at the start of each monster turn.
    """
    cd_list = cooldown_moves.split(",") if cooldown_moves else []
    move_id = get_monster_ai_move(
        monster_id=monster_id,
        monster_hp=monster_hp,
        monster_max_hp=monster_max_hp,
        hero_hp=hero_hp,
        hero_max_hp=hero_max_hp,
        hero_atk=hero_atk,
        hero_def=hero_def,
        hero_mag=hero_mag,
        last_move=last_move,
        turn=turn,
        cooldown_moves=cd_list,
    )
    return {"move_id": move_id}
