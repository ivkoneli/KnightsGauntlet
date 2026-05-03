import random

GAME_CONFIG = {
    "hero": {
        "id": "knight",
        "name": "Knight",
        "sprite_sheet": "rogues",
        "sprite_row": 1,
        "sprite_col": 0,
        "base_stats": {"atk": 10, "def": 8, "hp": 100, "mag": 6},
        "stats_per_level": {"atk": 2, "def": 1, "hp": 15, "mag": 1},
        "starter_moves": ["slash", "shield_up", "battle_cry", "second_wind"],
        "max_moves": 4
    },
    "moves": {
        "slash": {
            "id": "slash", "name": "Slash",
            "description": "A basic sword slash dealing physical damage.",
            "type": "physical", "target": "enemy",
            "atk_multiplier": 0.8, "icon": "slash_sword"
        },
        "shield_up": {
            "id": "shield_up", "name": "Shield Up",
            "description": "Raise your shield, boosting DEF by 5 for 2 turns.",
            "type": "buff", "target": "self",
            "stat_changes": [{"stat": "def", "amount": 5, "duration": 2}],
            "cooldown": 2, "icon": "shield_up"
        },
        "battle_cry": {
            "id": "battle_cry", "name": "Battle Cry",
            "description": "A fierce war cry that boosts ATK by 4 for 2 turns.",
            "type": "buff", "target": "self",
            "stat_changes": [{"stat": "atk", "amount": 4, "duration": 2}],
            "cooldown": 2, "icon": "battle_cry"
        },
        "second_wind": {
            "id": "second_wind", "name": "Second Wind",
            "description": "Catch your breath, healing 30% of max HP.",
            "type": "heal", "target": "self",
            "heal_percent": 0.3, "cooldown": 3, "icon": "second_wind"
        },
        "rusty_blade": {
            "id": "rusty_blade", "name": "Rusty Blade",
            "description": "A rusty sword strike dealing physical damage.",
            "type": "physical", "target": "enemy",
            "atk_multiplier": 0.8, "icon": "rusty_blade"
        },
        "dirty_kick": {
            "id": "dirty_kick", "name": "Dirty Kick",
            "description": "A cheap kick that deals damage and reduces enemy ATK by 2 for 2 turns.",
            "type": "physical", "target": "enemy",
            "atk_multiplier": 0.5,
            "stat_changes": [{"stat": "atk", "amount": -2, "duration": 2}],
            "cooldown": 1, "icon": "dirty_kick"
        },
        "frenzy": {
            "id": "frenzy", "name": "Frenzy",
            "description": "Enter a rage, boosting own ATK by 3 for 2 turns.",
            "type": "buff", "target": "self",
            "stat_changes": [{"stat": "atk", "amount": 3, "duration": 2}],
            "cooldown": 2, "icon": "frenzy"
        },
        "headbutt": {
            "id": "headbutt", "name": "Headbutt",
            "description": "A stunning headbutt dealing 60% ATK with a 30% stun chance.",
            "type": "physical", "target": "enemy",
            "atk_multiplier": 0.6, "stun_chance": 0.3, "cooldown": 2, "icon": "headbutt"
        },
        "firebolt": {
            "id": "firebolt", "name": "Firebolt",
            "description": "A bolt of fire dealing 80% MAG magic damage.",
            "type": "magic", "target": "enemy",
            "mag_multiplier": 0.8, "cooldown": 1, "icon": "firebolt"
        },
        "arcane_surge": {
            "id": "arcane_surge", "name": "Arcane Surge",
            "description": "Channel arcane power, boosting own MAG by 4 for 2 turns.",
            "type": "buff", "target": "self",
            "stat_changes": [{"stat": "mag", "amount": 4, "duration": 2}],
            "cooldown": 2, "icon": "arcane_surge"
        },
        "mana_drain": {
            "id": "mana_drain", "name": "Mana Drain",
            "description": "Siphon magical energy, reducing enemy MAG by 3 and healing self 5 HP.",
            "type": "magic", "target": "enemy",
            "mag_multiplier": 0.2,
            "stat_changes": [{"stat": "mag", "amount": -3, "duration": 2}],
            "self_heal": 5, "cooldown": 2, "icon": "mana_drain"
        },
        "hex_shield": {
            "id": "hex_shield", "name": "Hex Shield",
            "description": "Conjure a magical barrier, boosting own DEF by 4 for 2 turns.",
            "type": "buff", "target": "self",
            "stat_changes": [{"stat": "def", "amount": 4, "duration": 2}],
            "cooldown": 2, "icon": "hex_shield"
        },
        "bite": {
            "id": "bite", "name": "Bite",
            "description": "A venomous bite dealing 70% ATK and poisoning for 5 HP/turn for 2 turns.",
            "type": "physical", "target": "enemy",
            "atk_multiplier": 0.7,
            "dot": {"damage": 5, "duration": 2, "type": "poison"},
            "cooldown": 2, "icon": "bite"
        },
        "web_throw": {
            "id": "web_throw", "name": "Web Throw",
            "description": "Tangle the enemy in webbing, reducing their DEF by 3 for 2 turns.",
            "type": "physical", "target": "enemy",
            "atk_multiplier": 0.3,
            "stat_changes": [{"stat": "def", "amount": -3, "duration": 2}],
            "cooldown": 1, "icon": "web_throw"
        },
        "pounce": {
            "id": "pounce", "name": "Pounce",
            "description": "A powerful leaping attack dealing 120% ATK physical damage.",
            "type": "physical", "target": "enemy",
            "atk_multiplier": 1.2, "cooldown": 2, "icon": "pounce"
        },
        "skitter": {
            "id": "skitter", "name": "Skitter",
            "description": "Scuttle rapidly, increasing own DEF by 5 for 2 turns.",
            "type": "buff", "target": "self",
            "stat_changes": [{"stat": "def", "amount": 5, "duration": 2}],
            "cooldown": 2, "icon": "skitter"
        },
        "shadow_bolt": {
            "id": "shadow_bolt", "name": "Shadow Bolt",
            "description": "A bolt of dark energy dealing 90% MAG and reducing enemy DEF by 2.",
            "type": "magic", "target": "enemy",
            "mag_multiplier": 0.9,
            "stat_changes": [{"stat": "def", "amount": -2, "duration": 2}],
            "cooldown": 1, "icon": "shadow_bolt"
        },
        "drain_life": {
            "id": "drain_life", "name": "Drain Life",
            "description": "Drain the enemy's life force for 60% MAG magic damage, healing self for the same amount.",
            "type": "magic", "target": "enemy",
            "mag_multiplier": 0.6, "lifesteal": True, "cooldown": 3, "icon": "drain_life"
        },
        "curse": {
            "id": "curse", "name": "Curse",
            "description": "A terrible curse reducing all enemy stats by 2 for 3 turns.",
            "type": "debuff", "target": "enemy",
            "stat_changes": [
                {"stat": "atk", "amount": -2, "duration": 3},
                {"stat": "def", "amount": -2, "duration": 3},
                {"stat": "mag", "amount": -2, "duration": 3}
            ],
            "cooldown": 3, "icon": "curse"
        },
        "dark_pact": {
            "id": "dark_pact", "name": "Dark Pact",
            "description": "Make a dark pact: boost own MAG by 6 for 1 turn but lose 10 HP.",
            "type": "buff", "target": "self",
            "stat_changes": [{"stat": "mag", "amount": 6, "duration": 1}],
            "self_damage": 10, "cooldown": 2, "icon": "dark_pact"
        },
        "flame_breath": {
            "id": "flame_breath", "name": "Flame Breath",
            "description": "Breathe a torrent of flame dealing 100% MAG magic damage.",
            "type": "magic", "target": "enemy",
            "mag_multiplier": 1.0, "cooldown": 2, "icon": "flame_breath"
        },
        "claw_swipe": {
            "id": "claw_swipe", "name": "Claw Swipe",
            "description": "A devastating claw swipe dealing 110% ATK physical damage.",
            "type": "physical", "target": "enemy",
            "atk_multiplier": 1.1, "cooldown": 2, "icon": "claw_swipe"
        },
        "intimidate": {
            "id": "intimidate", "name": "Intimidate",
            "description": "A terrifying roar that reduces enemy ATK by 4 for 2 turns.",
            "type": "debuff", "target": "enemy",
            "stat_changes": [{"stat": "atk", "amount": -4, "duration": 2}],
            "cooldown": 2, "icon": "intimidate"
        },
        "dragon_scales": {
            "id": "dragon_scales", "name": "Dragon Scales",
            "description": "Harden your scales, boosting own DEF by 8 for 3 turns.",
            "type": "buff", "target": "self",
            "stat_changes": [{"stat": "def", "amount": 8, "duration": 3}],
            "cooldown": 3, "icon": "dragon_scales"
        }
    },
    "monsters": [
        {
            "id": "goblin_warrior",
            "name": "Goblin Warrior",
            "description": "A small but ferocious goblin with a rusty blade.",
            "sprite_sheet": "monsters",
            "sprite_row": 0, "sprite_col": 2,
            "stats": {"atk": 8, "def": 5, "hp": 60, "mag": 2},
            "moves": ["rusty_blade", "dirty_kick", "frenzy", "headbutt"],
            "xp_reward": 30, "difficulty": 1
        },
        {
            "id": "goblin_mage",
            "name": "Goblin Mage",
            "description": "A goblin who has mastered dangerous fire magic.",
            "sprite_sheet": "monsters",
            "sprite_row": 0, "sprite_col": 6,
            "stats": {"atk": 4, "def": 4, "hp": 50, "mag": 12},
            "moves": ["firebolt", "arcane_surge", "mana_drain", "hex_shield"],
            "xp_reward": 40, "difficulty": 2
        },
        {
            "id": "giant_spider",
            "name": "Giant Spider",
            "description": "A massive spider with paralyzing venom and sticky webs.",
            "sprite_sheet": "monsters",
            "sprite_row": 6, "sprite_col": 8,
            "stats": {"atk": 10, "def": 7, "hp": 70, "mag": 4},
            "moves": ["bite", "web_throw", "pounce", "skitter"],
            "xp_reward": 55, "difficulty": 3
        },
        {
            "id": "witch",
            "name": "Hag Witch",
            "description": "An ancient witch wielding devastating dark magic and curses.",
            "sprite_sheet": "monsters",
            "sprite_row": 5, "sprite_col": 4,
            "stats": {"atk": 6, "def": 6, "hp": 80, "mag": 16},
            "moves": ["shadow_bolt", "drain_life", "curse", "dark_pact"],
            "xp_reward": 70, "difficulty": 4
        },
        {
            "id": "dragon",
            "name": "Dragon",
            "description": "A fearsome dragon with near-impenetrable scales and devastating flame breath.",
            "sprite_sheet": "monsters",
            "sprite_row": 8, "sprite_col": 2,
            "stats": {"atk": 18, "def": 14, "hp": 120, "mag": 12},
            "moves": ["flame_breath", "claw_swipe", "intimidate", "dragon_scales"],
            "xp_reward": 100, "difficulty": 5
        }
    ]
}


