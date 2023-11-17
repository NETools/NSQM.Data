using NSQM.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NSQM.Data.Messages
{
	public struct NSQMAck
	{
		[JsonInclude]
		public Guid FromId;
		[JsonInclude]
		public Guid ToId;
		[JsonInclude]
		public Guid TaskId;
		[JsonInclude]
		public string ChannelId;
		[JsonInclude]
		public AckType AckType;
		[JsonInclude]
		public UserType UserType;

		public static NSQMessage Build(Guid senderId, Guid fromId, Guid toId, Guid taskId, string channelId, AckType ackType, UserType userType)
		{
			return new NSQMessage()
			{
				Type = MessageType.Ack,
				SenderId = senderId,
				StructBuffer = new NSQMAck()
				{
					AckType = ackType,
					FromId = fromId,
					ToId = toId,
					TaskId = taskId,
					ChannelId = channelId,
					UserType = userType

				}.ToJsonBytes(Encoding.UTF8)
			};
		}
	}
}
