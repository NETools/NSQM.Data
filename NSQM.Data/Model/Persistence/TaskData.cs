using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using NSQM.Data.Messages;
using TaskStatus = NSQM.Data.Extensions.TaskStatus;
using NSQM.Data.Extensions;
using System.Text.Json.Serialization;
using NSQM.Data.Converters;

namespace NSQM.Data.Model.Persistence
{
	public class TaskData
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		public int Id { get; set; }

		public Guid FromId { get; set; }
		public Guid ToId { get; set; }
		public Guid TaskId { get; set; }
		public Guid PhaseId { get; set; }
		public string TaskName { get; set; }
		public string ChannelId { get; set; }
		public TaskStatus Status { get; set; }

		[JsonConverter(typeof(JsonToByteArrayConverter))]
		public byte[] Content { get; set; }
		public UserType AddresseeType { get; set; }
		public UserType SenderType { get; set; }
	}
}
