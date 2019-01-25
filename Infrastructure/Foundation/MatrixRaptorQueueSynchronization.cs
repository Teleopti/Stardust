using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	/// <summary>
	/// Synchronizes Matrix Queues and Raptor QueueSources
	/// </summary>
	/// <remarks>
	/// Created by: zoet
	/// Created date: 2008-05-07
	/// </remarks>
	public class MatrixRaptorQueueSynchronization
	{
		private readonly IQueueSourceRepository _queueSourceRepository;

		/// <summary>
		/// Initializes a new instance of the <see cref="MatrixRaptorQueueSynchronization"/> class.
		/// </summary>
		/// <param name="queueSourceRepository">The queue sourcerepository.</param>
		/// <remarks>
		/// Created by: zoet
		/// Created date: 2008-05-07
		/// </remarks>
		public MatrixRaptorQueueSynchronization(IQueueSourceRepository queueSourceRepository)
		{
			_queueSourceRepository = queueSourceRepository;
		}

		/// <summary>
		/// Synchronizes the queues between matrix and raptor.
		/// </summary>
		/// <param name="matrixQueues">The matrix queues.</param>
		/// <returns>The number of queues affected by synchronization.</returns>
		/// <remarks>
		/// Created by: Jonas N
		/// Created date: 2009-07-02
		/// </remarks>
		public int SynchronizeQueues(IList<IQueueSource> matrixQueues)
		{
			var raptorQueues = _queueSourceRepository.LoadAll();
			IList<IQueueSource> queuesToAdd = new List<IQueueSource>(matrixQueues);
			int updatedCount = 0;

			var matrixQueuesByMartId = matrixQueues.ToDictionary(k => k.QueueMartId);
			var matrixQueuesByAggId = matrixQueues.ToLookup(k => k.QueueAggId);

			clearInvalidMartDataOnRaptorQueues(raptorQueues, matrixQueuesByMartId);

			foreach (IQueueSource raptorQueue in raptorQueues)
			{
				IQueueSource matrixQueue;
				if ((matrixQueuesByMartId.TryGetValue(raptorQueue.QueueMartId, out matrixQueue) &&
					 raptorQueue.DataSourceId > 0) || findByLookup(matrixQueuesByAggId, raptorQueue, out matrixQueue))
				{
					// Newly upgraded/converted database with unmapped queue with matching QueueAggId´s
					// Or
					// queues with matching QueueMartId´s. 
					raptorQueue.DataSourceId = matrixQueue.DataSourceId;
					raptorQueue.QueueMartId = matrixQueue.QueueMartId;
					raptorQueue.QueueAggId = matrixQueue.QueueAggId;
					raptorQueue.QueueOriginalId = matrixQueue.QueueOriginalId;
					raptorQueue.Name = matrixQueue.Name;
					raptorQueue.Description = matrixQueue.Description;
					raptorQueue.LogObjectName = matrixQueue.LogObjectName;
					queuesToAdd.Remove(matrixQueue);
					updatedCount += 1;
				}
			}

			if (queuesToAdd.Count > 0)
			{
				foreach (var queue in queuesToAdd)
				{
					_queueSourceRepository.Add(queue);
				}
			}

			return queuesToAdd.Count + updatedCount;
		}

		private bool findByLookup(ILookup<int, IQueueSource> matrixQueuesByAggId, IQueueSource raptorQueue, out IQueueSource matrixQueue)
		{
			var validToFind = (raptorQueue.QueueMartId == -1 &&
							   raptorQueue.QueueOriginalId == "0" &&
							   raptorQueue.DataSourceId == 0 &&
							   raptorQueue.QueueAggId > 0);

			matrixQueue = matrixQueuesByAggId[raptorQueue.QueueAggId].FirstOrDefault();
			return validToFind && matrixQueue != null;
		}

		private static void clearInvalidMartDataOnRaptorQueues(IEnumerable<IQueueSource> raptorQueues, IDictionary<int, IQueueSource> matrixQueues)
		{
			foreach (var raptorQueue in raptorQueues)
			{
				IQueueSource matrixQueue;
				if (!matrixQueues.TryGetValue(raptorQueue.QueueMartId, out matrixQueue))
				{
					raptorQueue.QueueMartId = -1;
					raptorQueue.QueueOriginalId = "0";
					raptorQueue.DataSourceId = 0;
				}
			}
		}
	}
}
