using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
	[TestFixture]
	[Category("BucketB")]
	public class MatrixRaptorQueueSynchronizationTest
	{
		[Test]
		public void ShouldClearInvalidMartDataFromRaptorQueue()
		{
			var queueSourceRepository = MockRepository.GenerateMock<IQueueSourceRepository>();
			var target = new MatrixRaptorQueueSynchronization(queueSourceRepository);
			IQueueSource raptorQueueInvalidMartData = new QueueSource("q1", "q1", "1", "2", 3, 4);
			
			queueSourceRepository.Stub(x => x.LoadAll()).Return(new List<IQueueSource> { raptorQueueInvalidMartData });
			
			var affectedQueues = target.SynchronizeQueues(new List<IQueueSource>());

			affectedQueues.Should().Be.EqualTo(0);
			raptorQueueInvalidMartData.QueueMartId.Should().Be.EqualTo(-1);
			raptorQueueInvalidMartData.QueueOriginalId.Should().Be.EqualTo("0");
			raptorQueueInvalidMartData.DataSourceId.Should().Be.EqualTo(0);

			queueSourceRepository.AssertWasNotCalled(x => x.Add(null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldSynchronizeClearedRaptorQueueWhenAggIdMatches()
		{
			var queueSourceRepository = MockRepository.GenerateMock<IQueueSourceRepository>();
			var target = new MatrixRaptorQueueSynchronization(queueSourceRepository);

			IQueueSource raptorQueueCleared = new QueueSource("q1", "q1", "0", "2", 0, 0);
			IQueueSource matrixQueue = new QueueSource("mart q1", "mart q1 desc", "1", "2", 3, 4);

			queueSourceRepository.Stub(x => x.LoadAll()).Return(new List<IQueueSource> { raptorQueueCleared });
			
			var affectedQueues = target.SynchronizeQueues(new List<IQueueSource> { matrixQueue });

			affectedQueues.Should().Be.EqualTo(1);
			raptorQueueCleared.QueueMartId.Should().Be.EqualTo(matrixQueue.QueueMartId);
			raptorQueueCleared.QueueOriginalId.Should().Be.EqualTo(matrixQueue.QueueOriginalId);
			raptorQueueCleared.DataSourceId.Should().Be.EqualTo(matrixQueue.DataSourceId);
			raptorQueueCleared.Name.Should().Be.EqualTo(matrixQueue.Name);
			raptorQueueCleared.Description.Should().Be.EqualTo(matrixQueue.Description);

			queueSourceRepository.AssertWasNotCalled(x => x.Add(null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldNotSynchronizeRaptorQueueWhenNoAggOrMartIdMatches()
		{
			var queueSourceRepository = MockRepository.GenerateMock<IQueueSourceRepository>();
			var target = new MatrixRaptorQueueSynchronization(queueSourceRepository);
			IQueueSource raptorQueueCleared = new QueueSource("q1", "q1", "0", "2", 0, 0);
			IQueueSource matrixQueue = new QueueSource("mart q1", "mart q1 desc", "1", "99", 3, 4);

			queueSourceRepository.Stub(x => x.LoadAll()).Return(new List<IQueueSource> { raptorQueueCleared });
			
			var affectedQueues = target.SynchronizeQueues(new List<IQueueSource> { matrixQueue });

			affectedQueues.Should().Be.EqualTo(1);
			raptorQueueCleared.QueueMartId.Should().Be.EqualTo(-1);
			raptorQueueCleared.QueueOriginalId.Should().Be.EqualTo("0");
			raptorQueueCleared.DataSourceId.Should().Be.EqualTo(0);

			queueSourceRepository.AssertWasCalled(x => x.Add(matrixQueue));
		}

		[Test]
		public void ShouldSynchronizeRaptorQueueWhenMartIdMatches()
		{
			var queueSourceRepository = MockRepository.GenerateMock<IQueueSourceRepository>();
			var target = new MatrixRaptorQueueSynchronization(queueSourceRepository);
			IQueueSource raptorQueue = new QueueSource("q1", "q1", "1", "2", 3, 4);
			IQueueSource matrixQueue = new QueueSource("mart q1", "mart q1 desc", "11", "22", 3, 44);

			queueSourceRepository.Stub(x => x.LoadAll()).Return(new List<IQueueSource> { raptorQueue });
			
			var affectedQueues = target.SynchronizeQueues(new List<IQueueSource> { matrixQueue });

			affectedQueues.Should().Be.EqualTo(1);
			raptorQueue.QueueMartId.Should().Be.EqualTo(matrixQueue.QueueMartId);
			raptorQueue.QueueOriginalId.Should().Be.EqualTo(matrixQueue.QueueOriginalId);
			raptorQueue.DataSourceId.Should().Be.EqualTo(matrixQueue.DataSourceId);
			raptorQueue.Name.Should().Be.EqualTo(matrixQueue.Name);
			raptorQueue.Description.Should().Be.EqualTo(matrixQueue.Description);

			queueSourceRepository.AssertWasNotCalled(x => x.Add(null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldNotCreateAnotherQueueSourceWhenMartIdMatchesAndItIsZero()
		{
			var queueSourceRepository = MockRepository.GenerateMock<IQueueSourceRepository>();
			var target = new MatrixRaptorQueueSynchronization(queueSourceRepository);
			IQueueSource raptorQueue = new QueueSource("q1", "q1", "1", "2", 0, 4);
			IQueueSource newRaptorQueue = new QueueSource("q2", "q2", "2", "3", 0, 4);
			IQueueSource matrixQueue = new QueueSource("mart q1", "mart q1 desc", "11", "22", 0, 4);

			queueSourceRepository.Stub(x => x.LoadAll()).Return(new List<IQueueSource> { raptorQueue, newRaptorQueue });
			
			var affectedQueues = target.SynchronizeQueues(new List<IQueueSource> { matrixQueue });

			affectedQueues.Should().Be.EqualTo(2);
			queueSourceRepository.AssertWasNotCalled(x => x.Add(null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldNotCreateAnotherQueueSourceWhenOriginalIdIsNegative()
		{
			var queueSourceRepository = MockRepository.GenerateMock<IQueueSourceRepository>();
			var target = new MatrixRaptorQueueSynchronization(queueSourceRepository);
			IQueueSource raptorQueue = new QueueSource("q1", "q1", "-11", "2", 0, 4);
			IQueueSource matrixQueue = new QueueSource("mart q1", "mart q1 desc", "-11", "2", 0, 4);

			queueSourceRepository.Stub(x => x.LoadAll()).Return(new List<IQueueSource> { raptorQueue });

			var affectedQueues = target.SynchronizeQueues(new List<IQueueSource> { matrixQueue });

			affectedQueues.Should().Be.EqualTo(1);
			queueSourceRepository.AssertWasNotCalled(x => x.Add(null), o => o.IgnoreArguments());
		}
	}
}
