using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSQM.Data.Extensions
{
	public enum TaskStatus
	{
		TaskStarted,
		TaskDone,
		TaskFailed,
	}

	public enum UserType
	{
		Consumer,
		Producer,
		All
	}

	public enum AckType
	{
		Proposal,
		Accepted,
		Rejected
	}
}
