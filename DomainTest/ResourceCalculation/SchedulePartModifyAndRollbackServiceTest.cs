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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "hej"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.Scheduling.Rules.BusinessRuleResponse.#ctor(System.Type,System.String,System.Boolean,System.Boolean,Teleopti.Interfaces.Domain.DateTimePeriod,Teleopti.Interfaces.Domain.IPerson,Teleopti.Interfaces.Domain.DateOnlyPeriod)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldModifyWithSpecifiedRules()
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
			var rules = NewBusinessRuleCollection.Minimum();

			using (_mocks.Record())
			{
				Expect.Call(_stateHolder.Schedules).Return(schedules).Repeat.AtLeastOnce();
				Expect.Call(schedules[person]).Return(range).IgnoreArguments().Repeat.AtLeastOnce();
				Expect.Call(range.ReFetch(schedulePart)).Return(partToSave);
				Expect.Call(schedules.Modify(ScheduleModifier.Scheduler, schedulePart, rules, _scheduleDayChangeCallback, _tagSetter)).IgnoreArguments().Return(validationList).Repeat.AtLeastOnce();
				Expect.Call(schedulePart.Person).Return(person).Repeat.Any();
				Expect.Call(_stateHolder.TeamLeaderMode).Return(false).Repeat.Any();
			}

			using (_mocks.Playback())
			{
				_target.Modify(schedulePart, rules);
			}

			Assert.AreEqual(1, _target.StackLength);
			Assert.AreEqual(1, _target.ModificationCollection.Count());

		}
		
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "hej"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.Scheduling.Rules.BusinessRuleResponse.#ctor(System.Type,System.String,System.Boolean,System.Boolean,Teleopti.Interfaces.Domain.DateTimePeriod,Teleopti.Interfaces.Domain.IPerson,Teleopti.Interfaces.Domain.DateOnlyPeriod)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldModifyStrictlyWithTagAndSpecifiedRules()
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
			var rules = NewBusinessRuleCollection.Minimum();

			using (_mocks.Record())
			{
				Expect.Call(_stateHolder.Schedules).Return(schedules).Repeat.AtLeastOnce();
				Expect.Call(schedules[person]).Return(range).IgnoreArguments().Repeat.AtLeastOnce();
				Expect.Call(range.ReFetch(schedulePart)).Return(partToSave);
				Expect.Call(schedules.Modify(ScheduleModifier.Scheduler, schedulePart, rules, _scheduleDayChangeCallback, _tagSetter)).IgnoreArguments().Return(validationList).Repeat.AtLeastOnce();
				Expect.Call(schedulePart.Person).Return(person).Repeat.Any();
				Expect.Call(_stateHolder.TeamLeaderMode).Return(false).Repeat.Any();
				Expect.Call(_stateHolder.UseValidation).Return(false);
				Expect.Call(_stateHolder.AllPersonAccounts).Return(new Dictionary<IPerson, IPersonAccountCollection>());
			}

			using (_mocks.Playback())
			{
				_target.ModifyStrictly(schedulePart,_tagSetter, rules);
			}

			Assert.AreEqual(0, _target.StackLength);
			Assert.AreEqual(0, _target.ModificationCollection.Count());

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
                //Expect.Call(schedulePart.DateOnlyAsPeriod).Return((new DateOnlyAsDateTimePeriod(new DateOnly(2008, 1, 1), (TimeZoneInfo.Utc)))).Repeat.Once();
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
                //Expect.Call(schedulePart.DateOnlyAsPeriod).Return((new DateOnlyAsDateTimePeriod(new DateOnly(2008, 1, 1), (TimeZoneInfo.Utc)))).Repeat.Once();
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "hej"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.Scheduling.Rules.BusinessRuleResponse.#ctor(System.Type,System.String,System.Boolean,System.Boolean,Teleopti.Interfaces.Domain.DateTimePeriod,Teleopti.Interfaces.Domain.IPerson,Teleopti.Interfaces.Domain.DateOnlyPeriod)"), Test]
		public void ShouldModifyBothAtTheSameTimeToAvoidTemporaryBrokenBusinessRules()
		{
			var schedulePart = _mocks.StrictMock<IScheduleDay>();
			var schedulePart2 = _mocks.StrictMock<IScheduleDay>();
			var schedules = _mocks.StrictMock<IScheduleDictionary>();
			IPerson person = new Person();
			var partToSave = _mocks.StrictMock<IScheduleDay>();
			var partToSave2 = _mocks.StrictMock<IScheduleDay>();
			var period = new DateTimePeriod(new DateTime(2008, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2008, 1, 1, 0, 0, 0, DateTimeKind.Utc));
			IList<IBusinessRuleResponse> validationList = new List<IBusinessRuleResponse>();
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(), new DateOnly());
			validationList.Add(new BusinessRuleResponse(typeof(string), "hej", true, false, period, person, dateOnlyPeriod));
			validationList.Add(new BusinessRuleResponse(typeof(string), "hej", true, false, period, person, dateOnlyPeriod));

			Expect.Call(_stateHolder.AllPersonAccounts).Return(new Dictionary<IPerson, IPersonAccountCollection>());
			Expect.Call(_stateHolder.Schedules).Return(schedules).Repeat.AtLeastOnce();
			Expect.Call(schedulePart.ReFetch()).Return(partToSave);
			Expect.Call(schedulePart2.ReFetch()).Return(partToSave2);
			Expect.Call(schedules.Modify(ScheduleModifier.Scheduler, new List<IScheduleDay> {schedulePart, schedulePart2},
				_businessRuleCollection, _scheduleDayChangeCallback, _tagSetter))
				.IgnoreArguments()
				.Return(validationList);
			Expect.Call(_stateHolder.UseValidation).Return(true).Repeat.AtLeastOnce();
			Expect.Call(_stateHolder.TeamLeaderMode).Return(false).Repeat.Any();
			_mocks.ReplayAll();

			var result = _target.ModifyParts(new List<IScheduleDay>{schedulePart, schedulePart2} );
			
			Assert.AreEqual(2, _target.StackLength);
			Assert.AreEqual(2, _target.ModificationCollection.Count());
			Assert.AreEqual(1, result.Count());

			_mocks.VerifyAll();
		}
    }
}
