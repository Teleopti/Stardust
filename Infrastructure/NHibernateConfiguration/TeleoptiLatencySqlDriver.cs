using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Threading;
using NHibernate.Driver;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public class TeleoptiLatencySqlDriver : SqlClientDriver
	{
		private int _latency;
		private bool _initialized;

		public TeleoptiLatencySqlDriver()
		{
			ConfigReader = new ConfigReader();
		}

		public IConfigReader ConfigReader { get; set; }

		public int Latency
		{
			get
			{
				//maybe do this thread safe but it's not the end of the world... only used in test environment.
				if (_initialized == false)
				{
					_latency = Convert.ToInt32(ConfigReader.AppConfig("latency"), CultureInfo.InvariantCulture);
					_initialized = true;
				}
				return _latency;
			}
		}

		public override DbCommand CreateCommand()
		{
			Thread.Sleep(Latency);
			return base.CreateCommand();
		}
	}
}