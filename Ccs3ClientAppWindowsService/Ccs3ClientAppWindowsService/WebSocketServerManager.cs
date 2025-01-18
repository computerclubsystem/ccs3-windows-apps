using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Ccs3ClientAppWindowsService;

public class WebSocketServerManager {
    private readonly WebSocketServerManagerState _state = new();

    public WebSocketServerManager() {
        _state.WebSocketStates = new();
    }

    public async Task HandleWebSocket(WebSocket webSocket) {
        WebSocketInstanceState wsState = new();
        wsState.TaskCompletionSource = new TaskCompletionSource<WebSocket>(webSocket);
        AddWebSocketsState(webSocket, wsState);

        await wsState.TaskCompletionSource.Task;
    }

    private void AddWebSocketsState(WebSocket ws, WebSocketInstanceState wsState) {
        _state.WebSocketStates.TryAdd(ws, wsState);
    }

    private class WebSocketServerManagerState {
        public ConcurrentDictionary<WebSocket, WebSocketInstanceState> WebSocketStates { get; set; }
    }

    private class WebSocketInstanceState {
        public TaskCompletionSource<WebSocket> TaskCompletionSource { get; set; }
    }
}


