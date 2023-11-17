using NSQM.Data.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NSQM.Data.Extensions
{
	public static class NSQMessageExtension
	{
		public static byte[] ToJsonBytes<T>(this T message, Encoding encoding) where T : struct
		{
			var json = JsonSerializer.Serialize(message);
			return encoding.GetBytes(json);
		}

		public static T ToStruct<T>(this byte[] buffer, Encoding encoding) where T : struct
		{
			return JsonSerializer.Deserialize<T>(buffer);
		}

		public static V Map<T, V>(this T sourceStruct) where T : struct where V : class
		{
			var model = (V)Activator.CreateInstance(typeof(V));
			var modelType = model.GetType();

			var fields = Toolkit.GetFieldsWithAnnotation<T, JsonIncludeAttribute>();
			foreach (var field in fields)
			{
				var fField = modelType.GetProperty(field.Name);
				if (fField == null) continue;
				fField.SetValue(model, field.GetValue(sourceStruct));
			}
			return model;

		}
	}
}
