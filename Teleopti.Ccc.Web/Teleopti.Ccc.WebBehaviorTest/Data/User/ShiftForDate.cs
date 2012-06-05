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
		public IShiftCategory ShiftCategory;
		public readonly TimeSpan StartTime;
		public readonly TimeSpan EndTime;

		private readonly bool _withLunch;
		private DateTimePeriod _assignmentPeriod;

		public IScenario Scenario = DataContext.Data().Data<CommonScenario>().Scenario;

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
			ShiftCategory = TestData.ShiftCategory;
			var dateUtc = user.PermissionInformation.DefaultTimeZone().ConvertTimeToUtc(Date);

			var assignmentRepository = new PersonAssignmentRepository(uow);

			// create main shift
			_assignmentPeriod = new DateTimePeriod(dateUtc.Add(StartTime), dateUtc.Add(EndTime));
			var assignment = PersonAssignmentFactory.CreatePersonAssignment(user, Scenario);
			assignment.SetMainShift(MainShiftFactory.CreateMainShift(TestData.ActivityPhone, _assignmentPeriod, ShiftCategory));

			// add lunch
			if (_withLunch)
			{
				var lunchPeriod = new DateTimePeriod(dateUtc.Add(StartTime).AddHours(3), dateUtc.Add(StartTime).AddHours(4));
				assignment.MainShift.LayerCollection.Add(new MainShiftActivityLayer(TestData.ActivityLunch, lunchPeriod));
			}

			assignmentRepository.Add(assignment);
		}

		protected abstract DateTime ApplyDate(CultureInfo cultureInfo);

		public TimeSpan GetContractTime()
		{
			// rolling my own contract time calculation.
			// do we need to do a projection here really?
			var contractTime = _assignmentPeriod.ElapsedTime();
			if (_withLunch)
				contractTime = contractTime.Subtract(TimeSpan.FromHours(1));
			return contractTime;
		}

	}
}