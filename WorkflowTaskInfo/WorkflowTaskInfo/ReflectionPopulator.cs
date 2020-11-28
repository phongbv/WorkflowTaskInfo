using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WorkflowTaskInfo
{
	public static class ReflectionPopulator
	{
		// Token: 0x06000014 RID: 20 RVA: 0x00002800 File Offset: 0x00000A00
		public static List<T> SqlQuery<T>(this OracleConnection conn, string sql, params OracleParameter[] parameters)
		{
			List<T> result;
			using (OracleCommand cmd = conn.CreateCommand())
			{
				bool flag = conn.State != ConnectionState.Open;
				if (flag)
				{
					conn.Open();
				}
				cmd.CommandText = sql;
				cmd.Parameters.AddRange(parameters);
				cmd.BindByName = true;
				result = CreateList<T>(cmd.ExecuteReader());
			}
			return result;
		}

		// Token: 0x06000015 RID: 21 RVA: 0x00002878 File Offset: 0x00000A78
		private static List<T> CreateList<T>(OracleDataReader reader)
		{
			List<T> results = new List<T>();
			while (reader.Read())
			{
				bool flag = IsPrimitiveType(typeof(T)) || typeof(T) == typeof(string);
				T item;
				if (flag)
				{
					Type convertTo = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
					item = (T)((object)Convert.ChangeType(reader[0], convertTo));
				}
				else
				{
					item = ((typeof(T) == typeof(string)) ? ((T)((object)Activator.CreateInstance(typeof(string), new object[]
					{
						"".ToCharArray()
					}))) : Activator.CreateInstance<T>());
					foreach (PropertyInfo property in typeof(T).GetProperties())
					{
						bool flag2 = !reader.IsDBNull(reader.GetOrdinal(property.Name.ToUpper()));
						if (flag2)
						{
							Type convertTo2 = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
							property.SetValue(item, Convert.ChangeType(reader[property.Name], convertTo2), null);
						}
					}
				}
				results.Add(item);
			}
			return results;
		}

		// Token: 0x06000016 RID: 22 RVA: 0x000029F4 File Offset: 0x00000BF4
		private static bool IsPrimitiveType(Type t)
		{
			return t.Namespace.Equals(typeof(decimal).Namespace);
		}
	}
}
