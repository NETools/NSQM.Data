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
	public struct NSQMInfoMessage
	{
		[JsonInclude]
		public string Information;
		[JsonInclude]
		public Guid ForConnectionId;
		public static NSQMessage Build(Guid senderId, string information, Guid forConnectionId, Encoding encoding)
		{
			return new NSQMessage()
			{
				SenderId = senderId,
				Type = MessageType.Info,
				StructBuffer = new NSQMInfoMessage()
				{
					Information = information,
					ForConnectionId = forConnectionId
				}.ToJsonBytes(encoding)
			};
		}
	}
}
