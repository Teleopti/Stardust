using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common.Configuration;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using AlarmTypeRepository=Teleopti.Ccc.Infrastructure.Repositories.AlarmTypeRepository;

namespace Teleopti.Ccc.Win.Common.Configuration
{
	public partial class AlarmControl : BaseUserControl, ISettingPage
	{
		private IUnitOfWork _uow;
		private AlarmTypeRepository _alarmTypeRepository;
		private readonly List<IAlarmType> _alarmTypes = new List<IAlarmType>();
		private AlarmControlView _view;
		private int _selectedItem =-1;

		public AlarmControl()
		{
			InitializeComponent();
		}

		private  void setColors()
		{
			BackColor = ColorHelper.WizardBackgroundColor();
			gridControlAlarmTypes.BackColor = ColorHelper.GridControlGridInteriorColor();
			gridControlAlarmTypes.Properties.BackgroundColor = ColorHelper.WizardBackgroundColor();
			gradientPanel1.BackColor = ColorHelper.OptionsDialogHeaderBackColor();
			tableLayoutPanel6.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			label2.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
			labelTitle.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();
		}

		private void showGrid()
		{
			var backgroundWorkerLoadData = new BackgroundWorker();
			backgroundWorkerLoadData.RunWorkerCompleted += setupAlarmGrid;
			backgroundWorkerLoadData.RunWorkerAsync();
		}

		void setupAlarmGrid(object sender, RunWorkerCompletedEventArgs e)
		{
			_view = new AlarmControlView(gridControlAlarmTypes);
			_view.Presenter = new AlarmControlPresenter(_alarmTypes, _view);
			_view.PresentThisItem += viewPresentThisItem;
			_view.WarnOfThis += viewWarnOfThis;
			_view.LoadGrid();
		}

		void viewWarnOfThis(object sender, CustomEventArgs<string> e)
		{
		   warn(e.Value );
			gridControlAlarmTypes.Focus();
		}

		void viewPresentThisItem(object sender, CustomEventArgs<int> e)
		{
			if (e.Value < 0) return;
			_selectedItem = e.Value;
		}

		private void buttonNewClick(object sender, EventArgs e)
		{
			var atype = new AlarmType(new Description(Resources.GiveAlarmAName), Color.Red, new TimeSpan(0, 0, 0), AlarmTypeMode.UserDefined, 0.0);
			_alarmTypes.Add(atype);
			_view.LoadGrid();
		}

		private void buttonAdvDeleteClick(object sender, EventArgs e)
		{
			if (_selectedItem != -1)
			{
				if (ViewBase.ShowYesNoMessage(Resources.DeleteSelectedRowsQuestionmark, Resources.Alarm) ==
					DialogResult.Yes)
				{
					var item = _alarmTypes[_selectedItem];
					if(item.Id.HasValue)
						_alarmTypeRepository.Remove(item);
					_alarmTypes.Remove(item);
					_selectedItem = -1;
				}
			}
			gridControlAlarmTypes.Selections.Clear();

			_view.LoadGrid();
		}

		public void SaveChanges()
		{
			foreach (var type in _alarmTypes.Where(type => type.Id == null))
			{
				if(!alarmTypeValuesAreOk(type)) return;
				_alarmTypeRepository.Add(type);
			}
		}

		private bool alarmTypeValuesAreOk(IAlarmType type)
		{
			if (String.IsNullOrEmpty(type.Description.Name ))
			{
				warn(Resources.GiveAlarmAName);
				return false;
			}
		   if (type.DisplayColor  == Color.Empty )
			{
				warn(Resources.SelectSuitableColor);
				return false;
			}   
			if (type.ThresholdTime < TimeSpan.Zero)
			{
				warn(Resources.SetTresholdTime);
				return false;
			}
			return true;
		}

		public void Unload()
		{
			_alarmTypes.Clear();
		}

		public void SetUnitOfWork(IUnitOfWork value)
		{
			_uow = value;
		}

		public void Persist()
		{}

		public void InitializeDialogControl()
		{
			setColors();     
			SetTexts();
		}

		public void LoadControl()
		{
			loadAlarmTypes();
		}

		private void loadAlarmTypes()
		{
			_alarmTypeRepository = new AlarmTypeRepository(_uow);
			_alarmTypes.Clear();
			_alarmTypes.AddRange(_alarmTypeRepository.LoadAll());

			if (_view!=null)
			{
				_view.LoadGrid();
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			showGrid();
		}

		public TreeFamily TreeFamily()
		{
			return new TreeFamily(Resources.RealTimeAdherence, DefinedRaptorApplicationFunctionPaths.ManageRealTimeAdherence);
		}

		public string TreeNode()
		{
			return Resources.AlarmTypes;
		}

		public void OnShow()
		{
		}

		private void warn(string s)
		{
			var dialog = new TimedWarningDialog(1,s,gridControlAlarmTypes );
			dialog.Show();
		}

		public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
		{
			throw new NotImplementedException();
		}

		public ViewType ViewType
		{
			get { return ViewType.Alarm; }
		}
	}
}
