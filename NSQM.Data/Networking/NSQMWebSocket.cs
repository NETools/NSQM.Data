using NSQM.Data.Extensions;
using NSQM.Data.Messages;
using NSQM.Data.Model;
using NSQM.Data.Model.Response;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NSQM.Data.Networking
{
	public abstract class NSQMWebSocket : IDisposable
	{
		private WebSocket _webSocket;
		private ConcurrentDictionary<Guid, ResponseSemaphore<NSQMInfoMessage>> _responseSemaphores = new();
		public Guid Id { get; private set; }

		public event Action<Guid>? Disconnected;

		public Action<string, LoggingType> Logger { get; set; } = (message, logginType) =>
		{
			switch (logginType)
			{
				case LoggingType.Info:
					Console.ForegroundColor = ConsoleColor.Cyan;
					break;
				case LoggingType.Error:
					Console.ForegroundColor = ConsoleColor.Red;
					break;
				case LoggingType.Warning:
					Console.ForegroundColor = ConsoleColor.Yellow;
					break;
			}
			Console.WriteLine(message);
			Console.ForegroundColor = ConsoleColor.Gray;
		};

		public NSQMWebSocket(WebSocket webSocket)
		{
			Id = Guid.NewGuid();
			_webSocket = webSocket;
		}

		public async Task Start()
		{
			var frameBuffer = new byte[8192];
			try
			{
				while (true)
				{
					try
					{
						var receiveResult = await _webSocket.ReceiveAsync(frameBuffer.AsMemory(), CancellationToken.None);
						int totalSize = receiveResult.Count;
						while (!receiveResult.EndOfMessage && !_webSocket.CloseStatus.HasValue)
						{
							Array.Resize(ref frameBuffer, frameBuffer.Length * 2);
							receiveResult = await _webSocket.ReceiveAsync(frameBuffer.AsMemory(totalSize..), CancellationToken.None);
							totalSize += receiveResult.Count;
							Logger?.Invoke("FrameBuffer length doubled", LoggingType.Info);
						}

						if (_webSocket.CloseStatus.HasValue)
						{
							Logger?.Invoke("Websocket has a close status", LoggingType.Warning);
							return;
						}

						var message = new ReadOnlySpan<byte>(frameBuffer, 0, totalSize).ToStruct<NSQMessage>(Encoding.UTF8);
						await ProcessMessage(message);
					}
					catch (Exception ex)
					{
						Logger?.Invoke($"Error occured on loop level: {ex.Message}", LoggingType.Error);
						if (_webSocket.State == WebSocketState.Closed || _webSocket.State == WebSocketState.Aborted)
							break;
					}
				}
			}
			catch (Exception ex)
			{
				Logger?.Invoke($"Error occured on method level: {ex.Message}", LoggingType.Error);
			}
			finally
			{
				Logger?.Invoke("Websocket closed", LoggingType.Info);
				await Close();
			}
		}

		public async Task Send(NSQMessage message)
		{
			var messageBuffer = message.ToJsonBytes(Encoding.UTF8);
			await _webSocket.SendAsync(messageBuffer, WebSocketMessageType.Binary, true, CancellationToken.None);
		}

		public async Task<ApiResponseL3<T>?> SendAndReceive<T>(NSQMessage message, CancellationToken cancellationToken) 
			where T : class
		{
			message.ConnectionId = Guid.NewGuid();
			await Send(message);
			using var responseSemaphore = new ResponseSemaphore<NSQMInfoMessage>();
			if (!_responseSemaphores.TryAdd(message.ConnectionId, responseSemaphore))
				return null;
			Logger?.Invoke($"Waiting for response.", LoggingType.Info);
			await responseSemaphore.WaitAsync(cancellationToken);
			Logger?.Invoke($"Response received..", LoggingType.Info);
			_responseSemaphores.TryRemove(message.ConnectionId, out _);
			var apiResponse = JsonSerializer.Deserialize<ApiResponseL3<T>>(responseSemaphore.Response.Information);
			return apiResponse;
		}

		public async Task Close()
		{
			if (_webSocket.State != WebSocketState.Closed && _webSocket.State != WebSocketState.Aborted)
			{
				Logger?.Invoke($"Closing websocket output.", LoggingType.Info);
				await _webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
			}
			Disconnected?.Invoke(Id);
			Dispose();
		}

		protected abstract Task HandleMessage(NSQMessage message);

		private async Task ProcessMessage(NSQMessage message)
		{
			switch (message.Type)
			{
				case MessageType.Info:
					var nsqmInfoMessage = message.StructBuffer.ToStruct<NSQMInfoMessage>(Encoding.UTF8);
					if(_responseSemaphores.TryGetValue(nsqmInfoMessage.ForConnectionId, out var responseSemaphore))
					{
						responseSemaphore.Response = nsqmInfoMessage;
						responseSemaphore.Release();
						return;
					}
					break;
			}

			await HandleMessage(message);
		}

		public void Dispose()
		{
			_webSocket?.Dispose();
			foreach (var semaphore in _responseSemaphores)
			{
				semaphore.Value.Release();
			}

			Logger?.Invoke($"NSQMWebSocket disposed.", LoggingType.Info);
		}
	}
}