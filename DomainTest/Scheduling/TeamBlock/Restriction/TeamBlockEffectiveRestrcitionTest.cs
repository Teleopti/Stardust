using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.Restriction
{
    [TestFixture]
    public class TeamBlockEffectiveRestrcitionTest
    {
        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _date = new DateOnly(2013, 10, 29);
            _schedulingOptions = new SchedulingOptions();
            _effectiveRestrcitionCreator = _mock.StrictMock<IEffectiveRestrictionCreator>();
			_personList = new List<IPerson>();
            _scheduleDictionary = _mock.StrictMock<IScheduleDictionary>();

			_target = new TeamBlockEffectiveRestriction(_effectiveRestrcitionCreator, _personList,
                                                        new SchedulingOptions(), _scheduleDictionary);
        }

        private MockRepository _mock;
        private IScheduleRestrictionStrategy _target;
        private IEffectiveRestrictionCreator _effectiveRestrcitionCreator;
        private IScheduleDictionary _scheduleDictionary;
        private SchedulingOptions _schedulingOptions;
        private DateOnly _date;
	    private List<IPerson> _personList;

	    [Test]
        public void ReturnExtractedRestriction()
        {
            var restriction = new EffectiveRestriction(new StartTimeLimitation(),
                                                       new EndTimeLimitation(),
                                                       new WorkTimeLimitation(), null, null, null,
                                                       new List<IActivityRestriction>());
            using (_mock.Record())
			{
				Expect.Call(_effectiveRestrcitionCreator.GetEffectiveRestriction(_personList, _date,
                                                                                 _schedulingOptions, _scheduleDictionary))
                      .IgnoreArguments().Return(restriction);
            }
            using (_mock.Playback())
            {
                Assert.AreEqual(_target.ExtractRestriction(new List<DateOnly> {_date}, new List<IScheduleMatrixPro>()),
                                restriction);
            }
        }

        [Test]
        public void ReturnExtractedRestrictionForTwoDays()
        {
            var restriction1 =
                new EffectiveRestriction(new StartTimeLimitation(new TimeSpan(0, 8, 0, 0), new TimeSpan(0, 9, 0, 0)),
                                         new EndTimeLimitation(),
                                         new WorkTimeLimitation(), null, null, null,
                                         new List<IActivityRestriction>());
            var restriction2 = new EffectiveRestriction(new StartTimeLimitation(),
                                                        new EndTimeLimitation(new TimeSpan(0, 10, 0, 0),
                                                                              new TimeSpan(0, 11, 0, 0)),
                                                        new WorkTimeLimitation(), null, null, null,
                                                        new List<IActivityRestriction>());
            var result =
                new EffectiveRestriction(new StartTimeLimitation(new TimeSpan(0, 8, 0, 0), new TimeSpan(0, 9, 0, 0)),
                                         new EndTimeLimitation(new TimeSpan(0, 10, 0, 0), new TimeSpan(0, 11, 0, 0)),
                                         new WorkTimeLimitation(), null, null, null,
                                         new List<IActivityRestriction>());
            using (_mock.Record())
            {
				Expect.Call(_effectiveRestrcitionCreator.GetEffectiveRestriction(_personList, _date,
                                                                                 _schedulingOptions, _scheduleDictionary))
                      .IgnoreArguments().Return(restriction1);
				Expect.Call(_effectiveRestrcitionCreator.GetEffectiveRestriction(_personList, _date.AddDays(1),
                                                                                 _schedulingOptions, _scheduleDictionary))
                      .IgnoreArguments().Return(restriction2);
            }
            using (_mock.Playback())
            {
                Assert.AreEqual(
                    _target.ExtractRestriction(new List<DateOnly> {_date, _date.AddDays(1)},
                                               new List<IScheduleMatrixPro>()), result);
            }
        }

        [Test]
        public void ReturnNullIfRestrictionIsNull()
        {

			var restriction = new EffectiveRestriction(new StartTimeLimitation(),
													   new EndTimeLimitation(),
													   new WorkTimeLimitation(), null, null, null,
													   new List<IActivityRestriction>());
            using (_mock.Record())
			{
				Expect.Call(_effectiveRestrcitionCreator.GetEffectiveRestriction(_personList, _date,
                                                                                 _schedulingOptions, _scheduleDictionary))
                      .IgnoreArguments().Return(null);
            }
            using (_mock.Playback())
            {
				Assert.That(_target.ExtractRestriction(new List<DateOnly> { _date }, new List<IScheduleMatrixPro>()), Is.EqualTo(restriction));
            }
        }
    }
}