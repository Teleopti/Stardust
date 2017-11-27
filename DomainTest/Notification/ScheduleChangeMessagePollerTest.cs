using NUnit.Framework;
using System;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Domain.Notification;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;

namespace Teleopti.Ccc.DomainTest.Notification
{
	[TestFixture, DomainTest]
	public class ScheduleChangeMessagePollerTest : ISetup
	{
		public ScheduleChangeMessagePoller Target;
		public FakeASMScheduleChangeTimeRepository FakeASMScheduleChangeTimeRepository;
		public FakeScenarioRepository ScenarioRepository;
		public DefaultScenarioFromRepository DefaultScenarioFromRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public MutableNow Now;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<ScheduleChangeMessagePoller>();
			system.UseTestDouble<FakeScenarioRepository>().For<IScenarioRepository>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<DefaultScenarioFromRepository>().For<ICurrentScenario>();
			system.UseTestDouble<MutableNow>().For<INow>();
			system.UseTestDouble<FakeASMScheduleChangeTimeRepository>().For<IASMScheduleChangeTimeRepository>();
		}

		[Test]
		public void ShouldReturnTrueWhenHasScheduleChangedWithinPollInterval()
		{
			var scenario = ScenarioFactory.CreateScenario("test", false, false).WithId();
			ScenarioRepository.Add(scenario);
			var me = PersonFactory.CreatePerson().WithId();
			LoggedOnUser.SetFakeLoggedOnUser(me);
			me.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			var scheduleChangeMessage = new ASMScheduleChangeTime
			{
				PersonId = me.Id.Value,
				TimeStamp = Now.UtcDateTime()
			};
			FakeASMScheduleChangeTimeRepository.Add(scheduleChangeMessage);

			Target.Check().Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldReturnFalseWhenNoScheduleChangedDataForAgent()
		{
			var scenario = ScenarioFactory.CreateScenario("test", false, false).WithId();
			ScenarioRepository.Add(scenario);
			var me = PersonFactory.CreatePerson().WithId();
			LoggedOnUser.SetFakeLoggedOnUser(me);
			me.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

			Target.Check().Should().Be.EqualTo(false);
		}

		[Test]
		public void ShouldReturnFalseWhenHasScheduleChangedOutsideOfPollInterval()
		{
			var scenario = ScenarioFactory.CreateScenario("test", false, false).WithId();
			ScenarioRepository.Add(scenario);
			var me = PersonFactory.CreatePerson().WithId();
			LoggedOnUser.SetFakeLoggedOnUser(me);

			var scheduleChangeMessage = new ASMScheduleChangeTime
			{
				PersonId = me.Id.Value,
				TimeStamp = new DateTime(2017, 11, 27, 10, 0, 0)
			};

			FakeASMScheduleChangeTimeRepository.Add(scheduleChangeMessage);
			Now.Is(new DateTime(2017, 11, 27, 10, 30, 0));

			Target.Check().Should().Be.False();
		}
		

	}

}
