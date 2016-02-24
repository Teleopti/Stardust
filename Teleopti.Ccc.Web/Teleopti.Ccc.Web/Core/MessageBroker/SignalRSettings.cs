using System;
using System.Configuration;
using System.Globalization;

namespace Teleopti.Ccc.Web.Broker
{
	public class SignalRSettings
	{
		public static SignalRSettings Load()
		{
			var settings = new SignalRSettings();

			var value = ConfigurationManager.AppSettings["KeepAlive"];
			if (!string.IsNullOrEmpty(value))
			{
				int seconds;
				settings.KeepAlive = Int32.TryParse(value,NumberStyles.Integer,CultureInfo.InvariantCulture,out seconds) ? TimeSpan.FromSeconds(seconds) : (TimeSpan?)null;
			}

			value = ConfigurationManager.AppSettings["ConnectionTimeout"];
			if (!string.IsNullOrEmpty(value))
			{
				settings.ConnectionTimeout = TimeSpan.FromSeconds(Convert.ToInt32(value));
			}

			value = ConfigurationManager.AppSettings["DisconnectTimeout"];
			if (!string.IsNullOrEmpty(value))
			{
				settings.DisconnectTimeout = TimeSpan.FromSeconds(Convert.ToInt32(value));
			}

			value = ConfigurationManager.AppSettings["DefaultMessageBufferSize"];
			if (!string.IsNullOrEmpty(value))
			{
				settings.DefaultMessageBufferSize = Convert.ToInt32(value);
			}

			value = ConfigurationManager.AppSettings["ScaleOutBackplaneUrl"];
			Uri uri;
			if (!string.IsNullOrEmpty(value) && Uri.TryCreate(value,UriKind.RelativeOrAbsolute, out uri))
			{
				settings.ScaleOutBackplaneUrl = uri;
			}

			value = ConfigurationManager.AppSettings["EnablePerformanceCounters"];
			settings.EnablePerformanceCounters = !string.IsNullOrEmpty(value) && Convert.ToBoolean(value);

			value = ConfigurationManager.AppSettings["ThrottleMessages"];
			settings.ThrottleMessages = string.IsNullOrEmpty(value) || Convert.ToBoolean(value);

			value = ConfigurationManager.AppSettings["MessagesPerSecond"];
			settings.MessagesPerSecond = string.IsNullOrEmpty(value) ? 80 : Convert.ToInt32(value);

			value = ConfigurationManager.AppSettings["SignalRBackplaneType"];
			settings.SignalRBackplaneType = parseSignalRBackplaneType(value);

			switch (settings.SignalRBackplaneType)
			{
				case SignalRBackplaneType.SqlServer:
					settings.SqlServerBackplaneConnectionString =
						ConfigurationManager.ConnectionStrings["MessageBroker"].ConnectionString;
					break;
				case SignalRBackplaneType.AzureServiceBus:
					settings.AzureServiceBusBackplaneConnectionString =
						ConfigurationManager.ConnectionStrings["AzureServiceBusBackplane"].ConnectionString;
					break;
                case SignalRBackplaneType.Redis:
                    settings.RedisBackplaneConnectionString =
                        ConfigurationManager.ConnectionStrings["RedisBackplane"].ConnectionString;
			        break;
			}
            return settings;
		}


	    private static SignalRBackplaneType parseSignalRBackplaneType(string value)
		{
			if (string.IsNullOrEmpty(value))
				return SignalRBackplaneType.Nothing;
			SignalRBackplaneType result;
			return !Enum.TryParse(value, true, out result) ? SignalRBackplaneType.Nothing : result;
		}

		public SignalRBackplaneType SignalRBackplaneType { get; set; }

		public TimeSpan? KeepAlive { get; set; }
		public TimeSpan? ConnectionTimeout { get; set; }
		public TimeSpan? DisconnectTimeout { get; set; }
		public int? DefaultMessageBufferSize { get; set; }
		public Uri ScaleOutBackplaneUrl { get; set; }
		public bool ThrottleMessages { get; set; }
		public int MessagesPerSecond { get; set; }
		public bool EnablePerformanceCounters { get; set; }
	    public string SqlServerBackplaneConnectionString { get; set; }
		public string AzureServiceBusBackplaneConnectionString { get; set; }
        public string RedisBackplaneConnectionString { get; set; }
    }

	public enum SignalRBackplaneType
	{
		Nothing,
		SqlServer,
		AzureServiceBus,
        Redis
	}
}