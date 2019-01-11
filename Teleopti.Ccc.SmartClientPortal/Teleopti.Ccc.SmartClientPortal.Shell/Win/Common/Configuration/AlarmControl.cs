using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.UserTexts;
using Teleopti.Wfm.Adherence.Configuration;
using Teleopti.Wfm.Adherence.Configuration.Repositories;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
	public partial class AlarmControl : BaseUserControl, ISettingPage
	{
		private readonly IEnumerable<IAlarmControlPresenterDecorator> _decorators;
		private IUnitOfWork _uow;
		private RtaRuleRepository _rtaRuleRepository;
		private RtaMapRepository _rtaMapRepository;
		private readonly List<IRtaRule> _rules = new List<IRtaRule>();
		private readonly List<IRtaMap> _rtaMaps = new List<IRtaMap>();
		private AlarmControlView _view;
		private int _selectedItem =-1;

		public AlarmControl(IEnumerable<IAlarmControlPresenterDecorator> decorators)
		{
			_decorators = decorators;
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
			_view.Presenter = new AlarmControlPresenter(_rules, _view, _decorators);
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
			var atype = new RtaRule(new Description(Resources.GiveAlarmAName), Color.Red, 0, 0.0);
			_rules.Add(atype);
			_view.LoadGrid();
		}

		private void buttonAdvDeleteClick(object sender, EventArgs e)
		{
			if (_selectedItem != -1)
			{
				if (ViewBase.ShowYesNoMessage(Resources.DeleteSelectedRowsQuestionmark, Resources.Alarm) ==
					DialogResult.Yes)
				{
					var item = _rules[_selectedItem];
					if (item.Id.HasValue)
					{
						if (_rtaMaps.Any(x => x.RtaRule != null && x.RtaRule.Equals(item)))
						{
							ViewBase.ShowWarningMessage(Resources.CannotBeDeletedBecauseItIsBeingUsedInMappings, Resources.Rules);
							return;
						}
						_rtaRuleRepository.Remove(item);
					}
					_rules.Remove(item);
					_selectedItem = -1;
				}
			}
			gridControlAlarmTypes.Selections.Clear();

			_view.LoadGrid();
		}

		public void SaveChanges()
		{
			foreach (var type in _rules.Where(type => type.Id == null))
			{
				if(!rulesAreOk(type)) return;
				_rtaRuleRepository.Add(type);
			}
		}

		private bool rulesAreOk(IRtaRule type)
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
			if (type.ThresholdTime < 0)
			{
				warn(Resources.SetTresholdTime);
				return false;
			}
			return true;
		}
		
		public void Unload()
		{
			_rules.Clear();
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

		protected override void SetCommonTexts()
		{
			base.SetCommonTexts();
			this.label2.Text = Resources.Rules;
			this.labelTitle.Text = Resources.ManageRules;
		}

		public void LoadControl()
		{
			loadRules();
			loadMappings();
		}

		private void loadRules()
		{
			_rtaRuleRepository = new RtaRuleRepository(new ThisUnitOfWork(_uow));
			_rules.Clear();
			_rules.AddRange(_rtaRuleRepository.LoadAll());

			if (_view!=null)
			{
				_view.LoadGrid();
			}
		}

		private void loadMappings()
		{
			_rtaMapRepository = new RtaMapRepository(new ThisUnitOfWork(_uow));
			_rtaMaps.Clear();
			_rtaMaps.AddRange(_rtaMapRepository.LoadAll());
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
			return Resources.Rules;
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
