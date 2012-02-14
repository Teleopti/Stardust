using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
	[TestFixture]
	[Category("LongRunning")]
	public class MatrixRaptorQueueSynchronizationTest
	{
		private IQueueSourceRepository _repMock;
		private MatrixRaptorQueueSynchronization _target;

		[SetUp]
		public void Setup()
		{
			_repMock = MockRepository.GenerateMock<IQueueSourceRepository>();
			_target = new MatrixRaptorQueueSynchronization(_repMock);
		}

		[Test]
		public void ShouldClearInvalidMartDataFromRaptorQueue()
		{
			IQueueSource raptorQueueInvalidMartData = new QueueSource("q1", "q1", 1, 2, 3, 4);
			
			_repMock.Stub(x => x.LoadAllQueues()).Return(new List<IQueueSource> { raptorQueueInvalidMartData });
			_repMock.Stub(x => x.AddRange(new List<IQueueSource>()));
			
			var affectedQueues = _target.SynchronizeQueues(new List<IQueueSource>());

			affectedQueues.Should().Be.EqualTo(0);
			raptorQueueInvalidMartData.QueueMartId.Should().Be.EqualTo(0);
			raptorQueueInvalidMartData.QueueOriginalId.Should().Be.EqualTo(0);
			raptorQueueInvalidMartData.DataSourceId.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldSynchronizeClearedRaptorQueueWhenAggIdMatches()
		{
			IQueueSource raptorQueueCleared = new QueueSource("q1", "q1", 0, 2, 0, 0);
			IQueueSource matrixQueue = new QueueSource("mart q1", "mart q1 desc", 1, 2, 3, 4);

			_repMock.Stub(x => x.LoadAllQueues()).Return(new List<IQueueSource> { raptorQueueCleared });
			_repMock.Stub(x => x.AddRange(new List<IQueueSource>()));

			var affectedQueues = _target.SynchronizeQueues(new List<IQueueSource> { matrixQueue });

			affectedQueues.Should().Be.EqualTo(1);
			raptorQueueCleared.QueueMartId.Should().Be.EqualTo(matrixQueue.QueueMartId);
			raptorQueueCleared.QueueOriginalId.Should().Be.EqualTo(matrixQueue.QueueOriginalId);
			raptorQueueCleared.DataSourceId.Should().Be.EqualTo(matrixQueue.DataSourceId);
			raptorQueueCleared.Name.Should().Be.EqualTo(matrixQueue.Name);
			raptorQueueCleared.Description.Should().Be.EqualTo(matrixQueue.Description);
		}

		[Test]
		public void ShouldNotSynchronizeRaptorQueueWhenNoAggOrMartIdMatches()
		{
			IQueueSource raptorQueueCleared = new QueueSource("q1", "q1", 0, 2, 0, 0);
			IQueueSource matrixQueue = new QueueSource("mart q1", "mart q1 desc", 1, 99, 3, 4);

			_repMock.Stub(x => x.LoadAllQueues()).Return(new List<IQueueSource> { raptorQueueCleared });
			_repMock.Stub(x => x.AddRange(new List<IQueueSource> { matrixQueue }));

			var affectedQueues = _target.SynchronizeQueues(new List<IQueueSource> { matrixQueue });

			affectedQueues.Should().Be.EqualTo(1);
			raptorQueueCleared.QueueMartId.Should().Be.EqualTo(0);
			raptorQueueCleared.QueueOriginalId.Should().Be.EqualTo(0);
			raptorQueueCleared.DataSourceId.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldSynchronizeRaptorQueueWhenMartIdMatches()
		{
			IQueueSource raptorQueue = new QueueSource("q1", "q1", 1, 2, 3, 4);
			IQueueSource matrixQueue = new QueueSource("mart q1", "mart q1 desc", 11, 22, 3, 44);

			_repMock.Stub(x => x.LoadAllQueues()).Return(new List<IQueueSource> { raptorQueue });
			_repMock.Stub(x => x.AddRange(new List<IQueueSource>()));

			var affectedQueues = _target.SynchronizeQueues(new List<IQueueSource> { matrixQueue });

			affectedQueues.Should().Be.EqualTo(1);
			raptorQueue.QueueMartId.Should().Be.EqualTo(matrixQueue.QueueMartId);
			raptorQueue.QueueOriginalId.Should().Be.EqualTo(matrixQueue.QueueOriginalId);
			raptorQueue.DataSourceId.Should().Be.EqualTo(matrixQueue.DataSourceId);
			raptorQueue.Name.Should().Be.EqualTo(matrixQueue.Name);
			raptorQueue.Description.Should().Be.EqualTo(matrixQueue.Description);
		}
	}
}
