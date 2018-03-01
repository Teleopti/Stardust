using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;

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

		public void Enqueue(BatchInputModel model)
		{
			_items.Enqueue(new QueueItemInfo
			{
				OnTenant = _dataSource.CurrentName(),
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
			public BatchInputModel Model { get; set; }
		}


	}
}