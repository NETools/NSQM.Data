using NSQM.Data.Extensions;
using NSQM.Data.Messages;
using NSQM.Data.Model;
using NSQM.Data.Model.Response;
using System;
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

		public NSQMWebSocket(WebSocket webSocket)
		{
			Id = Guid.NewGuid();
			_webSocket = webSocket;
		}

		public async Task Start()
		{
			try
			{
				var lengthBuffer = new byte[4];
				var receiveResult = await _webSocket.ReceiveAsync(lengthBuffer, CancellationToken.None);

				int messageLength = BitConverter.ToInt32(lengthBuffer);

				while (!receiveResult.CloseStatus.HasValue)
				{
					var messageBuffer = new byte[messageLength];
					receiveResult = await _webSocket.ReceiveAsync(messageBuffer, CancellationToken.None);

					if (receiveResult.CloseStatus.HasValue)
					{
						return;
					}

					var message = messageBuffer.ToStruct<NSQMessage>(Encoding.UTF8);
					await ProcessMessage(message);

					lengthBuffer = new byte[4];
					receiveResult = await _webSocket.ReceiveAsync(lengthBuffer, CancellationToken.None);

					messageLength = BitConverter.ToInt32(lengthBuffer);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			finally
			{
				await Close();
			}
		}

		public async Task Send(NSQMessage message)
		{
			var messageBuffer = message.ToJsonBytes(Encoding.UTF8);
			var lengthBuffer = BitConverter.GetBytes(messageBuffer.Length);

			await _webSocket.SendAsync(lengthBuffer, WebSocketMessageType.Binary, false, CancellationToken.None);
			await _webSocket.SendAsync(messageBuffer, WebSocketMessageType.Binary, false, CancellationToken.None);
		}

		public async Task<ApiResponseL3<T>?> SendAndReceive<T>(NSQMessage message, CancellationToken cancellationToken) 
			where T : class
		{
			message.ConnectionId = Guid.NewGuid();
			await Send(message);
			using var responseSemaphore = new ResponseSemaphore<NSQMInfoMessage>();
			if (!_responseSemaphores.TryAdd(message.ConnectionId, responseSemaphore))
				return null;
			await responseSemaphore.WaitAsync(cancellationToken);
			_responseSemaphores.TryRemove(message.ConnectionId, out _);
			var apiResponse = JsonSerializer.Deserialize<ApiResponseL3<T>>(responseSemaphore.Response.Information);
			return apiResponse;
		}

		public async Task Close()
		{
			if (_webSocket.State != WebSocketState.Aborted)
				await _webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
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
		}
	}
}