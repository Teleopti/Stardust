using NUnit.Framework;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence.Domain.Service
{
	[RtaTest]
	[TestFixture]
	public class ExceptionTest
	{
		public FakeDatabase Database;
		public Ccc.Domain.RealTimeAdherence.Domain.Service.Rta Target;

		[Test]
		public void ShouldThrowIfNoSourceId()
		{
			Assert.Throws<InvalidSourceException>(() =>
				Target.ProcessState(new StateForTest
				{
					SourceId = string.Empty
				})
				);
		}
		
		[Test]
		public void ShouldThrowIfNoPerson()
		{
			Assert.Throws<InvalidUserCodeException>(() =>
				Target.ProcessState(new StateForTest
				{
					UserCode = "unknown"
				})
				);
		}
		
	}
}