using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.TestData
{
	public class ShiftForDate : IUserDataSetup
	{
		public DateTime Date;
		public IScenario Scenario;
		public IShiftCategory ShiftCategory;
		public IActivity ActivityPhone;
		public IActivity ActivityLunch;
		public readonly TimeSpan StartTime;
		public readonly TimeSpan EndTime;

		private readonly bool _withLunch;
		private DateTimePeriod _assignmentPeriod;


		public ShiftForDate(DateTime date, TimeSpan startTime, TimeSpan endTime, IScenario scenario, IShiftCategory shiftCategory, IActivity activityPhone, IActivity activityLunch)
			: this(date, startTime, endTime, true, scenario, shiftCategory, activityPhone, activityLunch) { }

		public ShiftForDate(DateTime date, int startHour, IScenario scenario, IShiftCategory shiftCategory, IActivity activityPhone, IActivity activityLunch)
			: this(date, TimeSpan.FromHours(startHour), TimeSpan.FromHours(startHour + 8), scenario, shiftCategory, activityPhone, activityLunch) { }

		public ShiftForDate(DateTime date, TimeSpan startTime, TimeSpan endTime, bool withLunch, IScenario scenario, IShiftCategory shiftCategory, IActivity activityPhone, IActivity activityLunch)
		{
			StartTime = startTime;
			EndTime = endTime;
			_withLunch = withLunch;
			ShiftCategory = shiftCategory;
			ActivityPhone = activityPhone;
			Scenario = scenario;
			ActivityLunch = activityLunch;
			Date = date;
		}

		public void Apply(ICurrentUnitOfWork currentUnitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			var dateUtc = person.PermissionInformation.DefaultTimeZone().SafeConvertTimeToUtc(Date);

			var assignmentRepository = new PersonAssignmentRepository(currentUnitOfWork);

			// create main shift
			_assignmentPeriod = new DateTimePeriod(dateUtc.Add(StartTime), dateUtc.Add(EndTime));
			var assignment = PersonAssignmentFactory.CreatePersonAssignment(person, Scenario, new DateOnly(Date));
			assignment.AddActivity(ActivityPhone, _assignmentPeriod);

			// add lunch
			if (_withLunch)
			{
				var lunchPeriod = new DateTimePeriod(dateUtc.Add(StartTime).AddHours(3), dateUtc.Add(StartTime).AddHours(4));
				assignment.AddActivity(ActivityLunch, lunchPeriod);
			}

			assignment.SetShiftCategory(ShiftCategory);

			assignmentRepository.Add(assignment);
			PersonAssignment = assignment;
		}

		public IPersonAssignment PersonAssignment { get; set; }
		
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