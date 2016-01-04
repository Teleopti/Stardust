using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

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
			IList<IQueueSource> raptorQueues = _queueSourceRepository.LoadAllQueues();
			IList<IQueueSource> queuesToAdd = new List<IQueueSource>(matrixQueues);
			int updatedCount = 0;

			clearInvalidMartDataOnRaptorQueues(raptorQueues, matrixQueues);

			foreach (IQueueSource matrixQueue in matrixQueues)
			{
				foreach (IQueueSource raptorQueue in raptorQueues)
				{
					if ((raptorQueue.QueueMartId == 0 &&
						raptorQueue.QueueOriginalId == 0 &&
						raptorQueue.DataSourceId == 0 &&
						raptorQueue.QueueAggId > 0 &&
						raptorQueue.QueueAggId == matrixQueue.QueueAggId)
						||
						(raptorQueue.QueueMartId == matrixQueue.QueueMartId &&
						raptorQueue.QueueOriginalId >= 0 &&
						raptorQueue.DataSourceId > 0))
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
			}

			_queueSourceRepository.AddRange(queuesToAdd);

			return queuesToAdd.Count + updatedCount;
		}

		private static void clearInvalidMartDataOnRaptorQueues(IList<IQueueSource> raptorQueues, IList<IQueueSource> matrixQueues)
		{
			var keyedMatrixQueues = matrixQueues.ToDictionary(k => k.QueueMartId);
			foreach (var raptorQueue in raptorQueues)
			{
				IQueueSource matrixQueue;
				if (!keyedMatrixQueues.TryGetValue(raptorQueue.QueueMartId, out matrixQueue))
				{
					raptorQueue.QueueMartId = 0;
					raptorQueue.QueueOriginalId = 0;
					raptorQueue.DataSourceId = 0;
				}
			}
		}
	}
}
