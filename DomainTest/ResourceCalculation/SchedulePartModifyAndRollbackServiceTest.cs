using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class SchedulePartModifyAndRollbackServiceTest
    {
        private ISchedulePartModifyAndRollbackService _target;
        private MockRepository _mocks;
        private ISchedulingResultStateHolder _stateHolder;
        private IScheduleDayChangeCallback _scheduleDayChangeCallback;
        private IScheduleTagSetter _tagSetter;
        private INewBusinessRuleCollection _businessRuleCollection; 

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _stateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _scheduleDayChangeCallback = _mocks.DynamicMock<IScheduleDayChangeCallback>();
            _tagSetter = new ScheduleTagSetter(NullScheduleTag.Instance);
            _businessRuleCollection = NewBusinessRuleCollection.Minimum();
            _target = new SchedulePartModifyAndRollbackService(_stateHolder,_scheduleDayChangeCallback, _tagSetter);
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyModify()
        {
            IScheduleDay schedulePart = _mocks.StrictMock<IScheduleDay>();
            IScheduleDictionary schedules = _mocks.StrictMock<IScheduleDictionary>();
            IPerson person = new Person();
            IScheduleDay partToSave = _mocks.StrictMock<IScheduleDay>();
            IScheduleRange range = _mocks.StrictMock<IScheduleRange>();
            DateTimePeriod period = new DateTimePeriod(new DateTime(2008, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2008, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            IList<IBusinessRuleResponse> validationList = new List<IBusinessRuleResponse>();
            var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(), new DateOnly());
            validationList.Add(new BusinessRuleResponse(typeof(string), "hej", true, false, period, person, dateOnlyPeriod));

            using (_mocks.Record())
            {
				Expect.Call(_stateHolder.AllPersonAccounts).Return(new Dictionary<IPerson, IPersonAccountCollection>());
                Expect.Call(_stateHolder.Schedules).Return(schedules).Repeat.AtLeastOnce();
                Expect.Call(schedules[person]).Return(range).IgnoreArguments().Repeat.AtLeastOnce();
                Expect.Call(range.ReFetch(schedulePart)).Return(partToSave);
                Expect.Call(schedules.Modify(ScheduleModifier.Scheduler, schedulePart, _businessRuleCollection, _scheduleDayChangeCallback, _tagSetter)).IgnoreArguments().Return(validationList).Repeat.AtLeastOnce();
                Expect.Call(schedulePart.Person).Return(person).Repeat.Any();
                Expect.Call(_stateHolder.TeamLeaderMode).Return(false).Repeat.Any();
                Expect.Call(_stateHolder.UseValidation).Return(true).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                _target.Modify(schedulePart);
            }

            Assert.AreEqual(1, _target.StackLength);
            Assert.AreEqual(1, _target.ModificationCollection.Count());

        }

        [Test]
        public void VerifyRollback()
        {
            IScheduleDay schedulePart = _mocks.StrictMock<IScheduleDay>();
            IScheduleDictionary schedules = _mocks.StrictMock<IScheduleDictionary>();
            IPerson person = _mocks.StrictMock<IPerson>();
            IScheduleDay partToSave = _mocks.StrictMock<IScheduleDay>();
            IScheduleRange range = _mocks.StrictMock<IScheduleRange>();
            DateTimePeriod period = new DateTimePeriod(new DateTime(2008, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2008, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            IList<IBusinessRuleResponse> validationList = new List<IBusinessRuleResponse>();
            var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(), new DateOnly());
            validationList.Add(new BusinessRuleResponse(typeof(string), "hej", true, false, period, person, dateOnlyPeriod));

            using (_mocks.Record())
            {
            	Expect.Call(_stateHolder.AllPersonAccounts).Return(new Dictionary<IPerson, IPersonAccountCollection>()).Repeat.AtLeastOnce();
                Expect.Call(_stateHolder.Schedules).Return(schedules).Repeat.AtLeastOnce();
                Expect.Call(schedulePart.Person).Return(person).Repeat.AtLeastOnce();
                Expect.Call(schedules[person]).Return(range).IgnoreArguments().Repeat.AtLeastOnce();
                Expect.Call(range.ReFetch(schedulePart)).Return(partToSave);
                Expect.Call(schedules.Modify(ScheduleModifier.Scheduler, schedulePart, null, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance))).Return(validationList).IgnoreArguments().Repeat.AtLeastOnce();
                Expect.Call(_stateHolder.TeamLeaderMode).Return(false).Repeat.Any();
                Expect.Call(_stateHolder.UseValidation).Return(true).Repeat.AtLeastOnce();
                //Expect.Call(person.Name).Return(new Name()).Repeat.Once();
                //Expect.Call(schedulePart.DateOnlyAsPeriod).Return((new DateOnlyAsDateTimePeriod(new DateOnly(2008, 1, 1), new CccTimeZoneInfo(TimeZoneInfo.Utc)))).Repeat.Once();
            }

            using (_mocks.Playback())
            {
                _target.Modify(schedulePart);
                _target.Rollback();
            }

            Assert.AreEqual(0, _target.StackLength);
            Assert.AreEqual(0, _target.ModificationCollection.Count());

        }


        [Test]
        public void VerifyClearModificationCollection()
        {
            IScheduleDay schedulePart = _mocks.StrictMock<IScheduleDay>();
            IScheduleDictionary schedules = _mocks.StrictMock<IScheduleDictionary>();
            IPerson person = _mocks.StrictMock<IPerson>();
            IScheduleDay partToSave = _mocks.StrictMock<IScheduleDay>();
            IScheduleRange range = _mocks.StrictMock<IScheduleRange>();
            DateTimePeriod period = new DateTimePeriod(new DateTime(2008, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2008, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            IList<IBusinessRuleResponse> validationList = new List<IBusinessRuleResponse>();
            var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(), new DateOnly());
            validationList.Add(new BusinessRuleResponse(typeof(string), "hej", true, false, period, person, dateOnlyPeriod));

            using (_mocks.Record())
            {
				Expect.Call(_stateHolder.AllPersonAccounts).Return(new Dictionary<IPerson, IPersonAccountCollection>());
                Expect.Call(_stateHolder.Schedules).Return(schedules).Repeat.AtLeastOnce();
                Expect.Call(schedulePart.Person).Return(person).Repeat.AtLeastOnce();
                Expect.Call(schedules[person]).Return(range).IgnoreArguments().Repeat.AtLeastOnce();
                Expect.Call(range.ReFetch(schedulePart)).Return(partToSave);
                Expect.Call(schedules.Modify(ScheduleModifier.Scheduler, schedulePart, null, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance))).Return(validationList).IgnoreArguments().Repeat.AtLeastOnce();
                Expect.Call(_stateHolder.TeamLeaderMode).Return(false).Repeat.Any();
                Expect.Call(_stateHolder.UseValidation).Return(true).Repeat.AtLeastOnce();
                //Expect.Call(person.Name).Return(new Name()).Repeat.Once();
                //Expect.Call(schedulePart.DateOnlyAsPeriod).Return((new DateOnlyAsDateTimePeriod(new DateOnly(2008, 1, 1), new CccTimeZoneInfo(TimeZoneInfo.Utc)))).Repeat.Once();
            }

            using (_mocks.Playback())
            {
                _target.Modify(schedulePart);
            }

            Assert.AreEqual(1, _target.StackLength);
            Assert.AreEqual(1, _target.ModificationCollection.Count());
            //check twice to ensure getting modifictioncollection does not empty the list
            Assert.AreEqual(1, _target.StackLength);
            Assert.AreEqual(1, _target.ModificationCollection.Count());

            _target.ClearModificationCollection();
            Assert.AreEqual(0, _target.StackLength);
            Assert.AreEqual(0, _target.ModificationCollection.Count());
        }
    }
}
