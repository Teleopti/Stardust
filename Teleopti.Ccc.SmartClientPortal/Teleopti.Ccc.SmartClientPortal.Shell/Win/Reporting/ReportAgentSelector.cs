using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autofac;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.ExceptionHandling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Reporting
{
	public partial class ReportAgentSelector : BaseUserControl
	{
		private SchedulingScreenState _stateHolder;

		public ReportAgentSelector()
		{
			InitializeComponent();
		}

		public event EventHandler<EventArgs> OpenDialog;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (DesignMode || !StateHolderReader.IsInitialized) return;

			comboBoxAdv1.PopupContainer.Popup += popupContainerPopup;
			comboBoxAdv1.PopupContainer.CloseUp += popupContainerCloseUp;

			SetTexts();
			UpdateComboWithSelectedAgents();
		}

		void popupContainerCloseUp(object sender, Syncfusion.Windows.Forms.PopupClosedEventArgs e)
		{
			OpenDialog?.Invoke(this, EventArgs.Empty);

			try
			{
				Cursor = Cursors.WaitCursor;
				showFilterDialog();
				Cursor = Cursors.Default;
			}
			catch (DataSourceException dataSourceException)
			{
				using (var view =
					new SimpleExceptionHandlerView(dataSourceException, Resources.OpenReports, Resources.ServerUnavailable))
				{
					view.ShowDialog();
					Cursor = Cursors.Default;
				}
			}
		}

		void popupContainerPopup(object sender, EventArgs e)
		{
			comboBoxAdv1.PopupContainer.HidePopup();
		}

		public void SetStateHolder(SchedulingScreenState stateHolder)
		{
			_stateHolder = stateHolder;
		}

		public HashSet<Guid> SelectedPersonGuids { get; private set; } = new HashSet<Guid>();

		public void SetSelectedPersons(HashSet<Guid> persons)
		{
			SelectedPersonGuids = persons;
		}

		public string SelectedGroupPageKey { get; set; }

		private void showFilterDialog()
		{
			var all = _stateHolder.SchedulerStateHolder.ChoosenAgents.Select(p => p.Id.Value).ToList();

			using (var scheduleFilterView = new PersonsFilterView(_stateHolder.SchedulerStateHolder.RequestedPeriod.DateOnlyPeriod,
				_stateHolder.SchedulerStateHolder.FilteredCombinedAgentsDictionary,
				ComponentContext, ReportApplicationFunction, SelectedGroupPageKey, all, false))
			{
				scheduleFilterView.StartPosition = FormStartPosition.Manual;
				var pointToScreen =
					comboBoxAdv1.PointToScreen(new Point(comboBoxAdv1.Bounds.Y - 4, comboBoxAdv1.Bounds.Y + comboBoxAdv1.Height));
				scheduleFilterView.Location = pointToScreen;
				scheduleFilterView.AutoLocate();
				if (scheduleFilterView.ShowDialog() != DialogResult.OK) return;

				SelectedGroupPageKey = scheduleFilterView.SelectedGroupPageKey;
				SelectedPersonGuids = scheduleFilterView.SelectedAgentGuids();

				UpdateComboWithSelectedAgents();
			}
		}

		public void UpdateComboWithSelectedAgents()
		{
			var currentCultureInfo = TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;

			var builder = new StringBuilder();

			builder.Append(SelectedPersonGuids.Count.ToString(currentCultureInfo));
			builder.Append(":");
			_stateHolder.SchedulerStateHolder.FilterPersons(SelectedPersonGuids);
			foreach (var person in _stateHolder.SchedulerStateHolder.FilteredCombinedAgentsDictionary.Values)
			{
				builder.Append(person.Name);
				builder.Append(", ");
			}

			comboBoxAdv1.Text = builder.ToString();
		}

		public override bool HasHelp => false;

		public IComponentContext ComponentContext { get; set; }
		public IApplicationFunction ReportApplicationFunction { get; set; }
	}
}
