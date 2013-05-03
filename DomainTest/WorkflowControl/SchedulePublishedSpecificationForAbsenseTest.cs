﻿using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
    [TestFixture]
    public class SchedulePublishedSpecificationForAbsenceTest
    {
		private SchedulePublishedSpecificationForAbsence _target;
        private MockRepository _mocks;
        private IWorkflowControlSet _workflowControlSet;
        private ScheduleVisibleReasons _viewReason;
	    private IDateOnlyAsDateTimePeriod _period;
	    private DateOnly _periodStartDate;
	    private TimeZoneInfo _timeZoneInfo;
	    private PublishedScheduleDataFactory _publishedScheduleDataFactory;


        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _workflowControlSet = _mocks.StrictMock<IWorkflowControlSet>();
            _viewReason = ScheduleVisibleReasons.Any;
			_timeZoneInfo = TimeZoneInfoFactory.StockholmTimeZoneInfo();
	        _periodStartDate = new DateOnly(new DateTime(2013, 07, 01, 0, 0, 0, DateTimeKind.Utc));
			_period = new DateOnlyAsDateTimePeriod(_periodStartDate, _timeZoneInfo);
			_publishedScheduleDataFactory = new PublishedScheduleDataFactory(_mocks);
		}

        [Test]
        public void ShouldNullWorkflowControlSetMeansNotPublished()
        {
	        PublishedScheduleData data = null;
            using (_mocks.Record())
            {
				data = _publishedScheduleDataFactory.CreateMockedWithSwedishTimeZone();
            }
            using (_mocks.Playback())
            {
				_target = new SchedulePublishedSpecificationForAbsence(null, _viewReason, _period);
				Assert.IsFalse(_target.IsSatisfiedBy(data));
            }
        }


		[Test]
		public void ShouldScheduleDataOtherThatAbsenceMeansNotPublished()
		{
			PublishedScheduleData data = null;
			using (_mocks.Record())
			{
				data = _publishedScheduleDataFactory.CreateMockedNonAbsenceTypeData();
			}
			using (_mocks.Playback())
			{
				_target = new SchedulePublishedSpecificationForAbsence(null, _viewReason, _period);
				Assert.IsFalse(_target.IsSatisfiedBy(data));
			}
		}

        [Test]
		public void ShouldOutsidePreferencePeriodMeansNotPublished()
        {
			PublishedScheduleData data = null;
			using (_mocks.Record())
			{
				SetOuterPreferencePeriod();
				data = _publishedScheduleDataFactory.CreateMockedWithSwedishTimeZone();
			}
			using (_mocks.Playback())
			{
				_target = new SchedulePublishedSpecificationForAbsence(_workflowControlSet, _viewReason, _period);
				Assert.IsFalse(_target.IsSatisfiedBy(data));
			}
        }

		[Test]
		public void ShouldAbsenceStartInPreferencePeriodMeansPublished()
		{
			PublishedScheduleData data = null;
			DateTimePeriod absencePeriodInPreferencePeriod =
				new DateTimePeriod(_periodStartDate.AddDays(1), _periodStartDate.AddDays(2));
			using (_mocks.Record())
			{
				SetInnerPreferencePeriod();
				data = _publishedScheduleDataFactory.CreateMockedWithSwedishTimeZone();
				Expect.Call(_publishedScheduleDataFactory.MockedScheduleData.Period)
					.Return(absencePeriodInPreferencePeriod).Repeat.Any();
			}
			using (_mocks.Playback())
			{
				_target = new SchedulePublishedSpecificationForAbsence(_workflowControlSet, _viewReason, _period);
				Assert.IsTrue(_target.IsSatisfiedBy(data));
			}
		}

		/// <remarks>
		/// Important and unique feature for fixing bug 23072
		/// </remarks>
		[Test]
		public void ShouldAbsenceStartBeforePreferencePeriodAlsoMeansPublished()
		{
			PublishedScheduleData data = null;
			DateTimePeriod absencePeriodStartBeforePreferencePeriod =
				new DateTimePeriod(_periodStartDate.AddDays(-1), _periodStartDate.AddDays(1));
			using (_mocks.Record())
			{
				SetInnerPreferencePeriod();
				data = _publishedScheduleDataFactory.CreateMockedWithSwedishTimeZone();
				Expect.Call(_publishedScheduleDataFactory.MockedScheduleData.Period)
					.Return(absencePeriodStartBeforePreferencePeriod).Repeat.Any();
			}
			using (_mocks.Playback())
			{
				_target = new SchedulePublishedSpecificationForAbsence(_workflowControlSet, _viewReason, _period);
				Assert.IsTrue(_target.IsSatisfiedBy(data));
			}
		}

		/// <remarks>
		/// Important and unique feature for fixing bug 23072
		/// </remarks>
		[Test]
		public void ShouldAbsenceEndAfterPreferencePeriodAlsoMeansPublished()
		{
			PublishedScheduleData data = null;
			DateTimePeriod absencePeriodEndAfterPreferencePeriod =
				new DateTimePeriod(_periodStartDate.AddDays(-1), _periodStartDate.AddDays(1));
			using (_mocks.Record())
			{
				SetInnerPreferencePeriod();
				data = _publishedScheduleDataFactory.CreateMockedWithSwedishTimeZone();
				Expect.Call(_publishedScheduleDataFactory.MockedScheduleData.Period)
					.Return(absencePeriodEndAfterPreferencePeriod).Repeat.Any();
			}
			using (_mocks.Playback())
			{
				_target = new SchedulePublishedSpecificationForAbsence(_workflowControlSet, _viewReason, _period);
				Assert.IsTrue(_target.IsSatisfiedBy(data));
			}
		}

		private void SetInnerPreferencePeriod()
		{
			// preferencePeriod is juni30 -> july 2
			DateOnlyPeriod preferencePeriod = new DateOnlyPeriod(_periodStartDate.AddDays(-1), _periodStartDate.AddDays(1));
			Expect.Call(_workflowControlSet.PreferencePeriod)
				.Return(preferencePeriod).Repeat.Any();
			DateOnlyPeriod preferenceInputPeriod = new DateOnlyPeriod(DateOnly.Today.AddDays(-1), DateOnly.Today.AddDays(1));
			Expect.Call(_workflowControlSet.PreferenceInputPeriod)
				.Return(preferenceInputPeriod).Repeat.Any();
		}

		private void SetOuterPreferencePeriod()
		{
			// preferencePeriod is july 2 -> july 3
			DateOnlyPeriod preferencePeriod = new DateOnlyPeriod(_periodStartDate.AddDays(1), _periodStartDate.AddDays(2));
			Expect.Call(_workflowControlSet.PreferencePeriod)
				.Return(preferencePeriod).Repeat.Any();
		}
	}

	internal class PublishedScheduleDataFactory
	{
		private readonly MockRepository _mocker;

		public IScheduleData MockedScheduleData { get; private set; }
		public TimeZoneInfo TimeZoneInfo { get; private set; }

		public PublishedScheduleDataFactory(MockRepository mocker)
		{
			_mocker = mocker;
		}

		public PublishedScheduleData CreateMocked(TimeZoneInfo timeZoneInfo)
		{
			MockedScheduleData = _mocker.Stub<IPersonAbsence>();
			TimeZoneInfo = timeZoneInfo;
			return new PublishedScheduleData(MockedScheduleData, TimeZoneInfo);
			
		}

		public PublishedScheduleData CreateMockedNonAbsenceTypeData()
		{
			MockedScheduleData = _mocker.Stub<IScheduleData>();
			TimeZoneInfo = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			return new PublishedScheduleData(MockedScheduleData, TimeZoneInfo);

		}

		public PublishedScheduleData CreateMockedWithSwedishTimeZone()
		{
			return CreateMocked(TimeZoneInfoFactory.StockholmTimeZoneInfo());
		}
	}
}
