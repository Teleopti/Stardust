using NUnit.Framework;
using SharpTestsEx;
using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.InfrastructureTest.MessageBroker;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, MessageBrokerUnitOfWorkTest]
	public class ASMScheduleChangeTimeRepositoryTest
	{
		public IASMScheduleChangeTimeRepository Target;
		public INow Now;

		[Test]
		public void ShouldAddScheduleChangeTime()
		{
			var personId = Guid.NewGuid();
			var nowInUtc = Now.UtcDateTime();
			Target.Add(new ASMScheduleChangeTime
			{
				PersonId = personId,
				TimeStamp = nowInUtc
			});

			var scheduleChangeTime = Target.GetScheduleChangeTime(personId);

			scheduleChangeTime.PersonId.Should().Be(personId);
			scheduleChangeTime.TimeStamp.Should().Be(nowInUtc);
		}

		[Test]
		public void ShouldUpdateScheduleChangeTime()
		{
			var personId = Guid.NewGuid();
			var nowInUtc = Now.UtcDateTime();
			Target.Add(new ASMScheduleChangeTime
			{
				PersonId = personId,
				TimeStamp = nowInUtc
			});

			var newTime = new DateTime(2017, 11, 24, 10, 30, 0);
			Target.Update(new ASMScheduleChangeTime
			{
				PersonId = personId,
				TimeStamp = newTime
			});

			var scheduleChangeTime = Target.GetScheduleChangeTime(personId);
			scheduleChangeTime.PersonId.Should().Be(personId);
			scheduleChangeTime.TimeStamp.Should().Be(newTime);
		}

		
	}
}
