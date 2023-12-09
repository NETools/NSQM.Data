using NSQM.Data.Extensions;
using NSQM.Data.Model.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TaskStatus = NSQM.Data.Extensions.TaskStatus;

namespace NSQM.Data.Messages
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct NSQMTaskMessage
	{
		[JsonInclude]
		public Guid FromId;
		[JsonInclude]
		public Guid ToId;
		[JsonInclude]
		public Guid TaskId;
		[JsonInclude]
		public Guid PhaseId;
		[JsonInclude]
		public string TaskName;
		[JsonInclude]
		public string ChannelId;
		[JsonInclude]
		public TaskStatus Status;
		[JsonInclude]
		public byte[] Content;
		[JsonInclude]
		public UserType AddresseeType;
		[JsonInclude]
		public UserType SenderType;

		public static NSQMessage Build(Guid senderId, Guid fromId, Guid toId, string taskName, Guid taskId, string channelId, TaskStatus status, byte[] content, UserType addresseType, UserType senderType, Encoding encoding, bool isStreamed)
		{
			return new NSQMessage()
			{
				SenderId = senderId,
				Type = isStreamed ? MessageType.TaskStream : MessageType.Task,
				StructBuffer = new NSQMTaskMessage()
				{
					PhaseId = Guid.NewGuid(),
					FromId = fromId,
					ToId = toId,
					TaskId = taskId,
					TaskName = taskName,
					ChannelId = channelId,
					Content = content,
					Status = status,
					AddresseeType = addresseType,
					SenderType = senderType
				}.ToJsonBytes(encoding)
			};
		}
	}
}
