using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers
{
	[TestFixture]
	public class ScheduleDayReadModelsCreatorTest
	{
		private ScheduleDayReadModelsCreator _target;

		[SetUp]
		public void Setup()
		{
			_target = new ScheduleDayReadModelsCreator();
		}

		[Test]
		public void ShouldGetSchedulesAndCallCreator()
		{
			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));

			var ret =
				_target.GetReadModel(new ProjectionChangedEventScheduleDay
				{
					ContractTime = TimeSpan.FromHours(8),
					WorkTime = TimeSpan.FromHours(7),
					Date = new DateTime(2012, 12, 2),
					Shift = new ProjectionChangedEventShift
					{
						StartDateTime = new DateTime(2012, 12, 1, 8, 0, 0, DateTimeKind.Utc),
						EndDateTime = new DateTime(2012, 12, 1, 17, 0, 0, DateTimeKind.Utc)
					}
				}, person);

			ret.ContractTime.Should().Be.EqualTo(TimeSpan.FromHours(8));
			ret.WorkTime.Should().Be.EqualTo(TimeSpan.FromHours(7));
			ret.StartDateTime.Should().Be.EqualTo(new DateTime(2012, 12, 1, 9, 0, 0));
			ret.EndDateTime.Should().Be.EqualTo(new DateTime(2012, 12, 1, 18, 0, 0));
		}
	}
}