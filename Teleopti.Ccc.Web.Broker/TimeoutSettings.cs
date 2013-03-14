using System;
using System.Configuration;

namespace Teleopti.Ccc.Web.Broker
{
	public class TimeoutSettings
	{
		public static TimeoutSettings Load()
		{
			var settings = new TimeoutSettings();

			var value = ConfigurationManager.AppSettings["KeepAlive"];
			if (!string.IsNullOrEmpty(value))
			{
				settings.KeepAlive = TimeSpan.FromSeconds(Convert.ToInt32(value));
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
			return settings;
		}

		public TimeSpan? KeepAlive { get; set; }
		public TimeSpan? ConnectionTimeout { get; set; }
		public TimeSpan? DisconnectTimeout { get; set; }
		public Uri ScaleOutBackplaneUrl { get; set; }

		public int? DefaultMessageBufferSize { get; set; }
	}
}