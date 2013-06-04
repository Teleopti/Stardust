using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class PersonalShiftLayerViewModelTest : LayerViewModelTest
    {
        protected override string LayerModelDescription
        {
            get { return UserTexts.Resources.PersonalShifts; }
        }

        protected override LayerViewModel CreateTestInstance(ILayer layer)
        {
            return new PersonalShiftLayerViewModel(layer,null);
        }

        protected override bool ExpectMovePermitted
        {
            get { return true; }
        }

        protected override bool ExpectIsPayloadChangePermitted
        {
            get { return true; }
        }

		[Test]
		public void ShouldMoveUpAndDown()
		{
			var period = new DateTimePeriod(new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc), new DateTime(2001, 1, 1, 11, 0, 0, DateTimeKind.Utc));
			var activity = ActivityFactory.CreateActivity("activity");
			var layer = new PersonalShiftActivityLayer(activity, period);
			var personalShift1 = PersonalShiftFactory.CreatePersonalShift(activity, period);
			var personalShift2 = PersonalShiftFactory.CreatePersonalShift(activity, period);

			var personAssignment = PersonAssignmentFactory.CreatePersonAssignmentEmpty();
			personAssignment.AddPersonalShift(personalShift1);
			personAssignment.AddPersonalShift(personalShift2);

			Assert.IsTrue(personAssignment.PersonalShiftCollection.IndexOf(personalShift1) == 0);
			Assert.IsTrue(personAssignment.PersonalShiftCollection.IndexOf(personalShift2) == 1);

			var model = new PersonalShiftLayerViewModel(layer, personalShift2, null);
			model.MoveUp();
			
			Assert.IsTrue(personAssignment.PersonalShiftCollection.IndexOf(personalShift1) == 1);
			Assert.IsTrue(personAssignment.PersonalShiftCollection.IndexOf(personalShift2) == 0);

			model.MoveDown();
			Assert.IsTrue(personAssignment.PersonalShiftCollection.IndexOf(personalShift1) == 0);
			Assert.IsTrue(personAssignment.PersonalShiftCollection.IndexOf(personalShift2) == 1);
		}
    }
}
