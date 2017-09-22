using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
    [TestFixture]
    [Category("BucketB")]
    public class MatrixRaptorExternalLogOnSynchronizationTest
    {
		private IExternalLogOnRepository _repMock;
		private MatrixRaptorExternalLogOnSynchronization _target;

		[SetUp]
		public void Setup()
		{
			_repMock = MockRepository.GenerateMock<IExternalLogOnRepository>();
			_target = new MatrixRaptorExternalLogOnSynchronization(_repMock);
		}
        
		[Test]
		public void ShouldClearInvalidMartDataFromRaptorLogOn()
		{
			IExternalLogOn raptorLogOnInvalidMartData = new ExternalLogOn(1, 2, "1", "a1", true);

			_repMock.Stub(x => x.LoadAll()).Return(new List<IExternalLogOn> { raptorLogOnInvalidMartData });
			
			var affectedLogOn = _target.SynchronizeExternalLogOns(new List<IExternalLogOn>());

			affectedLogOn.Should().Be.EqualTo(0);
			raptorLogOnInvalidMartData.AcdLogOnMartId.Should().Be.EqualTo(-1);
			raptorLogOnInvalidMartData.DataSourceId.Should().Be.EqualTo(-1);
			raptorLogOnInvalidMartData.AcdLogOnAggId.Should().Be.EqualTo(2);
			raptorLogOnInvalidMartData.AcdLogOnOriginalId.Should().Be.EqualTo("2");
			raptorLogOnInvalidMartData.AcdLogOnName.Should().Be.EqualTo("2");
		}

		[Test]
		public void ShouldSynchronizeClearedRaptorLogOnWhenAggIdMatches()
		{
			IExternalLogOn raptorLogOnCleared = new ExternalLogOn(-1, 2, "2", "2", true);
			raptorLogOnCleared.DataSourceId = -1;
			IExternalLogOn matrixLogOn = new ExternalLogOn(1, 2, "9", "n9", true);

			_repMock.Stub(x => x.LoadAll()).Return(new List<IExternalLogOn> { raptorLogOnCleared });
			
			var affectedLogOns = _target.SynchronizeExternalLogOns(new List<IExternalLogOn> { matrixLogOn });

			affectedLogOns.Should().Be.EqualTo(1);
			raptorLogOnCleared.AcdLogOnMartId.Should().Be.EqualTo(matrixLogOn.AcdLogOnMartId);
			raptorLogOnCleared.AcdLogOnOriginalId.Should().Be.EqualTo(matrixLogOn.AcdLogOnOriginalId);
			raptorLogOnCleared.DataSourceId.Should().Be.EqualTo(matrixLogOn.DataSourceId);
			raptorLogOnCleared.AcdLogOnName.Should().Be.EqualTo(matrixLogOn.AcdLogOnName);
		}

		[Test]
		public void ShouldNotSynchronizeRaptorLogOnWhenNoAggOrMartIdMatches()
		{
			IExternalLogOn raptorLogOnCleared = new ExternalLogOn(-1, 2, "2", "2", true);
			IExternalLogOn matrixLogOn = new ExternalLogOn(1, 99, "origId", "name", true);

			_repMock.Stub(x => x.LoadAll()).Return(new List<IExternalLogOn> { raptorLogOnCleared });
			
			var affectedLogOns = _target.SynchronizeExternalLogOns(new List<IExternalLogOn> { matrixLogOn });

			affectedLogOns.Should().Be.EqualTo(1);
			raptorLogOnCleared.AcdLogOnMartId.Should().Be.EqualTo(-1);
			raptorLogOnCleared.DataSourceId.Should().Be.EqualTo(-1);
			raptorLogOnCleared.AcdLogOnOriginalId.Should().Be.EqualTo("2");
			raptorLogOnCleared.AcdLogOnName.Should().Be.EqualTo("2");

			_repMock.AssertWasCalled(x => x.AddRange(new List<IExternalLogOn> { matrixLogOn }));
		}

		[Test]
		public void ShouldSynchronizeRaptorQueueWhenMartIdMatches()
		{
			IExternalLogOn raptorLogOn = new ExternalLogOn(1, 2, "o1", "n1", true);
			IExternalLogOn matrixLogOn = new ExternalLogOn(1, 22, "origId", "name", true);

			_repMock.Stub(x => x.LoadAll()).Return(new List<IExternalLogOn> { raptorLogOn });
			
			var affectedLogOns = _target.SynchronizeExternalLogOns(new List<IExternalLogOn> { matrixLogOn });

			affectedLogOns.Should().Be.EqualTo(1);
			raptorLogOn.AcdLogOnMartId.Should().Be.EqualTo(matrixLogOn.AcdLogOnMartId);
			raptorLogOn.AcdLogOnOriginalId.Should().Be.EqualTo(matrixLogOn.AcdLogOnOriginalId);
			raptorLogOn.DataSourceId.Should().Be.EqualTo(matrixLogOn.DataSourceId);
			raptorLogOn.AcdLogOnName.Should().Be.EqualTo(matrixLogOn.AcdLogOnName);
		}
    }
}
