using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;


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
	        PublishedScheduleData data;
            using (_mocks.Record())
            {
				data = _publishedScheduleDataFactory.CreateMockedWithSwedishTimeZone(_period);
            }
            using (_mocks.Playback())
            {
				_target = new SchedulePublishedSpecificationForAbsence(null, _viewReason);
				Assert.IsFalse(_target.IsSatisfiedBy(data));
            }
        }


		[Test]
		public void ShouldScheduleDataOtherThatAbsenceMeansNotPublished()
		{
			PublishedScheduleData data;
			using (_mocks.Record())
			{
				data = _publishedScheduleDataFactory.CreateMockedNonAbsenceTypeData(_period);
			}
			using (_mocks.Playback())
			{
				_target = new SchedulePublishedSpecificationForAbsence(null, _viewReason);
				Assert.IsFalse(_target.IsSatisfiedBy(data));
			}
		}

        [Test]
		public void ShouldOutsidePreferencePeriodMeansNotPublished()
        {
			PublishedScheduleData data;
			using (_mocks.Record())
			{
				SetOuterPreferencePeriod();
				data = _publishedScheduleDataFactory.CreateMockedWithSwedishTimeZone(_period);
			}
			using (_mocks.Playback())
			{
				_target = new SchedulePublishedSpecificationForAbsence(_workflowControlSet, _viewReason);
				Assert.IsFalse(_target.IsSatisfiedBy(data));
			}
        }

		[Test]
		public void ShouldAbsenceStartInPreferencePeriodMeansPublished()
		{
			PublishedScheduleData data;
			var absencePeriodInPreferencePeriod = new DateOnlyPeriod(_periodStartDate.AddDays(1), _periodStartDate.AddDays(2)).ToDateTimePeriod(_timeZoneInfo);
			using (_mocks.Record())
			{
				SetInnerPreferencePeriod();
				data = _publishedScheduleDataFactory.CreateMockedWithSwedishTimeZone(_period);
				Expect.Call(_publishedScheduleDataFactory.MockedScheduleData.Period)
					.Return(absencePeriodInPreferencePeriod).Repeat.Any();
			}
			using (_mocks.Playback())
			{
				_target = new SchedulePublishedSpecificationForAbsence(_workflowControlSet, _viewReason);
				Assert.IsTrue(_target.IsSatisfiedBy(data));
			}
		}

		/// <remarks>
		/// Important and unique feature for fixing bug 23072
		/// </remarks>
		[Test]
		public void ShouldAbsenceStartBeforePreferencePeriodAlsoMeansPublished()
		{
			PublishedScheduleData data;
			var absencePeriodStartBeforePreferencePeriod = new DateOnlyPeriod(_periodStartDate.AddDays(-1), _periodStartDate.AddDays(1)).ToDateTimePeriod(_timeZoneInfo);
			using (_mocks.Record())
			{
				SetInnerPreferencePeriod();
				data = _publishedScheduleDataFactory.CreateMockedWithSwedishTimeZone(_period);
				Expect.Call(_publishedScheduleDataFactory.MockedScheduleData.Period)
					.Return(absencePeriodStartBeforePreferencePeriod).Repeat.Any();
			}
			using (_mocks.Playback())
			{
				_target = new SchedulePublishedSpecificationForAbsence(_workflowControlSet, _viewReason);
				Assert.IsTrue(_target.IsSatisfiedBy(data));
			}
		}

		/// <remarks>
		/// Important and unique feature for fixing bug 23072
		/// </remarks>
		[Test]
		public void ShouldAbsenceEndAfterPreferencePeriodAlsoMeansPublished()
		{
			PublishedScheduleData data;
			var absencePeriodEndAfterPreferencePeriod = new DateOnlyPeriod(_periodStartDate.AddDays(-1), _periodStartDate.AddDays(1)).ToDateTimePeriod(_timeZoneInfo);
			using (_mocks.Record())
			{
				SetInnerPreferencePeriod();
				data = _publishedScheduleDataFactory.CreateMockedWithSwedishTimeZone(_period);
				Expect.Call(_publishedScheduleDataFactory.MockedScheduleData.Period)
					.Return(absencePeriodEndAfterPreferencePeriod).Repeat.Any();
			}
			using (_mocks.Playback())
			{
				_target = new SchedulePublishedSpecificationForAbsence(_workflowControlSet, _viewReason);
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

		public PublishedScheduleData CreateMocked(TimeZoneInfo timeZoneInfo, IDateOnlyAsDateTimePeriod period)
		{
			MockedScheduleData = _mocker.Stub<IPersonAbsence>();
			TimeZoneInfo = timeZoneInfo;
			return new PublishedScheduleData(period,  MockedScheduleData, TimeZoneInfo);
			
		}

		public PublishedScheduleData CreateMockedNonAbsenceTypeData(IDateOnlyAsDateTimePeriod period)
		{
			MockedScheduleData = _mocker.Stub<IScheduleData>();
			TimeZoneInfo = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			return new PublishedScheduleData(period, MockedScheduleData, TimeZoneInfo);

		}

		public PublishedScheduleData CreateMockedWithSwedishTimeZone(IDateOnlyAsDateTimePeriod period)
		{
			return CreateMocked(TimeZoneInfoFactory.StockholmTimeZoneInfo(), period);
		}
	}
}
