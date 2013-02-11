using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
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
	    private IOpenHoursToEffectiveRestrictionConverter _openHoursToRestrictionConverter;

	    [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _effectiveRestrictionCreator = _mocks.StrictMock<IEffectiveRestrictionCreator>();
            _schedulingOptions = _mocks.StrictMock<ISchedulingOptions>();
            _schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_openHoursToRestrictionConverter = _mocks.StrictMock<IOpenHoursToEffectiveRestrictionConverter>();
            _target = new RestrictionAggregator(_effectiveRestrictionCreator,
                                                _schedulingResultStateHolder,
												_openHoursToRestrictionConverter);
        }

	    [Test]
	    public void ShouldAggregateRestrictions()
	    {
		    var dateOnly = new DateOnly(2012, 12, 7);
		    var dateList = new List<DateOnly> {dateOnly, dateOnly.AddDays(1)};
		    var person1 = PersonFactory.CreatePerson("bill");
		    var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
		    var groupPerson = _mocks.StrictMock<IGroupPerson>();

		    var firstDay =
			    new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(12)),
			                             new EndTimeLimitation(TimeSpan.FromHours(15), TimeSpan.FromHours(18)),
			                             new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
		    var secondDay =
			    new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(10), TimeSpan.FromHours(13)),
			                             new EndTimeLimitation(TimeSpan.FromHours(17), TimeSpan.FromHours(18)),
			                             new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
		    var openHoursRestriction =
			    new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(11),null),
			                             new EndTimeLimitation(null, TimeSpan.FromHours(17.5)),
			                             new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
		    using (_mocks.Record())
		    {
			    Expect.Call(groupPerson.GroupMembers)
			          .Return(new ReadOnlyCollection<IPerson>(new List<IPerson> {person1}))
			          .Repeat.AtLeastOnce();
			    Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
			    Expect.Call(
				    _effectiveRestrictionCreator.GetEffectiveRestriction(
					    new ReadOnlyCollection<IPerson>(new List<IPerson> {person1}), dateOnly,
					    _schedulingOptions, scheduleDictionary)).IgnoreArguments()
			          .Return(firstDay);
			    Expect.Call(
				    _effectiveRestrictionCreator.GetEffectiveRestriction(
					    new ReadOnlyCollection<IPerson>(new List<IPerson> {person1}), dateOnly.AddDays(1),
					    _schedulingOptions, scheduleDictionary)).IgnoreArguments()
			          .Return(secondDay);
			    Expect.Call(_openHoursToRestrictionConverter.Convert(groupPerson, dateList))
			          .Return(openHoursRestriction);
		    }

		    using (_mocks.Playback())
		    {
			    var result = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(11), TimeSpan.FromHours(12)),
			                                          new EndTimeLimitation(TimeSpan.FromHours(17), TimeSpan.FromHours(17.5)),
			                                          new WorkTimeLimitation(), null, null, null,
			                                          new List<IActivityRestriction>());

			    var restriction = _target.Aggregate(dateList, groupPerson, _schedulingOptions);
			    Assert.That(restriction, Is.EqualTo(result));
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
