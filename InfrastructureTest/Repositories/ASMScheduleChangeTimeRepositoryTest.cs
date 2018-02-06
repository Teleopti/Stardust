using NUnit.Framework;
using SharpTestsEx;
using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, DatabaseTest]
	public class ASMScheduleChangeTimeRepositoryTest
	{
		public IASMScheduleChangeTimeRepository Target;
		public INow Now;
		public WithUnitOfWork WithUnitOfWork;

		[Test]
		public void ShouldAddScheduleChangeTime()
		{

			var personId = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var nowInUtc = Now.UtcDateTime();
			WithUnitOfWork.Do(() =>
			{
				Target.Save(new ASMScheduleChangeTime
				{
					PersonId = personId,
					TimeStamp = nowInUtc
				}, new ASMScheduleChangeTime
				{
					PersonId = personId2,
					TimeStamp = nowInUtc
				});
			});

			WithUnitOfWork.Do(() =>
			{
				var scheduleChangeTime = Target.GetScheduleChangeTime(personId);

				scheduleChangeTime.PersonId.Should().Be(personId);
				scheduleChangeTime.TimeStamp.Should().Be(nowInUtc);

				scheduleChangeTime = Target.GetScheduleChangeTime(personId2);

				scheduleChangeTime.PersonId.Should().Be(personId2);
				scheduleChangeTime.TimeStamp.Should().Be(nowInUtc);
			});

		}

		[Test]
		public void ShouldUpdateScheduleChangeTime()
		{

			var personId = Guid.NewGuid();
			var nowInUtc = Now.UtcDateTime();
			WithUnitOfWork.Do(() =>
			{
				Target.Save(new ASMScheduleChangeTime
				{
					PersonId = personId,
					TimeStamp = nowInUtc
				});
			});
			var newTime = new DateTime(2017, 11, 24, 10, 30, 0);
			WithUnitOfWork.Do(() =>
			{
				Target.Save(new ASMScheduleChangeTime
				{
					PersonId = personId,
					TimeStamp = newTime
				});
			});
			WithUnitOfWork.Do(() =>
			{
				var scheduleChangeTime = Target.GetScheduleChangeTime(personId);
				scheduleChangeTime.PersonId.Should().Be(personId);
				scheduleChangeTime.TimeStamp.Should().Be(newTime);
			});

		}


	}
}
