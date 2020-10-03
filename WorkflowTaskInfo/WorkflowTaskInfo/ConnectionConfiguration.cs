using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkflowTaskInfo
{
	public class ConnectionConfiguration
	{
		
		
		
		public List<ConnectionConfiguration> ConnectionConfigurations { get; set; } = new List<ConnectionConfiguration>();

		
		public static ConnectionConfiguration Instance()
		{
			ConnectionConfiguration.instance = (ConnectionConfiguration.instance ?? new ConnectionConfiguration());
			ConnectionConfiguration.instance.ConnectionConfigurations = JsonConvert.DeserializeObject<List<ConnectionConfiguration>>(File.ReadAllText("config.json"));
			return ConnectionConfiguration.instance;
		}

		
		private ConnectionConfiguration()
		{
		}

		
		
		
		public string Name { get; set; }

		
		
		
		public string AppConnectionString { get; set; }

		
		
		
		public string BPMConnectionString { get; set; }

		
		private const string configName = "config.json";

		
		private static ConnectionConfiguration instance;
	}
}
