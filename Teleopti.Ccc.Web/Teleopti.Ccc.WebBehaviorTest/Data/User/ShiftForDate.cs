using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public abstract class ShiftForDate : IUserDataSetup
	{
		public DateTime Date;
		public readonly TimeSpan StartTime;
		public readonly TimeSpan EndTime;

		private readonly bool _withLunch;

		protected ShiftForDate(TimeSpan startTime, TimeSpan endTime)
		{
			StartTime = startTime;
			EndTime = endTime;
			_withLunch = true;
		}

		protected ShiftForDate(int startHour)
			: this(TimeSpan.FromHours(startHour), TimeSpan.FromHours(startHour + 8)) { }

		protected ShiftForDate(TimeSpan startTime, TimeSpan endTime, bool withLunch)
		{
			StartTime = startTime;
			EndTime = endTime;
			_withLunch = withLunch;
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			Date = ApplyDate(cultureInfo);
			var dateUtc = user.PermissionInformation.DefaultTimeZone().ConvertTimeToUtc(Date);

			var assignmentRepository = new PersonAssignmentRepository(uow);

			// create main shift
			var layerPeriod = new DateTimePeriod(dateUtc.Add(StartTime), dateUtc.Add(EndTime));
			var assignment = PersonAssignmentFactory.CreatePersonAssignment(user, TestData.Scenario);
			assignment.SetMainShift(MainShiftFactory.CreateMainShift(TestData.ActivityPhone, layerPeriod, TestData.ShiftCategory));

			// add lunch
			if (_withLunch)
			{
				layerPeriod = layerPeriod.ChangeStartTime(TimeSpan.FromHours(3)).ChangeEndTime(TimeSpan.FromHours(-4));
				assignment.MainShift.LayerCollection.Add(new MainShiftActivityLayer(TestData.ActivityLunch, layerPeriod));
			}

			assignmentRepository.Add(assignment);
		}

		protected abstract DateTime ApplyDate(CultureInfo cultureInfo);
	}
}