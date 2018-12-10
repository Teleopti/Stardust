using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
    [TestFixture]
    public class OpenAbsenceRequestPeriodExtractorTest
    {
        private IOpenAbsenceRequestPeriodExtractor _target;
        private IWorkflowControlSet _workflowControlSet;
        private IAbsence _holidayAbsence;

        [SetUp]
        public void Setup()
        {
            _holidayAbsence = AbsenceFactory.CreateAbsence("Holiday");

            _workflowControlSet = new WorkflowControlSet("MySet");
            _target = new OpenAbsenceRequestPeriodExtractor(_workflowControlSet, _holidayAbsence);
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyViewpointDate()
        {
            Assert.AreEqual(DateOnly.Today, _target.ViewpointDate);
            DateOnly dateOnly = new DateOnly(2010, 1, 1);
            _target.ViewpointDate = dateOnly;
            Assert.AreEqual(dateOnly, _target.ViewpointDate);
        }

        [Test]
        public void VerifyGetAvailablePeriods()
        {
            IAbsenceRequestOpenPeriod absenceRequestOpenPeriodOne = new AbsenceRequestOpenDatePeriod();
            absenceRequestOpenPeriodOne.Absence = _holidayAbsence;
            absenceRequestOpenPeriodOne.OpenForRequestsPeriod = new DateOnlyPeriod(2010, 1 , 1, 2010, 2, 28);

            IAbsenceRequestOpenPeriod absenceRequestOpenPeriodTwo = new AbsenceRequestOpenDatePeriod();
            absenceRequestOpenPeriodTwo.Absence = _holidayAbsence;
            absenceRequestOpenPeriodTwo.OpenForRequestsPeriod = new DateOnlyPeriod(2010, 3 , 1, 2010, 4, 30);
            _workflowControlSet.AddOpenAbsenceRequestPeriod(absenceRequestOpenPeriodOne);
            _workflowControlSet.AddOpenAbsenceRequestPeriod(absenceRequestOpenPeriodTwo);

            _target.ViewpointDate = new DateOnly(2010, 01, 15);
            Assert.AreEqual(1, _target.AvailablePeriods.Count());

            _target.ViewpointDate = new DateOnly(2010, 03, 15);
            Assert.AreEqual(1, _target.AvailablePeriods.Count());

            _target.ViewpointDate = new DateOnly(2010, 06, 15);
            Assert.AreEqual(0, _target.AvailablePeriods.Count());

            IAbsence otherAbsence = AbsenceFactory.CreateAbsence("Flyttdag");
            IAbsenceRequestOpenPeriod absenceRequestOpenPeriodThree = new AbsenceRequestOpenDatePeriod();
            absenceRequestOpenPeriodThree.Absence = otherAbsence;
            absenceRequestOpenPeriodThree.OpenForRequestsPeriod = new DateOnlyPeriod(2010, 1, 1, 2010, 2, 28);
            _workflowControlSet.AddOpenAbsenceRequestPeriod(absenceRequestOpenPeriodThree);

            _target.ViewpointDate = new DateOnly(2010, 01, 15);
            Assert.AreEqual(1, _target.AvailablePeriods.Count());
        }

		[Test]
		public void VerifyAllPeriods()
		{
			IAbsenceRequestOpenPeriod absenceRequestOpenPeriodOne = new AbsenceRequestOpenDatePeriod();
			absenceRequestOpenPeriodOne.Absence = _holidayAbsence;
			_workflowControlSet.AddOpenAbsenceRequestPeriod(absenceRequestOpenPeriodOne);
			Assert.AreEqual(1, _target.AllPeriods.Count());

			IAbsenceRequestOpenPeriod absenceRequestOpenPeriodTwo = new AbsenceRequestOpenDatePeriod();
			absenceRequestOpenPeriodTwo.Absence = _holidayAbsence;

			_workflowControlSet.AddOpenAbsenceRequestPeriod(absenceRequestOpenPeriodTwo);

			Assert.AreEqual(2, _target.AllPeriods.Count());

			IAbsence otherAbsence = AbsenceFactory.CreateAbsence("Flyttdag");
			IAbsenceRequestOpenPeriod absenceRequestOpenPeriodThree = new AbsenceRequestOpenDatePeriod();
			absenceRequestOpenPeriodThree.Absence = otherAbsence;
			_workflowControlSet.AddOpenAbsenceRequestPeriod(absenceRequestOpenPeriodThree);

			_target.ViewpointDate = new DateOnly(2010, 01, 15);
			Assert.AreEqual(2, _target.AllPeriods.Count());
		}

        [Test]
        public void VerifyCanGetProjection()
        {
            IOpenAbsenceRequestPeriodProjection projection = _target.Projection;
            Assert.IsNotNull(projection);
        }
    }
}
