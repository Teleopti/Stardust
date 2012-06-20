using System;
using System.Linq;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary;
using Teleopti.Interfaces.Domain;
using Rhino.Mocks;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.AgentRestrictions
{
	[TestFixture]
	public class AgentRestrictionsDetailModelTest
	{
		private AgentRestrictionsDetailModel _model;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private MockRepository _mocks;
		private IVirtualSchedulePeriod _schedulePeriod;
		private DateTimePeriod _period;
		private IRestrictionExtractor _restrictionExtractor;
		private RestrictionSchedulingOptions _schedulingOptions;
		private IAgentRestrictionsDetailEffectiveRestrictionExtractor _effectiveRestrictionExtractor;
		private IPreferenceNightRestChecker _preferenceNightRestChecker;
		private PreferenceCellData _preferenceCellData;

		[SetUp]
		public void Setup()
		{
			_period = new DateTimePeriod(2012, 6, 1, 2012, 7, 10);
			_model = new AgentRestrictionsDetailModel(_period);
			_mocks = new MockRepository();
			_scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			_schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_restrictionExtractor = _mocks.StrictMock<IRestrictionExtractor>();
			_schedulingOptions = new RestrictionSchedulingOptions {UseScheduling = true};
			_effectiveRestrictionExtractor = _mocks.StrictMock<IAgentRestrictionsDetailEffectiveRestrictionExtractor>();
			_preferenceNightRestChecker = _mocks.StrictMock<IPreferenceNightRestChecker>();
			_preferenceCellData = new PreferenceCellData();
		}

		[Test]
		public void ShouldLoadDetails()
		{
			var dateOnlyPeriod = new DateOnlyPeriod(2012, 6, 28, 2012, 7, 4);
			var loadedPeriod = new DateTimePeriod(2012, 6, 28, 2012, 7, 4);

			using(_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_schedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(dateOnlyPeriod).Repeat.AtLeastOnce();
				Expect.Call(() =>_effectiveRestrictionExtractor.Extract(_scheduleMatrixPro, _preferenceCellData, new DateOnly(), loadedPeriod,TimeSpan.FromHours(40))).IgnoreArguments().Repeat.AtLeastOnce();
				Expect.Call(() => _preferenceNightRestChecker.CheckNightlyRest(null)).IgnoreArguments();
			}

			using(_mocks.Playback())
			{
				_model.LoadDetails(_scheduleMatrixPro, _restrictionExtractor, _schedulingOptions, _effectiveRestrictionExtractor, TimeSpan.FromHours(40), _preferenceNightRestChecker);
			}

			Assert.AreEqual(28, _model.DetailData().Count);
		}
		
		[Test]
		public void ShouldGetDetailDays()
		{
			var startDate = new DateTime(2012, 6, 28, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2012, 7, 4, 0, 0, 0, DateTimeKind.Utc);
			var detailDates = _model.DetailDates(startDate, endDate);
			Assert.AreEqual(28, detailDates.Count);
			Assert.AreEqual(new DateOnly(2012, 6, 18), detailDates.Min());
			Assert.AreEqual(new DateOnly(2012, 7, 15), detailDates.Max());
		}
	}
}
