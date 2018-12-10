using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Wfm.Adherence.States;

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

		public int Count()
		{
			return _items.Count;
		}

		public class QueueItemInfo
		{
			public string OnTenant { get; set; } // remove on refact
			public BatchInputModel Model { get; set; }
		}
	}
}