using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Staffing
{
	[InfrastructureTest]
	public class SendUpdateStaffingReadModelHandlerTest : ISetup
	{
		public SendUpdateStaffingReadModelHandler Target;
		public IEventPublisherScope Publisher;
		public IBusinessUnitRepository BusinessUnitRepository;
		public IScenarioRepository ScenarioRepository;
		public WithUnitOfWork WithUnitOfWork;
		public IBusinessUnitScope BusinessUnitScope;
		public FakeSkillDayRepository FakeSkillDayRepository;
		public LegacyFakeEventPublisher FakeEventPublisher;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeSkillDayRepository>().For<ISkillDayRepository>();
			system.UseTestDouble<LegacyFakeEventPublisher>().For<IEventPublisher>();
		}

		[Test]
		public void ShouldSendUpdateJob()
		{
			FakeSkillDayRepository.HasSkillDays = true;
			WithUnitOfWork.Do(() =>
			{
				var bu = BusinessUnitFactory.CreateSimpleBusinessUnit("bu");
				BusinessUnitRepository.Add(bu);
				using (BusinessUnitScope.OnThisThreadUse(bu))
				{
					var scenario = ScenarioFactory.CreateScenario("Default scenario", true, false);
					ScenarioRepository.Add(scenario);
				}

			});

			Target.Handle(new TenantMinuteTickEvent());

			FakeEventPublisher.PublishedEvents.OfType<UpdateStaffingLevelReadModelEvent>().SingleOrDefault().Should().Not.Be
				.Null();
		}
	}
}