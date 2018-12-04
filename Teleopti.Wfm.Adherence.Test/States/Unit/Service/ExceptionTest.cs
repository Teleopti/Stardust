using NUnit.Framework;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service
{
	[RtaTest]
	[TestFixture]
	public class ExceptionTest
	{
		public FakeDatabase Database;
		public Rta Target;

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