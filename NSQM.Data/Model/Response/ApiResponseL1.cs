using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace NSQM.Data.Model.Response
{
	public enum ApiResponseL1
	{
		ChannelNotFound = 1 | 0xff << 8,
		UserNotFound = 2 | 0xff << 8,
		TaskDataNotFound = 3 | 0xff << 8,
		UserCreated = 4,
		UserExists = 5 | 0xaa << 8,
		ChannelCreated = 6,
		ChannelExists = 7 | 0xaa << 8,
		TaskDataExists = 8 | 0xaa << 8,
		TaskDataCreated = 9,
		TaskDataAddedToUser = 10,
		UserAddedToChannel = 11,
		UserAlreadyAdded = 12 | 0xaa << 8,
		TaskDataAlreadyAdded = 13 | 0xaa << 8,
		TaskDataRemovedFromUser = 14,
		TaskDataRemoved = 15,
		UserRemovedFromChannel = 16,
		UserRemoved = 17,
		TaskDataNotAddedToUser = 18 | 0xcc << 8,
		UserNotAddedToChannel = 19 | 0xcc << 8,
		IllegalUserDeletion_Callback = 20 | 0xbb << 8,
		IllegalUserDeletion_NoCallback = 21 | 0xbb << 8,
		FatalInstanceError = 22 | 0xee << 8,
		FatalSqlError = 23 | 0xee << 8
	}

	public enum ApiStatusCode
	{
		Ok,
		Error = 0xff,
		Warning = 0xaa,
		Fatal = 0xee,
		RequiresInteraction = 0xbb,

	}

	public class ApiResponseL1<T> where T : class
	{
		public ApiStatusCode Status => (ApiStatusCode)((int)ResponseCode >> 8 & 0xfff);
		public ApiResponseL1 ResponseCode { get; set; }
		public T? Model { get; set; }
		public string Description { get; set; }
		public EntityState EntityState { get; set; }

		public static ApiResponseL1<T> Failed(string description, ApiResponseL1 responseCode)
		{
			return new ApiResponseL1<T>()
			{
				ResponseCode = responseCode,
				Description = description
			};
		}

		public static ApiResponseL1<T> Ok(EntityEntry<T> entry, string description, ApiResponseL1 responseCode)
		{
			return new ApiResponseL1<T>()
			{
				ResponseCode = responseCode,
				EntityState = entry.State,
				Description = description,
				Model = entry.Entity
			};
		}

		public static ApiResponseL1<T> Ok(T model, string description, ApiResponseL1 responseCode)
		{
			return new ApiResponseL1<T>()
			{
				ResponseCode = responseCode,
				EntityState = EntityState.Unchanged,
				Description = description,
				Model = model
			};
		}

		public ApiResponseL1<V> ConvertTo<V>(Func<T, V> converter) where V : class
		{
			return new ApiResponseL1<V>()
			{
				EntityState = EntityState,
				Description = Description,
				Model = converter(Model),
				ResponseCode = ResponseCode
			};
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();

			stringBuilder.AppendLine("Response code: " + ResponseCode);
			stringBuilder.AppendLine("Model: " + Model);
			stringBuilder.AppendLine("State: " + EntityState);
			stringBuilder.AppendLine("Description: " + Description);

			return stringBuilder.ToString();
		}
	}
}
