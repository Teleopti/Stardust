using NUnit.Framework;
using Teleopti.Ccc.Domain.Rta.Service;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.Rta.Service
{
	[RtaTest]
	[TestFixture]
	public class ExceptionTest
	{
		public FakeDatabase Database;
		public Domain.Rta.Service.Rta Target;

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