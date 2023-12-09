using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NSQM.Data.Messages
{
	public enum MessageType
	{
		Subscribe,
		Task,
		TaskStream,
		Info,
		Ack
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct NSQMessage
	{
		[JsonInclude]
		public Guid SenderId;
		[JsonInclude]
		public Guid ConnectionId;
		[JsonInclude]
		public MessageType Type;
		[JsonInclude]
		public byte[] StructBuffer;
	}
}
