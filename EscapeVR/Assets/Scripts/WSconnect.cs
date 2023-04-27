using Meta.WitAi.Json;
using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class WSconnect : MonoBehaviour
{
    public GameObject head;
    public GameObject lHand;
    public GameObject rHand;
    public GameObject player2;
    public Player player2pos;
    public AudioTrigger correctSound;
    public AudioTrigger incorrectSound;
    [SerializeField]
    public List<Camera> Cameras;
    public WSclient ws;

    private string IP = "Your IP Address";
    private int PORT = 15009;

    private Dictionary<string, Player> players = new Dictionary<string, Player>();
    private string player_id;
    private bool role = false;

    void Start()
    {
        ws.Connect($"ws://{IP}:{PORT}");
        Debug.Log($"WSLog connected to {IP}");
        ws.MessageReceived += HandleMessage;
    }
    private void HandleMessage(string message)
    {
        ServerMessage serverMessage = JsonConvert.DeserializeObject<ServerMessage>(message);
        Debug.Log($"WSLog server message: {serverMessage}");
        switch (serverMessage.action)
        {
            case "role":
                player_id = serverMessage.id;
                role = serverMessage.role;
                Debug.Log($"WSLog Player ID: {player_id}, Role: {role}");
                UpdateCameraCullingMasks();
                StartCoroutine(SendPositionData());
                break;
            case "connect-player":
                string newPlayerId = serverMessage.id;
                players.Add(newPlayerId, player2pos);
                PlayStart();
                Debug.Log($"WSLog New player connected with ID: {newPlayerId}");
                break;
            case "data":
                string playerId = serverMessage.id;
                UpdatePlayerPosition(serverMessage.lHand, serverMessage.rHand, serverMessage.head);
                break;
            case "correct":
                correctSound.PlayAudio();
                break;
            case "incorrect":
                incorrectSound.PlayAudio();
                break;
            default:
                Debug.Log("WSLog Unknown Action");
                break;
        }
    }
    public IEnumerator SendPositionData()
    {
        while (player_id == null)
        {
            yield return null;
        }

        WaitForSeconds waitForInterval = new WaitForSeconds(0.1f);

        while (true)
        {
            Dictionary<string, float> lHandData = GetLeftData();
            Dictionary<string, float> rHandData = GetRightData();
            Dictionary<string, float> headData = GetHeadData();

            var positionData = new
            {
                action = "data",
                id = player_id,
                lHand = lHandData,
                rHand = rHandData,
                head = headData
            };

            string message = JsonConvert.SerializeObject(positionData);
            yield return SendWSMessageAndWait(message);

            yield return waitForInterval;
        }
    }
    private IEnumerator SendWSMessageAndWait(string message)
    {
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        ws.SendWSMessage(message).ContinueWith(_ => tcs.SetResult(true));
        yield return new WaitUntil(() => tcs.Task.IsCompleted);
    }
    private void UpdatePlayerPosition(Dictionary<string, float> lHandData, Dictionary<string, float> rHandData, Dictionary<string, float> headData)
    {
            player2pos.UpdatePosition(lHandData, rHandData, headData);
    }
    private void UpdateCameraCullingMasks()
    {
        foreach (Camera camera in Cameras)
        {
            int cullingMask = camera.cullingMask;

            if (role)
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
    public void PlayAudio(string filePath, bool loop = false)
    {
        AudioMessage message = new AudioMessage
        {
            action = "play",
            file_path = filePath,
            loop = loop,
            role = role
        };
        SendAudioMessage(message);
    }
    public void PlayStart()
    {
        AudioMessage message = new AudioMessage
        {
            action = "start",
        };
        SendAudioMessage(message);
    }
    public void StopAudio()
    {
        AudioMessage message = new AudioMessage
        {
            action = "stop"
        };
        SendAudioMessage(message);
    }
    public void PlayCorrect()
    {
        AudioMessage message = new AudioMessage
        {
            action = "correct"
        };
        SendAudioMessage(message);
    }
    public void PlayIncorrect()
    {
        AudioMessage message = new AudioMessage
        {
            action = "incorrect"
        };
        SendAudioMessage(message);
    }
    private async void SendAudioMessage(AudioMessage message)
    {
        string messageJson = JsonUtility.ToJson(message);
        await ws.SendWSMessage(messageJson);
    }

    [System.Serializable]
    private class ServerMessage
    {
        public string action;
        public bool role;
        public string id;
        public Dictionary<string, float> lHand = new Dictionary<string, float>();
        public Dictionary<string, float> rHand = new Dictionary<string, float>();
        public Dictionary<string, float> head = new Dictionary<string, float>();
        public ServerMessage()
        {
            lHand = new Dictionary<string, float>
            {
                {"posX", 0f},
                {"posY", 0f},
                {"posZ", 0f},
                {"rotX", 0f},
                {"rotY", 0f},
                {"rotZ", 0f},
                {"rotW", 0f}
            };
            rHand = new Dictionary<string, float>{
                {"posX", 0f},
                {"posY", 0f},
                {"posZ", 0f},
                {"rotX", 0f},
                {"rotY", 0f},
                {"rotZ", 0f},
                {"rotW", 0f}
            };
            head = new Dictionary<string, float>{
                {"posX", 0f},
                {"posY", 0f},
                {"posZ", 0f},
                {"rotX", 0f},
                {"rotY", 0f},
                {"rotZ", 0f},
                {"rotW", 0f}
            };
        }
    }
    public Dictionary<string, float> GetHeadData()
    {
        return new Dictionary<string, float>
        {
            {"posX", head.transform.position.x},
            {"posY", head.transform.position.y},
            {"posZ", head.transform.position.z},
            {"rotX", head.transform.rotation.x},
            {"rotY", head.transform.rotation.y},
            {"rotZ", head.transform.rotation.z},
            {"rotW", head.transform.rotation.w}
        };
    }
    public Dictionary<string, float> GetLeftData()
    {
        return new Dictionary<string, float>
        {
            {"posX", lHand.transform.position.x},
            {"posY", lHand.transform.position.y},
            {"posZ", lHand.transform.position.z},
            {"rotX", lHand.transform.rotation.x},
            {"rotY", lHand.transform.rotation.y},
            {"rotZ", lHand.transform.rotation.z},
            {"rotW", lHand.transform.rotation.w}
        };
    }
    public Dictionary<string, float> GetRightData()
    {
        return new Dictionary<string, float>
        {
            {"posX", rHand.transform.position.x},
            {"posY", rHand.transform.position.y},
            {"posZ", rHand.transform.position.z},
            {"rotX", rHand.transform.rotation.x},
            {"rotY", rHand.transform.rotation.y},
            {"rotZ", rHand.transform.rotation.z},
            {"rotW", rHand.transform.rotation.w}
        };
    }
}