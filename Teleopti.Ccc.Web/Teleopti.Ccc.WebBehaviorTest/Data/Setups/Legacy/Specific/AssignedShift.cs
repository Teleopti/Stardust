using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Default;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific
{
	public class AssignedShift : IUserDataSetup
	{
		protected static readonly CultureInfo SwedishCultureInfo = CultureInfo.GetCultureInfo(1053);

		public string Date { get; set; }
		public IShiftCategory ShiftCategory;
		public string StartTime { get; set; }
		public string EndTime { get; set; }
		public string Foo { get; set; }
		public bool WithLunch { get; set; }
		public string Activity { get; set; }

		private DateTimePeriod _assignmentPeriod;

		public AssignedShift()
		{
			StartTime = TimeSpan.FromHours(9).ToString("g",SwedishCultureInfo);
			EndTime = TimeSpan.FromHours(17).ToString("g",SwedishCultureInfo);
			WithLunch = true;
			Date = DateOnlyForBehaviorTests.TestToday.ToShortDateString(SwedishCultureInfo);
			Foo = "asdf";
		}

		public IScenario Scenario = GlobalDataMaker.Data().Data<DefaultScenario>().Scenario;

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var date = ApplyDate(cultureInfo);
		    var timeZone = user.PermissionInformation.DefaultTimeZone();
		    var shiftStartUtc = timeZone.SafeConvertTimeToUtc(date.Add(TimeSpan.Parse(StartTime,SwedishCultureInfo)));
		    var shiftEndUtc = timeZone.SafeConvertTimeToUtc(date.Add(TimeSpan.Parse(EndTime,SwedishCultureInfo)));

			var assignmentRepository = new PersonAssignmentRepository(uow);

			IActivity activity;
			var activityRepository = new ActivityRepository(uow);
			if (Activity != null)
			{
				activity = new ActivityRepository(uow).LoadAll().Single(sCat => sCat.Description.Name.Equals(Activity));
			}
			else
			{
				activity = new Activity(RandomName.Make()) { DisplayColor = Color.FromKnownColor(KnownColor.Green) };
				activityRepository.Add(activity);
			}

			// create main shift
            _assignmentPeriod = new DateTimePeriod(shiftStartUtc, shiftEndUtc);
			var assignment = PersonAssignmentFactory.CreatePersonAssignment(user, Scenario, new DateOnly(date));
			assignment.AddActivity(activity, _assignmentPeriod);

			// add lunch
			if (WithLunch)
			{
				var lunchactivity = new Activity(RandomName.Make()) { DisplayColor = Color.FromKnownColor(KnownColor.Yellow) };
				activityRepository.Add(lunchactivity);

                var lunchPeriod = new DateTimePeriod(shiftStartUtc.AddHours(3), shiftStartUtc.AddHours(4));
				assignment.AddActivity(lunchactivity, lunchPeriod);
			}

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory(RandomName.Make(), "Purple");
			new ShiftCategoryRepository(uow).Add(shiftCategory);
			ShiftCategory = shiftCategory;

			assignment.SetShiftCategory(shiftCategory);

			assignmentRepository.Add(assignment);
		}

		protected virtual DateTime ApplyDate(CultureInfo cultureInfo)
		{
			return DateTime.Parse(Date,SwedishCultureInfo);
		}

		public TimeSpan GetContractTime()
		{
			// rolling my own contract time calculation.
			// do we need to do a projection here really?
			var contractTime = _assignmentPeriod.ElapsedTime();
			if (WithLunch)
				contractTime = contractTime.Subtract(TimeSpan.FromHours(1));
			return contractTime;
		}
	}
}