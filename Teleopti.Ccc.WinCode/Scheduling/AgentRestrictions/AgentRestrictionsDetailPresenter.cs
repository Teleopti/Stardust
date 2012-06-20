using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions
{
	public class AgentRestrictionsDetailPresenter : SchedulePresenterBase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		private IAgentRestrictionsDetailView _view;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		private readonly IAgentRestrictionsDetailModel _model;

		public AgentRestrictionsDetailPresenter(IAgentRestrictionsDetailView view, IAgentRestrictionsDetailModel model, ISchedulerStateHolder schedulerState, IGridlockManager lockManager,
			ClipHandler<IScheduleDay> clipHandler, SchedulePartFilter schedulePartFilter,IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder, 
			IScheduleDayChangeCallback scheduleDayChangeCallback, IScheduleTag defaultScheduleTag)
			: base(view, schedulerState, lockManager, clipHandler, schedulePartFilter, overriddenBusinessRulesHolder, scheduleDayChangeCallback, defaultScheduleTag)
		{
			_view = view;
			_model = model;
		}

		public override int RowCount
		{
			get { return (_model.DetailData().Count + 7 - 1) / 7; }
		}

		public override int ColCount
		{
			get { return 7; }
		}

		public override void QueryCellInfo(object sender, Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs e)
		{
			if (e == null) return;
			if (e.RowIndex < 0 || e.ColIndex < 0) return;
			if (e.RowIndex == 0 && e.ColIndex == 0) return;

			if (e.RowIndex == 0 && e.ColIndex > 0)
			{
				e.Style.CellValue = "--Veckodag--";
				return;
			}

			if (e.RowIndex > 0 && e.ColIndex == 0)
			{
				e.Style.CellValue = "--VeckoNum--";
				return;
			}

			e.Style.CellValue = "--ScheduleDay--";		
		}
	}
}
