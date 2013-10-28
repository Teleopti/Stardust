using System;
using System.Globalization;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Common;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public abstract class ShiftForDate : IUserDataSetup
	{
		public DateTime Date;
		public IShiftCategory ShiftCategory;
		public readonly TimeSpan StartTime;
		public readonly TimeSpan EndTime;

		private readonly bool _withLunch;
		private DateTimePeriod _assignmentPeriod;

		public IScenario Scenario = GlobalDataMaker.Data().Data<CommonScenario>().Scenario;

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
		    var timeZone = user.PermissionInformation.DefaultTimeZone();
		    var shiftStartUtc = timeZone.SafeConvertTimeToUtc(Date.Add(StartTime));
		    var shiftEndUtc = timeZone.SafeConvertTimeToUtc(Date.Add(EndTime));

			var assignmentRepository = new PersonAssignmentRepository(uow);

			// create main shift
            _assignmentPeriod = new DateTimePeriod(shiftStartUtc, shiftEndUtc);
			var assignment = PersonAssignmentFactory.CreatePersonAssignment(user, Scenario, new DateOnly(Date));
			assignment.AddMainLayer(TestData.ActivityPhone, _assignmentPeriod);

			// add lunch
			if (_withLunch)
			{
                var lunchPeriod = new DateTimePeriod(shiftStartUtc.AddHours(3), shiftStartUtc.AddHours(4));
				assignment.AddMainLayer(TestData.ActivityLunch, lunchPeriod);
			}

			assignment.SetShiftCategory(ShiftCategory);

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