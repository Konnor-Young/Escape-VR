import asyncio
import websockets
import json
import uuid
import threading
from pydub import AudioSegment
from pydub.playback import play
import ffmpeg


global buttons, intro, stop_event, playing_sounds
intro = True
buttons = True
players = {}
playing_sounds = []
stop_event = asyncio.Event()


class Player:
    def __init__(self, websocket, player_id, role):
        self.websocket = websocket
        self.id = player_id
        self.role = role

async def play_sound(file_path, loop=False):
    global stop_event, playing_sounds
    sound = AudioSegment.from_wav(file_path)

    def play_audio():
        nonlocal sound
        while not stop_event.is_set():
            play(sound)
            if not loop:
                break
    play_thread = threading.Thread(target=play_audio)
    playing_sounds.append(play_thread)
    play_thread.start()

def stop_all_sounds():
    global stop_event, playing_sounds
    stop_event.set()

    for sound in playing_sounds:
        sound_thread.join()

    stop_event.clear()
    playing_sounds.clear()

async def action_play(player, data):
    global stop_event
    if data.role:
        print("play song")
        file_path = data.get("file")
        loop = data.get("loop", False)
        asyncio.create_task(play_sound(file_path, loop))
    else:
        return

async def action_start(player, data):
    global intro
    if intro:
        print("playing intro")
        intro = False
        asyncio.create_task(play_sound("wav/intro.wav", False))
        asyncio.create_task(play_sound("wav/backbass.wav", True))
    else:
        return

async def action_stop(player, data):
    global stop_event
    stop_all_sounds()

async def action_data(player, data):
    await send_to_all(players, player.id, data)
async def action_correct(player, data):
    await send_to_all(players, player.id, data)
async def action_incorrect(player, data):
    await send_to_all(players, player.id, data)

action_handlers = {
        "play": action_play,
        "stop_all": action_stop,
        "data": action_data,
        "start": action_start,
        "correct": action_correct,
        "incorrect": action_incorrect
    }

def assign_role():
    global buttons
    if(buttons):
        buttons = False
        return True
    else:
        return False
    
async def send_to_all(players, player_id, data):
    tasks = []
    for p_id, player in players.items():
        if p_id != player_id:
            tasks.append(player.websocket.send(json.dumps(data)))
    await asyncio.gather(*tasks)

async def handle_client(websocket, path):
    player_id = str(uuid.uuid4())
    role = assign_role()
    player = Player(websocket, player_id, role)
    players[player_id] = player
    welcome_msg = {
        "action": "role",
        "id": player_id,
        "role": role
    }
    await websocket.send(json.dumps(welcome_msg))
    print(f"player {player_id} connected role:{role}")
    broadcast_msg = {
        "action": "connect-player",
        "id": player_id
    }
    await send_to_all(players, player_id, broadcast_msg)
    #handle Messages
    try:
        async for message in websocket:
            data = json.loads(message)
            action = data.get("action")
            if(action != "data"):
                print(f"msg send from {player}: {action}")
            if action in action_handlers:
                await action_handlers[action](player, data)

    except websockets.ConnectionClosed:
        print(f"Connection closed by player {player_id}")
        del players[player_id]

async def main():
    server = await websockets.serve(handle_client, "0.0.0.0", 15009)
    print("Server Running on 0.0.0.0:15009")

    await server.wait_closed()

if __name__ == "__main__":
    asyncio.run(main())
