import socket
import json
import subprocess
import threading
import psutil

HOST = '0.0.0.0'
PORT = 8080

players = {}
player_counter = 0

def play_mp3(file_path, loop):
    if file_path:
        command = ['mpg123', '--loop', '-1' if loop else '1', file_path]
        subprocess.Popen(command, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)

def stop_all_songs():
    for process in psutil.process_iter():
        try:
            process_info = process.as_dict(attrs=['pid', 'name'])
            if process_info['name'] == 'mpg123':
                process.terminate()
        except (psutil.NoSuchProcess, psutil.AccessDenied, psutil.ZombieProcess):
            pass

def broadcast_player_data():
    for conn, player_data in players.items():
        for other_conn in players.keys():
            if conn != other_conn:
                other_conn.sendall(json.dumps(player_data).encode())

def handle_client(conn):
    global players, player_counter
    try:
        playerID = f"player_{player_counter}"
        player_counter += 1
        players[conn] = {'playerID': playerID}

        # Send playerID to the client
        id_message = {
            "type": "PlayerID",
            "playerID": playerID
        }
        conn.sendall(json.dumps(id_message).encode())

        while True:
            data = conn.recv(1024)
            if not data:
                break

            message = json.loads(data.decode())
            action = message.get('type')

            file_path = message.get('file_path')
            loop = message.get('loop')

            if action == 'PlayerData':
                players[conn] = message
                broadcast_player_data()
            elif action == 'play':
                thread = threading.Thread(target=play_mp3, args=(file_path, loop,))
                thread.start()
            elif action == 'stop':
                stop_all_songs()

    except Exception as e:
        print(e)
    finally:
        del players[conn]
        conn.close()

def assign_roles():
    global players
    player_count = len(players)

    if player_count >= 2:
        roles = [True, False]
        for i, (conn, _) in enumerate(players.items()):
            role = roles[i % 2]
            message = {
                "type": "Role",
                "buttons": role
            }
            conn.sendall(json.dumps(message).encode())


def main():
    global players

    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.bind((HOST, PORT))
        s.listen()
        print(f"Server listening on {HOST}:{PORT}")

        while True:
            conn, addr = s.accept()
            print(f"Client connected from {addr}")
            threading.Thread(target=handle_client, args=(conn,)).start()
            players[conn] = {}
            assign_roles()

if __name__ == '__main__':
    main()
