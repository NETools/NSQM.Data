using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using NSQM.Data.Extensions;

namespace NSQM.Data.Model.Persistence
{
	public class User
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		public Guid UserId { get; set; }
		public UserType UserType { get; set; }
		public virtual IList<TaskData>? TaskDatas { get; set; } = new List<TaskData>();
	}
}
