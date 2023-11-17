using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSQM.Data.Model
{
	public class ResponseSemaphore<T> : SemaphoreSlim
	{
		public T? Response { get; set; }
		public ResponseSemaphore() : base(0, 1) { }
	}
}
