using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Auditing
{
	[DomainTest]
	public class PurgeAuditRunnerTest
	{
		public PurgeAuditRunner Target;
		public FakeStaffingAuditRepository StaffingAuditRepository;
		public FakePersonAccessAuditRepository PersonAccessAuditRepository;

		[Test]
		public void RunAudits()
		{
			Target.Run();
			StaffingAuditRepository.PurgeCounter.Should().Be(1);
			PersonAccessAuditRepository.PurgeCounter.Should().Be(1);
		}
	}
}
