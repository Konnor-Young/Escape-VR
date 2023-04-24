using System.Collections;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System;

public class AudioClient : MonoBehaviour
{
    private string IP = "192.168.0.105";
    private int PORT = 8080;

    [Serializable]
    public class AudioMessage
    {
        public string action;
        public string file_path;
        public bool loop;
    }

    public void PlayAudio(string filePath, bool loop = false)
    {
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
        AudioMessage message = new AudioMessage
        {
            action = "stop"
        };
        SendAudioMessage(message);
    }

    private void SendAudioMessage(AudioMessage message)
    {
        StartCoroutine(SendAudioMessageCoroutine(message));
    }

    private IEnumerator SendAudioMessageCoroutine(AudioMessage message)
    {
        using(TcpClient client = new TcpClient())
        {
            yield return StartCoroutine(ConnectToServer(client));

            string messageJson = JsonUtility.ToJson(message);
            byte[] data = Encoding.ASCII.GetBytes(messageJson);

            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);

            client.Close();
        }
    }
    private IEnumerator ConnectToServer(TcpClient client)
    {
        DateTime startTime = DateTime.Now;
        while (!client.Connected)
        {
            try
            {
                client.Connect(IP, PORT);
            }
            catch (SocketException)
            {
                if ((DateTime.Now - startTime).TotalSeconds > 5)
                {
                    Debug.LogError("Connection to audio server timed out.");
                    yield break;
                }
            }
            yield return null;
        }
    }
}