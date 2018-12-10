using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;


namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
	[TestFixture]
	public class EffectiveRestrictionCreatorTest
	{
		private IEffectiveRestrictionCreator _target;
		private IRestrictionExtractor _extractor;
		private MockRepository _mocks;
		private IScheduleDay _scheduleDay;
		private SchedulingOptions _options;
		private IExtractedRestrictionResult _extractorResult;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_extractor = _mocks.StrictMock<IRestrictionExtractor>();
			_extractorResult = _mocks.StrictMock<IExtractedRestrictionResult>();
			_target = new EffectiveRestrictionCreator(_extractor);
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
		}

		[Test]
		public void SimpleTest()
		{
			var options = new SchedulingOptions();
			IEffectiveRestriction expected = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
			                                                          new WorkTimeLimitation(), null, null, null,
			                                                          new List<IActivityRestriction>());

			using (_mocks.Record())
			{
				Expect.Call(_extractor.Extract(_scheduleDay)).Return(_extractorResult);
				Expect.Call(_extractorResult.CombinedRestriction(options)).Return(expected);
			}

			IEffectiveRestriction ret;
			using (_mocks.Playback())
			{
				ret = _target.GetEffectiveRestriction(_scheduleDay, options);
			}

			Assert.AreEqual(expected, ret);
		}

		[Test]
		public void WhenUseAvailabilityAndIsAvailabilityDayAndNotAvailableTargetShouldHaveDayOff()
		{
			var options = new SchedulingOptions();
			options.UseAvailability = true;
			IDayOffTemplate dayOffTemplate = new DayOffTemplate(new Description());
			options.DayOffTemplate = dayOffTemplate;
			IEffectiveRestriction fromExtractor = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
																	  new WorkTimeLimitation(), null, null, null,
																	  new List<IActivityRestriction>());
			fromExtractor.IsAvailabilityDay = true;
			fromExtractor.NotAvailable = true;

			IEffectiveRestriction expected = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
																	  new WorkTimeLimitation(), null, dayOffTemplate, null,
																	  new List<IActivityRestriction>());

			using (_mocks.Record())
			{
				Expect.Call(_extractor.Extract(_scheduleDay)).Return(_extractorResult);
				Expect.Call(_extractorResult.CombinedRestriction(options)).Return(fromExtractor);
			}

			IEffectiveRestriction ret;
			using (_mocks.Playback())
			{
				ret = _target.GetEffectiveRestriction(_scheduleDay, options);
			}

			Assert.AreEqual(expected.DayOffTemplate, ret.DayOffTemplate);
		}

		[Test]
		public void WhenUseHourlyAvailabilityAndNotAvailableTargetShouldHaveDayOff()
		{
			_target = new EffectiveRestrictionCreator(_extractor);
			var options = new SchedulingOptions();
			options.UseAvailability = false;
			options.UseStudentAvailability = true;
			IDayOffTemplate dayOffTemplate = new DayOffTemplate(new Description("Test"));
			options.DayOffTemplate = dayOffTemplate;
			IEffectiveRestriction fromExtractor = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
																	  new WorkTimeLimitation(), null, null, null,
																	  new List<IActivityRestriction>());
			fromExtractor.NotAvailable = true;

			IEffectiveRestriction expected = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
																	  new WorkTimeLimitation(), null, dayOffTemplate, null,
																	  new List<IActivityRestriction>());

			using (_mocks.Record())
			{
				Expect.Call(_extractor.Extract(_scheduleDay)).Return(_extractorResult);
				Expect.Call(_extractorResult.CombinedRestriction(options)).Return(fromExtractor);
			}

			IEffectiveRestriction ret;
			using (_mocks.Playback())
			{
				ret = _target.GetEffectiveRestriction(_scheduleDay, options);
			}

			Assert.AreEqual(expected.DayOffTemplate, ret.DayOffTemplate);
		}

		[Test]
		public void VerifyEffectiveRestrictionOnGroupOfPeople()
		{
			var dateOnly = new DateOnly(2010, 1, 1);
			var person1 = _mocks.StrictMock<IPerson>();
			var person2 = _mocks.StrictMock<IPerson>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			var range1 = _mocks.StrictMock<IScheduleRange>();
			var range2 = _mocks.StrictMock<IScheduleRange>();
			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var options = new SchedulingOptions();
			IEffectiveRestriction restriction1 = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)), new EndTimeLimitation(),
																	  new WorkTimeLimitation(), null, null, null,
																	  new List<IActivityRestriction>());
			IEffectiveRestriction restriction2 = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(TimeSpan.FromHours(18), TimeSpan.FromHours(18)),
																	  new WorkTimeLimitation(), null, null, null,
																	  new List<IActivityRestriction>());
			IEffectiveRestriction expected =
				new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)),
				                         new EndTimeLimitation(TimeSpan.FromHours(18), TimeSpan.FromHours(18)),
				                         new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

			using (_mocks.Record())
			{
				Expect.Call(scheduleDictionary[person1]).Return(range1);
				Expect.Call(range1.ScheduledDay(dateOnly)).Return(_scheduleDay);
				Expect.Call(_extractor.Extract(_scheduleDay)).Return(_extractorResult);
				Expect.Call(_extractorResult.CombinedRestriction(options)).Return(restriction1);

				Expect.Call(scheduleDictionary[person2]).Return(range2);
				Expect.Call(range2.ScheduledDay(dateOnly)).Return(scheduleDay2);
				Expect.Call(_extractor.Extract(scheduleDay2)).Return(_extractorResult);
				Expect.Call(_extractorResult.CombinedRestriction(options)).Return(restriction2);
			}

			IEffectiveRestriction ret;
			using (_mocks.Playback())
			{
				ret = _target.GetEffectiveRestriction(new List<IPerson>{person1, person2}, dateOnly, options, scheduleDictionary);
			}

			Assert.AreEqual(expected, ret);
		}

		[Test]
		public void ShouldReturnNullIfConflictingRestrictionsOnGroupOfPeople()
		{
			var cat1 = new ShiftCategory("cattis");
			var cat2 = new ShiftCategory("cattis2");

			var dateOnly = new DateOnly(2010, 1, 1);
			var person1 = _mocks.StrictMock<IPerson>();
			var person2 = _mocks.StrictMock<IPerson>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			var range1 = _mocks.StrictMock<IScheduleRange>();
			var range2 = _mocks.StrictMock<IScheduleRange>();
			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var options = new SchedulingOptions();
			IEffectiveRestriction restriction1 = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
																	  new WorkTimeLimitation(), cat1, null, null,
																	  new List<IActivityRestriction>());
			IEffectiveRestriction restriction2 = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
																	  new WorkTimeLimitation(), cat2, null, null,
																	  new List<IActivityRestriction>());
			using (_mocks.Record())
			{
				Expect.Call(scheduleDictionary[person1]).Return(range1);
				Expect.Call(range1.ScheduledDay(dateOnly)).Return(_scheduleDay);
				Expect.Call(_extractor.Extract(_scheduleDay)).Return(_extractorResult);
				Expect.Call(_extractorResult.CombinedRestriction(options)).Return(restriction1);

				Expect.Call(scheduleDictionary[person2]).Return(range2);
				Expect.Call(range2.ScheduledDay(dateOnly)).Return(scheduleDay2);
				Expect.Call(_extractor.Extract(scheduleDay2)).Return(_extractorResult);
				Expect.Call(_extractorResult.CombinedRestriction(options)).Return(restriction2);

			}

			IEffectiveRestriction ret;
			using (_mocks.Playback())
			{
				ret = _target.GetEffectiveRestriction(new List<IPerson> { person1, person2 }, dateOnly, options, scheduleDictionary);
			}

			Assert.IsNull(ret);
		}

		[Test]
		public void ShouldReturnFalseIfOptionsOrEffectiveRestrictionIsNull()
		{
			_options = new SchedulingOptions { AvailabilityDaysOnly = true, UseAvailability = true };
			var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
																				  new EndTimeLimitation(),
																				  new WorkTimeLimitation()
																				  , null, null, null,
																				  new List<IActivityRestriction>()) { IsAvailabilityDay = false };

			Assert.IsFalse(EffectiveRestrictionCreator.OptionsConflictWithRestrictions(_options,null));
			Assert.IsFalse(EffectiveRestrictionCreator.OptionsConflictWithRestrictions(null,effectiveRestriction));
		}

		[Test]
		public void VerifyOptionsConflictWithRestrictions()
		{
			_options = new SchedulingOptions {AvailabilityDaysOnly = true, UseAvailability = true};
			var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
																				  new EndTimeLimitation(),
																				  new WorkTimeLimitation()
																				  , null, null, null,
																				  new List<IActivityRestriction>()) {IsAvailabilityDay = false};
			Assert.IsTrue(EffectiveRestrictionCreator.OptionsConflictWithRestrictions(_options, effectiveRestriction));

			_options.AvailabilityDaysOnly = false;
			Assert.IsFalse(EffectiveRestrictionCreator.OptionsConflictWithRestrictions(_options, effectiveRestriction));
			_options.RotationDaysOnly = true;
			effectiveRestriction.IsRotationDay = false;
			_options.UseRotations = true;
			Assert.IsTrue(EffectiveRestrictionCreator.OptionsConflictWithRestrictions(_options, effectiveRestriction));

			_options.RotationDaysOnly = false;
			Assert.IsFalse(EffectiveRestrictionCreator.OptionsConflictWithRestrictions(_options, effectiveRestriction));
			_options.PreferencesDaysOnly = true;
			effectiveRestriction.IsPreferenceDay = false;
			_options.UsePreferences = true;
			Assert.IsTrue(EffectiveRestrictionCreator.OptionsConflictWithRestrictions(_options, effectiveRestriction));

		}

		[Test]
		public void VerifyThatTherestrictionAreCreatedForSinglePerson()
		{
			var dateOnly = new DateOnly(2010, 1, 1);
			var person1 = _mocks.StrictMock<IPerson>();
			var range1 = _mocks.StrictMock<IScheduleRange>();
			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var options = new SchedulingOptions();
			IEffectiveRestriction restriction1 = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8)), new EndTimeLimitation(),
																	  new WorkTimeLimitation(), null, null, null,
																	  new List<IActivityRestriction>());

			using (_mocks.Record())
			{
				Expect.Call(scheduleDictionary[person1]).Return(range1);
				Expect.Call(range1.ScheduledDay(dateOnly)).Return(_scheduleDay);
				Expect.Call(_extractor.Extract(_scheduleDay)).Return(_extractorResult);
				Expect.Call(_extractorResult.CombinedRestriction(options)).Return(restriction1);
			}

			IEffectiveRestriction ret;
			using (_mocks.Playback())
			{
				ret = _target.GetEffectiveRestrictionForSinglePerson(person1, dateOnly, options, scheduleDictionary);
			}

			Assert.AreEqual(restriction1, ret);

		}
	}
}