using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NSQM.Data.Model.Persistence
{
	public class Channel
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		public string ChannelId { get; set; }
		public virtual IList<User>? Users { get; set; } = new List<User>();
	}
}
