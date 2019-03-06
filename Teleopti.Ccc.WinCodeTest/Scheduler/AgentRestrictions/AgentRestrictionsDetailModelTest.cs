using System;
using System.Collections.Generic;
using NUnit.Framework;

using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.AgentRestrictions;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.RestrictionSummary;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.AgentRestrictions
{
	[TestFixture]
	public class AgentRestrictionsDetailModelTest
	{
		private AgentRestrictionsDetailModel _model;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private MockRepository _mocks;
		private DateTimePeriod _period;
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
			_schedulingOptions = new RestrictionSchedulingOptions {UseScheduling = true};
			_effectiveRestrictionExtractor = _mocks.StrictMock<IAgentRestrictionsDetailEffectiveRestrictionExtractor>();
			_preferenceNightRestChecker = _mocks.StrictMock<IPreferenceNightRestChecker>();
			_preferenceCellData = new PreferenceCellData();
		}

		[Test]
		public void ShouldLoadDetails()
		{
			var loadedPeriod = new DateTimePeriod(2012, 6, 28, 2012, 7, 4);
			IDictionary<DateOnly, IScheduleDayPro> dic = new Dictionary<DateOnly, IScheduleDayPro>();
			dic.Add(DateOnly.MinValue, null);
			dic.Add(DateOnly.MaxValue, null);

			using(_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro.OuterWeeksPeriodDictionary).Return(dic);
				Expect.Call(() =>_effectiveRestrictionExtractor.Extract(_scheduleMatrixPro, _preferenceCellData, new DateOnly(), loadedPeriod,TimeSpan.FromHours(40), null)).IgnoreArguments().Repeat.AtLeastOnce();
				Expect.Call(() => _preferenceNightRestChecker.CheckNightlyRest(null)).IgnoreArguments();
			}

			using(_mocks.Playback())
			{
				_model.LoadDetails(_scheduleMatrixPro, _schedulingOptions, _effectiveRestrictionExtractor, TimeSpan.FromHours(40), _preferenceNightRestChecker, null);
			}

			Assert.AreEqual(2, _model.DetailData().Count);
		}
	}
}
