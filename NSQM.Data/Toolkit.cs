using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NSQM.Data
{
	internal static class Toolkit
	{
		public static IEnumerable<FieldInfo> GetFieldsWithAnnotation<T, V>() where T : struct where V : Attribute
		{
			var typeInfo = typeof(T);
			var fields = typeInfo.GetFields(BindingFlags.Instance | BindingFlags.Public);
			foreach (var field in fields)
			{
				var attributes = field.GetCustomAttribute<V>(true);
				if (attributes != null)
				{
					yield return field;
				}
			}
		}
	}
}
