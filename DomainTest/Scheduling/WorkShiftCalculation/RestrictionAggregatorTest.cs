using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.WorkShiftCalculation
{
    [TestFixture]
    public class RestrictionAggregatorTest
    {
        private MockRepository _mocks;
        private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private ISchedulingOptions _schedulingOptions;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private IRestrictionAggregator _target;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _effectiveRestrictionCreator = _mocks.StrictMock<IEffectiveRestrictionCreator>();
            _schedulingOptions = _mocks.StrictMock<ISchedulingOptions>();
            _schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _target = new RestrictionAggregator(_effectiveRestrictionCreator,
                                                _schedulingResultStateHolder);
        }

        [Test]
        public void ShouldAggregateRestrictions()
        {
            var dateOnly = new DateOnly(2012, 12, 7);
            var dateList = new List<DateOnly> { dateOnly, dateOnly.AddDays(1) };
            var person1 = _mocks.StrictMock<IPerson>();
            var person2 = _mocks.StrictMock<IPerson>();
            var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            var groupPerson = _mocks.StrictMock<IGroupPerson>();
            var skillDay1 = _mocks.StrictMock<ISkillDay>();
            var skillDay2 = _mocks.StrictMock<ISkillDay>();
            var skillDays = new List<ISkillDay> { skillDay1, skillDay2 };
            var firstDay =
                new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(12)),
                                         new EndTimeLimitation(TimeSpan.FromHours(15), TimeSpan.FromHours(18)),
                                         new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
            var secondDay =
                new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(10), TimeSpan.FromHours(13)),
                                         new EndTimeLimitation(TimeSpan.FromHours(17), TimeSpan.FromHours(18)),
                                         new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
            var result =
                            new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(11), TimeSpan.FromHours(12)),
                                                     new EndTimeLimitation(TimeSpan.FromHours(17), TimeSpan.FromHours(17.5)),
                                                     new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

            using (_mocks.Record())
            {
                Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(dateList)).Return(skillDays);
                Expect.Call(skillDay1.OpenHours()).Return(new ReadOnlyCollection<TimePeriod>(new List<TimePeriod> { new TimePeriod(11, 0, 18, 0) }));
                Expect.Call(skillDay2.OpenHours()).Return(new ReadOnlyCollection<TimePeriod>(new List<TimePeriod> { new TimePeriod(10, 0, 17, 30) }));
                Expect.Call(groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> { person1, person2 })).Repeat.Twice();
                Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(new ReadOnlyCollection<IPerson>(new List<IPerson> { person1, person2 }), dateOnly,
                                                                                 _schedulingOptions, scheduleDictionary)).IgnoreArguments()
                    .Return(firstDay);
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(new ReadOnlyCollection<IPerson>(new List<IPerson> { person1, person2 }), dateOnly.AddDays(1),
                                                                                 _schedulingOptions, scheduleDictionary)).IgnoreArguments()
                    .Return(secondDay);
            }

            using (_mocks.Playback())
            {
                Assert.That(_target.Aggregate(dateList, groupPerson, _schedulingOptions), Is.EqualTo(result));
            }
        }

        [Test]
        public void ShouldReturnNullWhenNoGroupPerson()
        {
            var dateOnly = new DateOnly(2012, 12, 7);
            var dateList = new List<DateOnly> { dateOnly, dateOnly.AddDays(1) };

            Assert.That(_target.Aggregate(dateList, null, _schedulingOptions), Is.Null);
        }
    }
}
