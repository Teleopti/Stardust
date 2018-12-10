using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Wfm.Adherence.Configuration;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
	public class RtaConfigurationValidationPoller : IDisposable
	{
		private readonly ICurrentDataSource _dataSource;
		private readonly Timer _timer;
		private readonly IList<Action<IEnumerable<string>>> _callbacks = new List<Action<IEnumerable<string>>>();
		private readonly object _lock = new object();
		private readonly HttpClient _client;
		private readonly string _serverUrl;

		public RtaConfigurationValidationPoller(ICurrentDataSource dataSource, IConfigReader config)
		{
			_client = new HttpClient();
			_dataSource = dataSource;
			_serverUrl = config.AppConfig("ConfigServer");
			_timer = new Timer(o =>
			{
				Action<IEnumerable<string>>[] callbacksCopy;
				lock (_lock)
				{
					if (_callbacks.IsEmpty())
						return;
					callbacksCopy = _callbacks.ToArray();
				}

				try
				{
					var texts = getTexts();
					callbacksCopy.ForEach(c => { c(texts); });
				}
				catch
				{
				}
			});
		}

		public IDisposable Poll(Action<IEnumerable<string>> callback)
		{
			lock (_lock)
				_callbacks.Add(callback);
			_timer.Change(10000, 10000);
			return new GenericDisposable(() =>
			{
				lock (_lock)
					_callbacks.Remove(callback);
			});
		}

		private IEnumerable<string> getTexts()
		{
			try
			{
				var json = _client.GetStringAsync(_serverUrl + "Rta/Configuration/Validate?tenant=" + _dataSource.CurrentName()).GetAwaiter().GetResult();
				var models = JsonConvert.DeserializeObject<IEnumerable<ConfigurationValidationViewModel>>(json);
				var texts = models
					.Select(x => string.Format(UserTexts.Resources.ResourceManager.GetString(x.Resource), x.Data.EmptyIfNull().Cast<object>().ToArray()))
					.ToArray();
				return texts;
			}
			catch (Exception)
			{
			}

			return Enumerable.Empty<string>();
		}

		public void Dispose()
		{
			_client?.Dispose();
		}
	}
}