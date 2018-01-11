using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Configuration;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
	public class RtaConfigurationValidationPoller
	{
		private readonly Timer _timer;
		private readonly IList<Action<IEnumerable<string>>> _callbacks = new List<Action<IEnumerable<string>>>();
		private readonly object _lock = new object();

		public RtaConfigurationValidationPoller()
		{
			_timer = new Timer(o =>
			{
				if (!Monitor.TryEnter(_lock))
					return;
				try
				{
					if (_callbacks.IsEmpty())
						return;
					var texts = getTexts();
					_callbacks.ForEach(c => { c(texts); });
				}
				finally
				{
					Monitor.Exit(_lock);
				}
			});
		}

		public IDisposable Poll(Action<IEnumerable<string>> callback)
		{
			_callbacks.Add(callback);
			_timer.Change(10000, 10000);
			return new GenericDisposable(() => { _callbacks.Remove(callback); });
		}

		private IEnumerable<string> getTexts()
		{
			using (var client = new HttpClient())
			{
				try
				{
					var json = client.GetStringAsync(ConfigurationManager.AppSettings["ConfigServer"] + "Rta/Configuration/Validate?tenant=" + CurrentDataSource.Make().CurrentName()).Result;
					var models = JsonConvert.DeserializeObject<IEnumerable<ConfigurationValidationViewModel>>(json);
					var texts = models
						.Select(x => string.Format(x.English, x.Data.Cast<object>().ToArray()))
						.ToArray();
					return texts;
				}
				catch (Exception e)
				{
				}

				return Enumerable.Empty<string>();
			}
		}
	}
}