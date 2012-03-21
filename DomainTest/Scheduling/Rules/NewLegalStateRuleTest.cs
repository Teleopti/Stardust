using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
    [TestFixture]
    public class NewLegalStateRuleTest
    {
        private MockRepository _mocks;
        private IList<IScheduleMatrixPro> _matrixList;
        private INewBusinessRule _rule;
        private Dictionary<IPerson, IScheduleRange> _dic;
        private IScheduleMatrixPro _scheduleMatrixPro;
        private IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
        private IScheduleMatrixListCreator _scheduleMatrixListCreator;
        private ISchedulingOptions _schedulingOptions;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
			_scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
            _matrixList = new List<IScheduleMatrixPro>{_scheduleMatrixPro};
			_workShiftMinMaxCalculator = _mocks.StrictMock<IWorkShiftMinMaxCalculator>();
			_scheduleMatrixListCreator = _mocks.StrictMock<IScheduleMatrixListCreator>();
            _schedulingOptions = _mocks.StrictMock<ISchedulingOptions>();
        }

        [Test]
        public void ShouldCreateNewLegalStateRule()
        {
            _rule = new NewLegalStateRule(_scheduleMatrixListCreator, _workShiftMinMaxCalculator, _schedulingOptions);
            
            Assert.IsNotNull(_rule);
            Assert.AreEqual("",_rule.ErrorMessage);
            Assert.IsTrue(_rule.HaltModify);
            Assert.IsFalse(_rule.IsMandatory);
        }

        [Test]
        public void ShouldReturnEmptyResponseListWhenInLegalState()
        {
            _rule = new NewLegalStateRule(_scheduleMatrixListCreator, _workShiftMinMaxCalculator, _schedulingOptions);

            var person = _mocks.StrictMock<IPerson>();
            var range = _mocks.StrictMock<IScheduleRange>();
            _dic = new Dictionary<IPerson, IScheduleRange> {{person, range}};
            var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
            var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
            var scheduleDays = new List<IScheduleDay> { scheduleDay1, scheduleDay2 };

            using(_mocks.Record())
            {
                Expect.Call(_scheduleMatrixListCreator.CreateMatrixListFromSchedulePartsAndAlternativeScheduleRanges(null, scheduleDays)).IgnoreArguments().Return(_matrixList);
                Expect.Call(_scheduleMatrixPro.Person).Return(person);
                Expect.Call(_dic[person].BusinessRuleResponseInternalCollection).IgnoreArguments().Return(
                    new List<IBusinessRuleResponse>());
                Expect.Call(_scheduleMatrixPro.EffectivePeriodDays).Return(
                    new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>()));
                Expect.Call(_workShiftMinMaxCalculator.IsPeriodInLegalState(_scheduleMatrixPro, _schedulingOptions)).Return(true);
            }

            using(_mocks.Playback())
            {
                var retList = _rule.Validate(_dic, scheduleDays);
                Assert.AreEqual(0, retList.Count());
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldReturnResponseForEverydayInPeriodWhenNotInLegalState()
        {
            _rule = new NewLegalStateRule(_scheduleMatrixListCreator, _workShiftMinMaxCalculator, _schedulingOptions);

            var person = _mocks.StrictMock<IPerson>();
            var range = _mocks.StrictMock<IScheduleRange>();
            _dic = new Dictionary<IPerson, IScheduleRange> {{person, range}};
            var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
            var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
            var scheduleDays = new List<IScheduleDay> { scheduleDay1, scheduleDay2 };
            var dateOnly1 = new DateOnly(2010, 1, 1);
            var dateOnly2 = new DateOnly(2010, 1, 2);
            ICccTimeZoneInfo timeZoneInfo = new CccTimeZoneInfo(TimeZoneInfo.Utc);
            var scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
            var permissionInformation = _mocks.StrictMock<IPermissionInformation>();

            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixListCreator.CreateMatrixListFromSchedulePartsAndAlternativeScheduleRanges(null, scheduleDays)).IgnoreArguments().Return(_matrixList);
                Expect.Call(_scheduleMatrixPro.Person).Return(person).Repeat.Any();
                Expect.Call(_dic[person].BusinessRuleResponseInternalCollection).IgnoreArguments().Return(
                    new List<IBusinessRuleResponse>());
                Expect.Call(_scheduleMatrixPro.EffectivePeriodDays).Return(
                    new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>{scheduleDayPro1, scheduleDayPro2})).Repeat.Twice();
                Expect.Call(_workShiftMinMaxCalculator.IsPeriodInLegalState(_scheduleMatrixPro, _schedulingOptions)).Return(false);
                Expect.Call(scheduleDayPro1.Day).Return(dateOnly1).Repeat.Twice();
                Expect.Call(scheduleDayPro2.Day).Return(dateOnly2).Repeat.Twice();
                Expect.Call(person.PermissionInformation).Return(permissionInformation).Repeat.AtLeastOnce();
                Expect.Call(permissionInformation.DefaultTimeZone()).Return(timeZoneInfo).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                IEnumerable<IBusinessRuleResponse> retList = _rule.Validate(_dic, scheduleDays);
                Assert.AreEqual(2, retList.Count());

                Assert.AreEqual(typeof(NewLegalStateRule), retList.ElementAt(0).TypeOfRule);
                Assert.AreEqual(typeof(NewLegalStateRule), retList.ElementAt(1).TypeOfRule);

                Assert.AreEqual(dateOnly1.Date, retList.ElementAt(0).Period.ToDateOnlyPeriod(timeZoneInfo).StartDate.Date);
                Assert.AreEqual(dateOnly2.Date, retList.ElementAt(1).Period.ToDateOnlyPeriod(timeZoneInfo).StartDate.Date);
            }       
        }
    }
}
