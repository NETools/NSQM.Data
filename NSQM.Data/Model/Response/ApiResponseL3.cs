using Microsoft.EntityFrameworkCore;
using NSQM.Data.Model.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NSQM.Data.Model.Response
{
	public class ApiResponseL3<T> where T : class
	{
		public HttpStatusCode Status { get; set; }
		public T? Model { get; set; }
		public string ApiCode { get; set; }
		public string? Description { get; set; }

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();

			stringBuilder.AppendLine("Status: " + Status);
			stringBuilder.AppendLine("Model: " + Model);
			stringBuilder.AppendLine("ApiCode: " + ApiCode);
			stringBuilder.AppendLine("Description: " + Description);

			return stringBuilder.ToString();
		}

		public static ApiResponseL3<T>? Failed(string description, ApiResponseL1 apiResponse)
		{
			return JsonSerializer.Deserialize<ApiResponseL3<T>>(
					new ApiResponseL2<T>(
						ApiResponseL1<T>.Failed(
							description,
							apiResponse)).ApiResponseLayer3);
		}
	}
}