# GAME-LOGIC func that calculates the best move possible for the boss given the current game state , and return that moves ID 
# 

def get_monster_ai_move(
    monster_id: str,
    monster_hp: int,
    monster_max_hp: int,
    hero_hp: int,
    hero_max_hp: int,
    hero_atk: int,
    hero_def: int,
    hero_mag: int,
    last_move: str,
    turn: int,
    cooldown_moves: list = None,
) -> str:
    monster = next((m for m in GAME_CONFIG["monsters"] if m["id"] == monster_id), None)
    if not monster:
        return "rusty_blade"

    all_moves = monster["moves"]
    # Filter out moves that are on cooldown; fall back to full list if everything is cooled down
    moves = [m for m in all_moves if not cooldown_moves or m not in cooldown_moves]
    if not moves:
        moves = all_moves
    monster_hp_pct = monster_hp / max(monster_max_hp, 1)
    hero_hp_pct = hero_hp / max(hero_max_hp, 1)

    weights = {}
    for move_id in moves:
        move = GAME_CONFIG["moves"].get(move_id, {})
        move_type = move.get("type", "physical")
        w = 1.0

        # Avoid repeating the same move twice
        if move_id == last_move:
            w *= 0.15

        # Monster is low HP: strongly prefer heals and defensive buffs; avoid self-damage
        if monster_hp_pct < 0.3:
            if move_id == "drain_life":     #TO-DO  make these TYPE based not tied to specific spells
                w *= 4.0
            if move_id == "dark_pact":
                w *= 0.05
            stat_changes = move.get("stat_changes", [])
            if any(sc["stat"] == "def" and sc["amount"] > 0 for sc in stat_changes):
                w *= 2.5

        # Hero is low HP: prioritize finishing moves
        if hero_hp_pct < 0.35:
            mult = move.get("atk_multiplier", move.get("mag_multiplier", 0))
            if mult >= 1.0:
                w *= 3.5
            elif mult >= 0.7:
                w *= 1.8

        # Hero has high ATK: debuff their attack
        if hero_atk > 14 and move_id in ("dirty_kick", "intimidate", "curse"):
            w *= 2.5

        # Hero has high DEF: use DEF-stripping moves
        if hero_def > 12 and move_id in ("web_throw", "shadow_bolt", "curse"):
            w *= 2.5

        # Hero has high MAG: use magic resistance buffs
        if hero_mag > 10 and move_id in ("hex_shield", "dragon_scales"):
            w *= 1.8

        # Turn 1: never waste a buff (use an attack to probe)
        if turn == 1 and move_type in ("buff",):
            w *= 0.3

        weights[move_id] = max(w, 0.05)

    total = sum(weights.values())
    roll = random.uniform(0, total)
    cumulative = 0.0
    for move_id, w in weights.items():
        cumulative += w
        if roll <= cumulative:
            return move_id

    return moves[0]
