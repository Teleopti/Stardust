using System;
using System.Collections.Generic;
using System.Configuration;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeConfigReader : IConfigReader
	{
		private readonly IDictionary<string, Func<string>> _settings = new Dictionary<string, Func<string>>();
		private readonly IDictionary<string, Func<string>> _connectionStrings = new Dictionary<string, Func<string>>();

		public FakeConfigReader(IEnumerable<KeyValuePair<string, string>> config)
		{
			config.ForEach(x => _settings[x.Key] = () => x.Value);
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
			_settings[name] = () => value;
		}
		
		public void FakeSetting(string name, Func<string> value)
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
			return _settings.ContainsKey(name) ? _settings[name].Invoke() : null;
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
			config.FakeConnectionString("Status", InfraTestConfigReader.ApplicationConnectionString);
			config.FakeConnectionString("Toggle", InfraTestConfigReader.ApplicationConnectionString);
			config.FakeConnectionString("Hangfire", InfraTestConfigReader.AnalyticsConnectionString);
			config.FakeConnectionString("RtaTracer", InfraTestConfigReader.AnalyticsConnectionString);
			
			//this connstring is a app setting for some strange reason. not easy to change...
			config.FakeSetting("DatamartConnectionString", InfraTestConfigReader.AnalyticsConnectionString);
			config.FakeSetting("CertificateModulus", ConfigurationManager.AppSettings["CertificateModulus"]);
			config.FakeSetting("CertificateExponent", ConfigurationManager.AppSettings["CertificateExponent"]);
			config.FakeSetting("CertificateP", ConfigurationManager.AppSettings["CertificateP"]);
			config.FakeSetting("CertificateQ", ConfigurationManager.AppSettings["CertificateQ"]);
			config.FakeSetting("CertificateDP", ConfigurationManager.AppSettings["CertificateDP"]);
			config.FakeSetting("CertificateDQ", ConfigurationManager.AppSettings["CertificateDQ"]);
			config.FakeSetting("CertificateInverseQ", ConfigurationManager.AppSettings["CertificateInverseQ"]);
			config.FakeSetting("CertificateD", ConfigurationManager.AppSettings["CertificateD"]);
			config.FakeSetting("Hangfire", InfraTestConfigReader.AnalyticsConnectionString);
			if(TestSiteConfigurationSetup.URL != null)
			{
				config.FakeSetting("ManagerLocation", TestSiteConfigurationSetup.URL.AbsoluteUri + @"StardustDashboard/");
				config.FakeSetting("MessageBroker", TestSiteConfigurationSetup.URL.AbsoluteUri);
				config.FakeSetting("NumberOfNodes", "1");
			}
			return config;
		}

		public static FakeConfigReader AddConnectionString(this FakeConfigReader configReader, string name, string connectionString)
		{
			configReader.FakeConnectionString("Toggle", null as string);
			return configReader;
		}				
	}
}