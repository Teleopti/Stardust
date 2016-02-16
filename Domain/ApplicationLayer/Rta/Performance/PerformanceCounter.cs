using System;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Performance
{
	public class PerformanceCounter : IPerformanceCounter
	{
		private readonly IMessageSender _messageSender;
		private readonly INow _now;
		private readonly IJsonSerializer _serializer;
		private int _count;

		public PerformanceCounter(IMessageSender messageSender, INow now, IJsonSerializer serializer)
		{
			_messageSender = messageSender;
			_now = now;
			_serializer = serializer;
			_count = 0;
		}

		public bool IsEnabled { get { return Limit > 0; } }
		public int Limit { get; set; }
		public Guid BusinessUnitId { get; set; }
		public string DataSource { get; set; }
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
				_messageSender.Send(new Message
				{
					BinaryData = _serializer.SerializeObject(message),
					DomainType = message.GetType().Name,
					BusinessUnitId = BusinessUnitId.ToString(),
					DataSource = DataSource
				});
			}
		}

		public void ResetCount()
		{
			_count = 0;
		}
	}
}