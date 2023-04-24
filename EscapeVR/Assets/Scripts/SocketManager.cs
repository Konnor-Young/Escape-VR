using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class SocketManager : MonoBehaviour
{
    public GameObject playerPrefab;
    [SerializeField]
    public List<GameObject> hands;
    public List<Camera> Cameras;

    private float sendInterval = .1f;
    private string IP = "192.168.0.84";
    private int PORT = 8080;
    private Dictionary<string, GameObject> otherPlayers = new Dictionary<string, GameObject>();
    private string playerId;
    private TcpClient ws;
    private NetworkStream stream;
    private Thread dataReceiverThread;

    private bool buttons = false;

    void Start()
    {
        ConnectToSocketServer();
    }
    private void ConnectToSocketServer()
    {
        try
        {
            ws = new TcpClient(IP, PORT);
            stream = ws.GetStream();
            dataReceiverThread = new Thread(new ThreadStart(ReceiveData));
            dataReceiverThread.IsBackground = true;
            dataReceiverThread.Start();
            InvokeRepeating("SendPlayerData", 0f, sendInterval);
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }
    public void SendMessageToServer(string message)
    {
        if (stream == null) return;

        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }
    private void ReceiveData()
    {
        while (ws != null && ws.Connected)
        {
            try
            {
                byte[] data = new byte[1024];
                int dataSize = stream.Read(data, 0, data.Length);
                if (dataSize > 0)
                {
                    string message = Encoding.UTF8.GetString(data, 0, dataSize);
                    ProcessReceivedData(message);
                }
            }
            catch (SocketException socketException)
            {
                Debug.Log("Socket exception: " + socketException);
            }
        }
    }
        private void ProcessReceivedData(string message)
    {
        Player playerData = JsonUtility.FromJson<Player>(message);
        string playerID = playerData.id;

        switch (playerData.id)
        {
            case "PlayerData":
                GameObject playerObj;
                if (!otherPlayers.ContainsKey(playerID))
                {
                    playerObj = Instantiate(playerPrefab);
                    otherPlayers.Add(playerID, playerObj);
                }
                else
                {
                    playerObj = otherPlayers[playerID];
                }

                PlayerVR player = playerObj.GetComponent<PlayerVR>();
                player.UpdateTransforms(playerData);
                break;
            case "Role":
                playerId = playerData.id;
                UpdateCameraCullingMasks();
                break;
        }
    }
    public void PlayAudio(string filePath, bool loop = false)
    {
        if (!buttons) return;
        AudioMessage message = new AudioMessage
        {
            action = "play",
            file_path = filePath,
            loop = loop,
        };
        SendAudioMessage(message);
    }
    public void StopAudio()
    {
        if (!buttons) return;
        AudioMessage message = new AudioMessage
        {
            action = "stop"
        };
        SendAudioMessage(message);
    }
    private void SendAudioMessage(AudioMessage message)
    {
        if (stream == null) return;

        try
        {
            string messageJson = JsonUtility.ToJson(message);
            byte[] data = Encoding.ASCII.GetBytes(messageJson);
            stream.Write(data, 0, data.Length);
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }
    private void OnDestroy()
    {
        if (ws != null)
        {
            stream.Close();
            ws.Close();
        }
        if (dataReceiverThread != null && dataReceiverThread.IsAlive)
        {
            dataReceiverThread.Abort();
        }
    }
    private void UpdateCameraCullingMasks()
    {
        foreach (Camera camera in Cameras)
        {
            int cullingMask = camera.cullingMask;

            if (buttons)
            {
                cullingMask |= (1 << LayerMask.NameToLayer("MasterLayer"));
                cullingMask &= ~(1 << LayerMask.NameToLayer("NonMasterLayer"));
            }
            else
            {
                cullingMask |= (1 << LayerMask.NameToLayer("NonMasterLayer"));
                cullingMask &= ~(1 << LayerMask.NameToLayer("MasterLayer"));
            }

            camera.cullingMask = cullingMask;
        }
    }

    private void SendPlayerData()
    {
        Vector3 headPosition = Cameras[0].transform.position;
        Quaternion headRotation = Cameras[0].transform.rotation;
        Vector3 lHandPosition = hands[0].transform.position;
        Quaternion lHandRotation = hands[0].transform.rotation;
        Vector3 rHandPosition = hands[1].transform.position;
        Quaternion rHandRotation = hands[1].transform.rotation;
        string jsonMessage = JsonUtility.ToJson(headRotation);
        byte[] data = Encoding.UTF8.GetBytes(jsonMessage);
        stream.Write(data, 0, data.Length);
    }
}
