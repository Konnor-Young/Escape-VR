using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MakeHTTP : MonoBehaviour
{
    private readonly string ip = "192.168.0.105";
    private readonly int port = 8080;
    public string path = "/data";

    public void Request()
    {
        string url = "http://" + ip + ":" + port.ToString() + path;
        UnityWebRequest request = UnityWebRequest.Get(url);
        StartCoroutine(SendGET(request));
    }
    IEnumerator SendGET(UnityWebRequest request)
    {
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("HTTP GET request failed: " + request.error);
            yield break;
        }
        string responseText = request.downloadHandler.text;
        Debug.Log("HTTP GET request successful. Response: " + responseText);
    }
}