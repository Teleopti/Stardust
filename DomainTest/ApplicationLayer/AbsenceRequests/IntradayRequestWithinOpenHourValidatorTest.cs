using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest]
	public class IntradayRequestWithinOpenHourValidatorTest : IIsolateSystem
	{
		public IntradayRequestWithinOpenHourValidator Target;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<IntradayRequestWithinOpenHourValidator>().For<IIntradayRequestWithinOpenHourValidator>();
		}

		[Test]
		public void ReturnWithinOpenPeriodIfPeriodWithinOpeningHour()
		{
			var skill = SkillFactory.CreateSkill("myskill");
			var timePeriod = new TimePeriod(new TimeSpan(8, 0, 0), new TimeSpan(15, 0, 0));
			var requestPeriod = new DateTimePeriod(new DateTime(2016,09,1,14,0,0,DateTimeKind.Utc) , new DateTime(2016,09,1,16,0,0,DateTimeKind.Utc));
			WorkloadFactory.CreateWorkloadWithOpenHours(skill,
				timePeriod);
			Target.Validate(skill, requestPeriod).Should().Be.EqualTo(OpenHourStatus.WithinOpenHour);
		}

		[Test]
		public void ShouldConsiderSkillTimeZoneDuringValidation()
		{
			var skill = SkillFactory.CreateSkill("myskill");
			skill.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			var timePeriod = new TimePeriod(new TimeSpan(8, 0, 0), new TimeSpan(15, 0, 0));
			var requestPeriod = new DateTimePeriod(new DateTime(2016, 09, 1, 14, 0, 0, DateTimeKind.Utc), new DateTime(2016, 09, 1, 16, 0, 0, DateTimeKind.Utc));
			WorkloadFactory.CreateWorkloadWithOpenHours(skill,
				timePeriod);
			
			Target.Validate(skill, requestPeriod).Should().Be.EqualTo(OpenHourStatus.OutsideOpenHour);
		}

		[Test]
		public void ReturnOutsideOpenPeriodIfNotWithinOpeningHour()
		{
			var skill = SkillFactory.CreateSkill("myskill");
			var timePeriod = new TimePeriod(new TimeSpan(8, 0, 0), new TimeSpan(21, 0, 0));
			var requestPeriod = new DateTimePeriod(new DateTime(2016, 09, 1, 14, 0, 0, DateTimeKind.Utc), new DateTime(2016, 09, 1, 16, 0, 0, DateTimeKind.Utc));
			WorkloadFactory.CreateWorkloadWithOpenHours(skill,
				timePeriod);
			foreach (var workloadDayTemplate in skill.WorkloadCollection.FirstOrDefault().TemplateWeekCollection)
			{
				//thrusday
				if (workloadDayTemplate.Key == 4)
					workloadDayTemplate.Value.ChangeOpenHours(new[] { new TimePeriod(new TimeSpan(8, 0, 0), new TimeSpan(13, 0, 0)) });
			}

			Target.Validate(skill, requestPeriod).Should().Be.EqualTo(OpenHourStatus.OutsideOpenHour);
		}

		[Test]
		public void ReturnMissingOpenHourIfNoOpeningHourFound()
		{
			var skill = SkillFactory.CreateSkill("myskill");
			var timePeriod = new TimePeriod(new TimeSpan(8, 0, 0), new TimeSpan(15, 0, 0));
			var requestPeriod = new DateTimePeriod(new DateTime(2016, 09, 3, 9, 0, 0, DateTimeKind.Utc), new DateTime(2016, 09, 3, 10, 0, 0, DateTimeKind.Utc));
			WorkloadFactory.CreateWorkload(skill);
			foreach (var workloadDayTemplate in skill.WorkloadCollection.FirstOrDefault().TemplateWeekCollection)
			{
				//thrusday
				if (workloadDayTemplate.Key != 6)
					workloadDayTemplate.Value.ChangeOpenHours(new[] { timePeriod });
			}
			Target.Validate(skill, requestPeriod).Should().Be.EqualTo(OpenHourStatus.MissingOpenHour);
		}

		[Test]
		public void ReturnWitinOpenHoursByMergingMultipleWorkloadOpenHours()
		{
			var skill = SkillFactory.CreateSkill("myskill");
			var timePeriodSmall = new TimePeriod(new TimeSpan(8, 0, 0), new TimeSpan(15, 0, 0));
			var timePeriodLarge = new TimePeriod(new TimeSpan(6, 0, 0), new TimeSpan(18, 0, 0));
			var requestPeriod = new DateTimePeriod(new DateTime(2016, 09, 3, 17, 0, 0, DateTimeKind.Utc), new DateTime(2016, 09, 3, 18, 0, 0, DateTimeKind.Utc));
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, timePeriodSmall);
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, timePeriodLarge);
			
			Target.Validate(skill, requestPeriod).Should().Be.EqualTo(OpenHourStatus.WithinOpenHour);
		}

		[Test]
		public void ReturnWitinOpenHoursIfSkillIsOpen24Hours()
		{
			var skill = SkillFactory.CreateSkill("myskill");
			var timePeriodLarge = new TimePeriod(new TimeSpan(6, 0, 0), new TimeSpan(18, 0, 0));
			var requestPeriod = new DateTimePeriod(new DateTime(2016, 09, 3, 2, 0, 0, DateTimeKind.Utc), new DateTime(2016, 09, 3, 3, 0, 0, DateTimeKind.Utc));
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, timePeriodLarge);
			IWorkload workload = new Domain.Forecasting.Workload(skill);
			workload.Description = "desc from factory";
			workload.Name = "name from factory";
			workload.TemplateWeekCollection.ForEach(x => x.Value.MakeOpen24Hours());
			skill.AddWorkload(workload);

			Target.Validate(skill, requestPeriod).Should().Be.EqualTo(OpenHourStatus.WithinOpenHour);
		}

		[Test]
		public void ReturnWintinPeriodIfSkillIsOpenUntilMidnight()
		{
			var skill = SkillFactory.CreateSkill("myskill");
			var timePeriodLarge = new TimePeriod(new TimeSpan(6, 0, 0), new TimeSpan(1,0, 0, 0));
			var requestPeriod = new DateTimePeriod(new DateTime(2016, 09, 3, 19, 0, 0, DateTimeKind.Utc), new DateTime(2016, 09, 3, 20, 0, 0, DateTimeKind.Utc));
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, timePeriodLarge);
			Target.Validate(skill, requestPeriod).Should().Be.EqualTo(OpenHourStatus.WithinOpenHour);
		}
	}
}