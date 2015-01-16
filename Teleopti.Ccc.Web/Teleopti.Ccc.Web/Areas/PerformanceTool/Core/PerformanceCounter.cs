using System;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Performance;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.Web.Areas.PerformanceTool.Core
{
	public class PerformanceCounter : IPerformanceCounter
	{
		private readonly IMessageSender _messageSender;
		private readonly INow _now;
		private int _count;

		public PerformanceCounter(IMessageSender messageSender, INow now)
		{
			_messageSender = messageSender;
			_now = now;
			_count = 0;
		}

		public bool IsEnabled { get { return Limit > 0; } }
		public int Limit { get; set; }
		public DateTime FirstTimestamp { get; set; }
		public DateTime LastTimestamp { get; set; }

		public void Count()
		{
			if (_count == 0)
				FirstTimestamp = _now.UtcDateTime();
			if (++_count == Limit)
			{
				LastTimestamp = _now.UtcDateTime();
				var message = new PerformanceCountDone {StartTime = FirstTimestamp, EndTime = LastTimestamp};
				_messageSender.Send(new Notification
				{
					BinaryData = JsonConvert.SerializeObject(message),
					DomainType = message.GetType().Name
				});
			}
		}
	}
}