using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class WSclient : MonoBehaviour
{
    private ClientWebSocket _webSocket;
    private CancellationTokenSource _cancellationTokenSource;

    public delegate void OnMessageReceived(string message);
    public event OnMessageReceived MessageReceived;
    
    public async void Connect(string uri)
    {
        _webSocket = new ClientWebSocket();
        _cancellationTokenSource = new CancellationTokenSource();
        try
        {
            await _webSocket.ConnectAsync(new Uri(uri), _cancellationTokenSource.Token);
            Debug.Log("WSLog Connected to the WebSocket Server.");
            if (_webSocket.State == WebSocketState.Open)
            {
                _ = ReceiveMessages();
            }
            else
            {
                Debug.LogWarning($"WSLog WebSocket connection failed. State: {_webSocket.State}");
            }
        }
        catch (Exception e)
        {
            Debug.Log($"WSLog Error Connecting to the WebSocket: {e.Message}");
        }
    }
    public async Task ReceiveMessages()
    {
        Debug.Log($"WSLog WebSocket state: {_webSocket.State}");
        while (_webSocket.State == WebSocketState.Open)
        {
            var buffer = new ArraySegment<byte>(new byte[1024]);
            WebSocketReceiveResult result;

            try
            {
                result = await _webSocket.ReceiveAsync(buffer, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("WSLog Receiving messages canceled.");
                break;
            }

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server closed connection.", _cancellationTokenSource.Token);
                Debug.Log("WSLog Server closed the connection.");
                break;
            }

            var message = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
            Debug.Log($"WSLog Received message from server: {message}");

            // Trigger the event with the received message
            MessageReceived?.Invoke(message);
        }
    }
    public async Task SendWSMessage(string message)
    {
        if (_webSocket == null || _webSocket.State != WebSocketState.Open)
        {
            Debug.LogError("WSLog WebSocket is not connected.");
            return;
        }

        var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
        try
        {
            await _webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
        }
        catch (Exception e)
        {
            Debug.LogError($"WSLog Error sending message: {e.Message}");
        }
    }

}
