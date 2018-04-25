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
	public class ScheduleChangeMessagePollerTest : IIsolateSystem, IExtendSystem
	{
		public ScheduleChangeMessagePoller Target;
		public FakeASMScheduleChangeTimeRepository FakeASMScheduleChangeTimeRepository;
		public FakeScenarioRepository ScenarioRepository;
		public DefaultScenarioFromRepository DefaultScenarioFromRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public MutableNow Now;
		
		public void Extend(IExtend extend, IIocConfiguration configuration)
		{
			extend.AddService<ScheduleChangeMessagePoller>();
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeScenarioRepository>().For<IScenarioRepository>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			isolate.UseTestDouble<DefaultScenarioFromRepository>().For<ICurrentScenario>();
			isolate.UseTestDouble<MutableNow>().For<INow>();
			isolate.UseTestDouble<FakeASMScheduleChangeTimeRepository>().For<IASMScheduleChangeTimeRepository>();
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
			FakeASMScheduleChangeTimeRepository.Save(scheduleChangeMessage);

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

			FakeASMScheduleChangeTimeRepository.Save(scheduleChangeMessage);
			Now.Is(new DateTime(2017, 11, 27, 10, 30, 0));

			Target.Check().Should().Be.False();
		}
		

	}

}
