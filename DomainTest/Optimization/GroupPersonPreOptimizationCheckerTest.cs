using NUnit.Framework;
using System.Collections.Generic;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class GroupPersonPreOptimizationCheckerTest
    {
        private MockRepository _mock;
        private IGroupPersonSameForPersonOnDateChecker _groupPersonSameForPersonOnDateChecker;
        private IGroupPersonSchedulePeriodChecker _groupPersonSchedulePeriodChecker;
        private GroupPersonPreOptimizationChecker _target;
        private IPerson _person;
        private IGroupPersonSameDayOffsChecker _groupPersonSameDayOffsChecker;
    	private ISchedulingOptions _schedulingOptions;

    	[SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _groupPersonSameForPersonOnDateChecker = _mock.StrictMock<IGroupPersonSameForPersonOnDateChecker>();
            _groupPersonSchedulePeriodChecker = _mock.StrictMock<IGroupPersonSchedulePeriodChecker>();
            _groupPersonSameDayOffsChecker = _mock.StrictMock<IGroupPersonSameDayOffsChecker>();
        	_schedulingOptions = new SchedulingOptions();
            _person = _mock.StrictMock<IPerson>();
            _target = new GroupPersonPreOptimizationChecker(_groupPersonSameForPersonOnDateChecker, _groupPersonSameDayOffsChecker, _groupPersonSchedulePeriodChecker);
        }

        [Test]
        public void ShouldReturnTrueIfAllChecksAreGood()
        {
            var daysOffToRemove = new List<DateOnly>();
            var daysOffToAdd = new List<DateOnly>();
            var dates = new List<DateOnly>();
            var allSelectedPersons = new List<IPerson>();
            var groupPerson = _mock.StrictMock<IGroupPerson>();
            var dic = new List<IScheduleMatrixPro>();
            Expect.Call(_groupPersonSameForPersonOnDateChecker.FindCommonGroupPersonForPersonOnDates(_person, dates,
																									 allSelectedPersons, _schedulingOptions)).Return(groupPerson);
            Expect.Call(_groupPersonSchedulePeriodChecker.AllInSameGroupHasSameSchedulePeriod(groupPerson, dates)).
                Return(true);
            Expect.Call(_groupPersonSameDayOffsChecker.CheckGroupPerson(dic, groupPerson, daysOffToRemove, daysOffToAdd))
                .Return(true);
            _mock.ReplayAll();
			Assert.That(_target.CheckPersonOnDates(dic, _person, daysOffToRemove, daysOffToAdd, allSelectedPersons, _schedulingOptions), Is.Not.Null);
            _mock.VerifyAll();
        }

        [Test]
        public void ShouldReturnFalseIfNoCommonGroupPerson()
        {
            var daysOffToRemove = new List<DateOnly>();
            var daysOffToAdd = new List<DateOnly>();
            var allSelectedPersons = new List<IPerson>();
            var dic = new List<IScheduleMatrixPro>();

            Expect.Call(_groupPersonSameForPersonOnDateChecker.FindCommonGroupPersonForPersonOnDates(_person, new List<DateOnly>(),
																									  allSelectedPersons, _schedulingOptions)).Return(null);
            _mock.ReplayAll();
			Assert.That(_target.CheckPersonOnDates(dic, _person, daysOffToRemove, daysOffToAdd, allSelectedPersons, _schedulingOptions), Is.Null);

        }

        [Test]
        public void ShouldReturnFalseIfNoCommonSchedulePeriod()
        {
            var daysOffToRemove = new List<DateOnly>();
            var daysOffToAdd = new List<DateOnly>();
            var dates = new List<DateOnly>();
            var allSelectedPersons = new List<IPerson>();
            var groupPerson = _mock.StrictMock<IGroupPerson>();
            var dic = new List<IScheduleMatrixPro>();
            Expect.Call(_groupPersonSameForPersonOnDateChecker.FindCommonGroupPersonForPersonOnDates(_person, dates,
																									 allSelectedPersons, _schedulingOptions)).Return(groupPerson);
            Expect.Call(_groupPersonSchedulePeriodChecker.AllInSameGroupHasSameSchedulePeriod(groupPerson, dates)).
                Return(false);
            _mock.ReplayAll();
			Assert.That(_target.CheckPersonOnDates(dic, _person, daysOffToRemove, daysOffToAdd, allSelectedPersons, _schedulingOptions), Is.Null);
            _mock.VerifyAll();
        }

        [Test]
        public void ShouldReturnFalseIfCheckGroupPersonFails()
        {
            var daysOffToRemove = new List<DateOnly>();
            var daysOffToAdd = new List<DateOnly>();
            var dates = new List<DateOnly>();
            var allSelectedPersons = new List<IPerson>();
            var groupPerson = _mock.StrictMock<IGroupPerson>();
            var dic = new List<IScheduleMatrixPro>();
            Expect.Call(_groupPersonSameForPersonOnDateChecker.FindCommonGroupPersonForPersonOnDates(_person, dates,
																									 allSelectedPersons, _schedulingOptions)).Return(groupPerson);
            Expect.Call(_groupPersonSchedulePeriodChecker.AllInSameGroupHasSameSchedulePeriod(groupPerson, dates)).
                Return(true);
            Expect.Call(_groupPersonSameDayOffsChecker.CheckGroupPerson(dic, groupPerson, daysOffToRemove, daysOffToAdd))
                .Return(false);
            _mock.ReplayAll();
			Assert.That(_target.CheckPersonOnDates(dic, _person, daysOffToRemove, daysOffToAdd, allSelectedPersons, _schedulingOptions), Is.Null);
            _mock.VerifyAll();
        }

        [Test]
        public void ShouldReturnGroupPersonBuilderFromChecker()
        {
            Expect.Call(_groupPersonSameForPersonOnDateChecker.GroupPersonsBuilder).Return(null);
            _mock.ReplayAll();
            var ret =_target.GroupPersonBuilder;
            Assert.That(ret,Is.Null);
            _mock.VerifyAll();
        }
    }

    
}