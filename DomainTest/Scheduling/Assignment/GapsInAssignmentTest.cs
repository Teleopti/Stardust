using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    /// <summary>
    /// Tests the GapsInAssignment restriction
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-11-07
    /// </remarks>
    [TestFixture]
    public class GapsInAssignmentTest : RestrictionTest<IPersonAssignment>
    {
        private IScenario testScenario;
        private IPerson testAgent;
        private IPersonAssignment _testPersonAssignment;

        /// <summary>
        /// Runs every test. Implemented by repository's concrete implementation.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-07
        /// </remarks>
        protected override void ConcreteSetup()
        {
            Target = new GapsInAssignment();

            testScenario = ScenarioFactory.CreateScenarioAggregate();
            testAgent = PersonFactory.CreatePerson();
            _testPersonAssignment = new PersonAssignment(testAgent, testScenario);
        }

        /// <summary>
        /// Creates the invalid entity to verify.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-07
        /// </remarks>
        protected override IPersonAssignment CreateInvalidEntityToVerify()
        {
            IActivity phoneActivity = ActivityFactory.CreateActivity("Phone");

            MainShift ms1 = new MainShift(ShiftCategoryFactory.CreateShiftCategory("myCategory"));
            DateTimePeriod period1 =
                new DateTimePeriod(new DateTime(2007, 8, 11, 1, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 8, 11, 4, 0, 0, DateTimeKind.Utc));
            DateTimePeriod period2 =
                new DateTimePeriod(new DateTime(2007, 8, 11, 6, 30, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 8, 11, 7, 0, 0, DateTimeKind.Utc)); 
                        MainShiftActivityLayer layer1 = new MainShiftActivityLayer(phoneActivity, period1);
            MainShiftActivityLayer layer2 = new MainShiftActivityLayer(phoneActivity, period2);

            ms1.LayerCollection.Add(layer1);
            ms1.LayerCollection.Add(layer2);
            _testPersonAssignment.SetMainShift(ms1);

            PersonalShift ps1 = new PersonalShift();
            DateTimePeriod period3 =
                new DateTimePeriod(new DateTime(2007, 8, 11, 3, 45, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 8, 11, 6, 0, 0, DateTimeKind.Utc));

            PersonalShiftActivityLayer layer3 = new PersonalShiftActivityLayer(ActivityFactory.CreateActivity("Möte"), period3);

            ps1.LayerCollection.Add(layer3);
            _testPersonAssignment.AddPersonalShift(ps1);

            return _testPersonAssignment;
        }

        /// <summary>
        /// Creates the valid entity to verify.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-07
        /// </remarks>
        protected override IPersonAssignment CreateValidEntityToVerify()
        {
            MainShift ms1 = new MainShift(ShiftCategoryFactory.CreateShiftCategory("myCategory"));
            DateTimePeriod period1 =
                new DateTimePeriod(new DateTime(2007, 8, 11, 1, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 8, 12, 6, 0, 0, DateTimeKind.Utc));
            MainShiftActivityLayer layer1 = new MainShiftActivityLayer(ActivityFactory.CreateActivity("Phone"), period1);

            ms1.LayerCollection.Add(layer1);
            _testPersonAssignment.SetMainShift(ms1);

            return _testPersonAssignment;
        }
    }
}