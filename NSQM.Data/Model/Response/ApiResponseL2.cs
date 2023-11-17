using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace NSQM.Data.Model.Response
{
	public class ApiResponseL2<T> where T : class
	{
		public HttpStatusCode StatusCode { get; private set; }
		public string ApiResponseLayer3 { get; private set; }

		public ApiResponseL2(ApiResponseL1<T> apiResponse)
		{
			ApiResponseLayer3 = "{}";
			TranslateL1(apiResponse);
		}

		private void TranslateL1(ApiResponseL1<T> apiResponse)
		{
			switch (apiResponse.Status)
			{
				case ApiStatusCode.Error:
					StatusCode = HttpStatusCode.InternalServerError;
					break;
				case ApiStatusCode.RequiresInteraction:
					StatusCode = HttpStatusCode.Conflict;
					break;
				case ApiStatusCode.Warning:
					StatusCode = HttpStatusCode.BadRequest;
					break;
				case ApiStatusCode.Ok:
					StatusCode = HttpStatusCode.OK;
					break;
			}

			ApiResponseLayer3 = JsonSerializer.Serialize(new ApiResponseL3<T>()
			{
				Status = StatusCode,
				Model = apiResponse.Model,
				Description = apiResponse.Description,
				ApiCode = $"{apiResponse.ResponseCode} ({(int)apiResponse.ResponseCode})"
			});
		}

		public IActionResult Accept()
		{
			switch (StatusCode)
			{
				case HttpStatusCode.BadRequest:
				case HttpStatusCode.InternalServerError:
				default:
					return new BadRequestObjectResult(ApiResponseLayer3);
				case HttpStatusCode.Conflict:
					return new ConflictObjectResult(ApiResponseLayer3);
				case HttpStatusCode.OK:
					return new OkObjectResult(ApiResponseLayer3);
			}
		}

		public override string ToString()
		{
			return ApiResponseLayer3;
		}

	}
}
