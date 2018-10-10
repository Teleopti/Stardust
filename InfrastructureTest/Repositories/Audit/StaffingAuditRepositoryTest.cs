using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Audit
{
	[TestFixture]
	[InfrastructureTest]
	public class StaffingAuditRepositoryTest 
		//: IIsolateSystem
	{
		//public SendUpdateStaffingReadModelHandler Target;
		//public IEventPublisherScope Publisher;
		//public IBusinessUnitRepository BusinessUnitRepository;
		//public IScenarioRepository ScenarioRepository;
		//public WithUnitOfWork WithUnitOfWork;
		//public IBusinessUnitScope BusinessUnitScope;
		//public FakeSkillDayRepository FakeSkillDayRepository;
		//public LegacyFakeEventPublisher FakeEventPublisher;

		//public void Isolate(IIsolate isolate)
		//{
		//	isolate.UseTestDouble<FakeSkillDayRepository>().For<ISkillDayRepository>();
		//	isolate.UseTestDouble<LegacyFakeEventPublisher>().For<IEventPublisher>();
		//}

		//[Test]
		//public void ShouldSendUpdateJob()
		//{
		//	FakeSkillDayRepository.HasSkillDays = true;
		//	WithUnitOfWork.Do(() =>
		//	{
		//		var bu = BusinessUnitFactory.CreateSimpleBusinessUnit("bu");
		//		BusinessUnitRepository.Add(bu);
		//		using (BusinessUnitScope.OnThisThreadUse(bu))
		//		{
		//			var scenario = ScenarioFactory.CreateScenario("Default scenario", true, false);
		//			ScenarioRepository.Add(scenario);
		//		}

		//	});

		//	Target.Handle(new TenantMinuteTickEvent());

		//	FakeEventPublisher.PublishedEvents.OfType<UpdateStaffingLevelReadModelEvent>().SingleOrDefault().Should().Not.Be
		//		.Null();
		//}
	}
}
