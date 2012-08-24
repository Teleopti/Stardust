using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class ShiftConfigurable : IUserDataSetup
	{
		private DateTimePeriod _assignmentPeriod;

		public string ShiftCategoryName { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public bool Lunch { get; set; }

		public IScenario Scenario = GlobalDataContext.Data().Data<CommonScenario>().Scenario;

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			//för nu sparar ett shiftcategory hårt
			//var shiftCat =new ShiftCategoryRepository(uow).LoadAll().Single(sCat => sCat.Description.Name.Equals(ShiftCategoryName));
			var shiftCat = new ShiftCategory("will be removed");
			new ShiftCategoryRepository(uow).Add(shiftCat);

			var dateUtc = user.PermissionInformation.DefaultTimeZone().ConvertTimeToUtc(StartTime.Date);

			var assignmentRepository = new PersonAssignmentRepository(uow);

			// create main shift
			_assignmentPeriod = new DateTimePeriod(dateUtc.Add(StartTime.TimeOfDay), dateUtc.Add(EndTime.TimeOfDay));
			var assignment = PersonAssignmentFactory.CreatePersonAssignment(user, Scenario);
			assignment.SetMainShift(MainShiftFactory.CreateMainShift(TestData.ActivityPhone, _assignmentPeriod, shiftCat));

			// add lunch
			if (Lunch)
			{
				var lunchPeriod = new DateTimePeriod(dateUtc.Add(StartTime.TimeOfDay).AddHours(3), dateUtc.Add(StartTime.TimeOfDay).AddHours(4));
				assignment.MainShift.LayerCollection.Add(new MainShiftActivityLayer(TestData.ActivityLunch, lunchPeriod));
			}

			assignmentRepository.Add(assignment);
		}

		public TimeSpan GetContractTime()
		{
			// rolling my own contract time calculation.
			// do we need to do a projection here really?
			var contractTime = _assignmentPeriod.ElapsedTime();
			if (Lunch)
				contractTime = contractTime.Subtract(TimeSpan.FromHours(1));
			return contractTime;
		}

	}
}