using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeConfigReader : IConfigReader
	{
		private readonly IDictionary<string, string> _settings = new Dictionary<string, string>();
		private readonly IDictionary<string, Func<string>> _connectionStrings = new Dictionary<string, Func<string>>();

		public FakeConfigReader(IEnumerable<KeyValuePair<string, string>> config)
		{
			config.ForEach(x => _settings[x.Key] = x.Value);
		}

		public FakeConfigReader()
		{
		}

		public FakeConfigReader(string name, string value)
		{
			FakeSetting(name, value);
		}

		public void FakeSetting(string name, string value)
		{
			_settings[name] = value;
		}

		public void FakeConnectionString(string name, string connectionString)
		{
			_connectionStrings[name] = () => connectionString;
		}

		public void FakeConnectionString(string name, Func<string> connectionString)
		{
			_connectionStrings[name] = connectionString;
		}
		
		public string AppConfig(string name)
		{
			return _settings.ContainsKey(name) ? _settings[name] : null;
		}

		public string ConnectionString(string name)
		{
			return _connectionStrings.ContainsKey(name) ? _connectionStrings[name].Invoke() : null;
		}
	}
	
	public static class FakeConfigReaderExtensions
	{
		public static FakeConfigReader FakeInfraTestConfig(this FakeConfigReader config)
		{
			config.FakeConnectionString("MessageBroker", InfraTestConfigReader.AnalyticsConnectionString);
			config.FakeConnectionString("Tenancy", InfraTestConfigReader.ApplicationConnectionString);
			config.FakeConnectionString("Toggle", InfraTestConfigReader.ApplicationConnectionString);
			config.FakeConnectionString("Hangfire", InfraTestConfigReader.AnalyticsConnectionString);
			config.FakeConnectionString("RtaTracer", InfraTestConfigReader.AnalyticsConnectionString);
			return config;
		}

		public static FakeConfigReader AddConnectionString(this FakeConfigReader configReader, string name, string connectionString)
		{
			configReader.FakeConnectionString("Toggle", null as string);
			return configReader;
		}				
	}
}