using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.RestrictionSummary;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.AgentRestrictions
{
	public interface IAgentRestrictionsDetailModel
	{
		void LoadDetails(IScheduleMatrixPro scheduleMatrixPro, RestrictionSchedulingOptions schedulingOptions,  IAgentRestrictionsDetailEffectiveRestrictionExtractor effectiveRestrictionExtractor, TimeSpan periodTarget, IPreferenceNightRestChecker preferenceNightRestChecker);
		Dictionary<int, IPreferenceCellData> DetailData();
	}

	public class AgentRestrictionsDetailModel : IAgentRestrictionsDetailModel
	{
		private Dictionary<int, IPreferenceCellData> _detailData;
		private readonly DateTimePeriod _loadedPeriod;
		private readonly object _lock = new object();

		public AgentRestrictionsDetailModel(DateTimePeriod loadedPeriod)
		{
			_detailData = new Dictionary<int, IPreferenceCellData>();
			_loadedPeriod = loadedPeriod;
		}

		public void LoadDetails(IScheduleMatrixPro scheduleMatrixPro, RestrictionSchedulingOptions schedulingOptions, IAgentRestrictionsDetailEffectiveRestrictionExtractor effectiveRestrictionExtractor, TimeSpan periodTarget, IPreferenceNightRestChecker preferenceNightRestChecker)
		{
			lock (_lock)
			{
				_detailData = new Dictionary<int, IPreferenceCellData>();

				var counter = 0;

				foreach (var dateOnly in scheduleMatrixPro.OuterWeeksPeriodDictionary.Keys)
				{
					var data = new PreferenceCellData();
					effectiveRestrictionExtractor.Extract(scheduleMatrixPro, data, dateOnly, _loadedPeriod, periodTarget);
					_detailData.Add(counter, data);

					counter++;
				}

				preferenceNightRestChecker.CheckNightlyRest(_detailData);
			}
		}

		public Dictionary<int, IPreferenceCellData> DetailData()
		{
			return _detailData;
		}
	}
}
