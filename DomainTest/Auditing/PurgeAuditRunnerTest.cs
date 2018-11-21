using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Auditing
{
	//[DomainTest]
	//public class PurgeAuditRunnerTest
	//{
	//	public IPurgeAuditRunner Target;
	//	public FakeStaffingAuditRepository StaffingAuditRepository;
	//	public FakePersonAccessAuditRepository PersonAccessAuditRepository;

	//	[Test]
	//	public void RunAudits()
	//	{
	//		Target.Run();
	//		StaffingAuditRepository.PurgeCounter.Should().Be(1);
	//		PersonAccessAuditRepository.PurgeCounter.Should().Be(1);
	//	}

	//	[Test]
	//	public void ShouldRunOtherPurgesWhenOneCrashes1()
	//	{
	//		StaffingAuditRepository.ThrowOnPurgeOldAudits = true;
	//		Target.Run();
	//		StaffingAuditRepository.PurgeCounter.Should().Be(0);
	//		PersonAccessAuditRepository.PurgeCounter.Should().Be(1);
	//	}

	//	[Test]
	//	public void ShouldRunOtherPurgesWhenOneCrashes2()
	//	{
	//		PersonAccessAuditRepository.ThrowOnPurgeOldAudits = true;
	//		Target.Run();
	//		StaffingAuditRepository.PurgeCounter.Should().Be(1);
	//		PersonAccessAuditRepository.PurgeCounter.Should().Be(0);
	//	}
	//}
}
