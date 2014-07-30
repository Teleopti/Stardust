using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autofac;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.ExceptionHandling;
using Teleopti.Ccc.Win.Scheduling;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Reporting
{
	public partial class ReportAgentSelector : BaseUserControl
	{
		private HashSet<Guid> _selectedPersonGuids = new HashSet<Guid>();
		private SchedulerStateHolder _stateHolder;
		private string _selectedGroupPageKey;
		
		public ReportAgentSelector()
		{
			InitializeComponent();
		}

		public event EventHandler<EventArgs> OpenDialog;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
		   
			if (!DesignMode && StateHolderReader.IsInitialized)
			{
				comboBoxAdv1.PopupContainer.Popup += popupContainerPopup;
				comboBoxAdv1.PopupContainer.CloseUp += popupContainerCloseUp;


				SetTexts();

				UpdateComboWithSelectedAgents();
			}     
		}

		void popupContainerCloseUp(object sender, Syncfusion.Windows.Forms.PopupClosedEventArgs e)
		{
			var handler = OpenDialog;
			if(handler != null)
				handler(this, EventArgs.Empty);

			try
			{
				Cursor = Cursors.WaitCursor;
				showFilterDialog();
				Cursor = Cursors.Default;
			}
			catch (DataSourceException dataSourceException)
			{
				using (var view = new SimpleExceptionHandlerView(dataSourceException, Resources.OpenReports, Resources.ServerUnavailable))
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

		public void SetStateHolder(SchedulerStateHolder stateHolder)
		{
			_stateHolder = stateHolder;
		}

		public HashSet<Guid> SelectedPersonGuids
		{
			get { return _selectedPersonGuids; }
		}

		public void SetSelectedPersons(HashSet<Guid> persons)
		{
			_selectedPersonGuids = persons;
		}

		public string SelectedGroupPageKey
		{
			get { return _selectedGroupPageKey; }
			set { _selectedGroupPageKey = value; }
		}

		private void showFilterDialog()
		{
			var all = _stateHolder.AllPermittedPersons.Select(p => p.Id.Value).ToList();

			using (var scheduleFilterView = new PersonsFilterView(_stateHolder.RequestedPeriod.DateOnlyPeriod, _stateHolder.FilteredPersonDictionary.Keys,
				ComponentContext,ReportApplicationFunction, _selectedGroupPageKey, all))
			{
				scheduleFilterView.StartPosition = FormStartPosition.Manual;
				Point pointToScreen =
					comboBoxAdv1.PointToScreen(new Point(comboBoxAdv1.Bounds.Y - 4,
															  comboBoxAdv1.Bounds.Y + comboBoxAdv1.Height));
				scheduleFilterView.Location = pointToScreen;
				scheduleFilterView.AutoLocate();
				if (scheduleFilterView.ShowDialog() == DialogResult.OK)
				{
					_selectedGroupPageKey = scheduleFilterView.SelectedGroupPageKey;
					_selectedPersonGuids = scheduleFilterView.SelectedAgentGuids();
					
					UpdateComboWithSelectedAgents();
				}
			}
		}

		public void UpdateComboWithSelectedAgents()
		{
			var currentCultureInfo = TeleoptiPrincipal.Current.Regional.Culture;

			var builder = new StringBuilder();

			builder.Append(_selectedPersonGuids.Count().ToString(currentCultureInfo));
			builder.Append(":");
			_stateHolder.FilterPersons(_selectedPersonGuids);
			foreach (var person in _stateHolder.FilteredPersonDictionary)
			{
				builder.Append(person.Value.Name);
				builder.Append(", ");
			}

			comboBoxAdv1.Text = builder.ToString();
		}

		public override bool HasHelp
		{
			get
			{
				return false;
			}
		}

		public IComponentContext ComponentContext { get; set; }
		public IApplicationFunction ReportApplicationFunction { get; set; }
	}
}
