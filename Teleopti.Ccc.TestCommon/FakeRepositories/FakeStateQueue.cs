using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeStateQueue : IStateQueueWriter, IStateQueueReader
	{
		private readonly ICurrentDataSource _dataSource;
		private readonly Queue<QueueItemInfo> _items = new Queue<QueueItemInfo>();

		public IEnumerable<QueueItemInfo> Items() => _items;

		public FakeStateQueue(ICurrentDataSource dataSource)
		{
			_dataSource = dataSource;
		}

		public void Enqueue(DateTime time, BatchInputModel model)
		{
			_items.Enqueue(new QueueItemInfo
			{
				OnTenant = _dataSource.CurrentName(),
				Time = time,
				Model = model
			});
		}

		public BatchInputModel Dequeue()
		{
			return _items.IsEmpty() ? null : _items.Dequeue()?.Model;
		}

		public class QueueItemInfo
		{
			public string OnTenant { get; set; }
			public DateTime Time { get; set; }
			public BatchInputModel Model { get; set; }
		}


	}
}