using NSQM.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NSQM.Data.Messages
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct NSQMSubscribeMessage
	{
		[JsonInclude]
		public string ChannelId;
		[JsonInclude]
		public UserType UserType;

		public static NSQMessage Build(Guid senderId, string channelId, UserType userType, Encoding encoding)
		{
			return new NSQMessage()
			{
				SenderId = senderId,
				Type = MessageType.Subscribe,
				StructBuffer = new NSQMSubscribeMessage()
				{
					ChannelId = channelId,
					UserType = userType,
				}.ToJsonBytes(encoding)
			};
		}
	}
}
