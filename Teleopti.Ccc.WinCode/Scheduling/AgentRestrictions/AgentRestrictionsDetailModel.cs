using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public interface IAgentRestrictionsDetailModel
	{
		void LoadDetails(IScheduleMatrixPro scheduleMatrixPro);
		IList<DateOnly> DetailDates(DateTime startDate, DateTime endDate);
		Dictionary<int, IPreferenceCellData> DetailData();
	}

	public class AgentRestrictionsDetailModel : IAgentRestrictionsDetailModel
	{
		private Dictionary<int, IPreferenceCellData> _detailData;
		private DateTimePeriod _loadedPeriod;

		public AgentRestrictionsDetailModel(DateTimePeriod loadedPeriod)
		{
			_detailData = new Dictionary<int, IPreferenceCellData>();
			_loadedPeriod = loadedPeriod;
		}

		public void LoadDetails(IScheduleMatrixPro scheduleMatrixPro)
		{
			if(scheduleMatrixPro == null) throw new ArgumentNullException("scheduleMatrixPro");

			_detailData = new Dictionary<int, IPreferenceCellData>();
			var dates = DetailDates(scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod.StartDate.Date, scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod.EndDate.Date);

			var counter = 0;

			foreach (var dateOnly in dates)
			{
				var data = new PreferenceCellData();
				data.TheDate = dateOnly;
				data.SchedulePart = scheduleMatrixPro.GetScheduleDayByKey(dateOnly).DaySchedulePart();
				data.Enabled = _loadedPeriod.Contains(dateOnly) && scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod.Contains(dateOnly);

				_detailData.Add(counter, data);
				counter++;
			}
		}

		public Dictionary<int, IPreferenceCellData> DetailData()
		{
			return _detailData;
		}

		public IList<DateOnly> DetailDates(DateTime startDate, DateTime endDate)
		{
			var firstDateTime = DateHelper.GetFirstDateInWeek(startDate, TeleoptiPrincipal.Current.Regional.Culture);
			var lastDateTime = DateHelper.GetLastDateInWeek(endDate, TeleoptiPrincipal.Current.Regional.Culture);
			var start = new DateOnly(firstDateTime).AddDays(-7);
			var end = new DateOnly(lastDateTime).AddDays(7);
			var dateOnlyPeriod = new DateOnlyPeriod(start, end);
			return dateOnlyPeriod.DayCollection();
		}
	}
}
