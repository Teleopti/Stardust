using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common.Configuration;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Common.Configuration
{
	public partial class AlarmControl : BaseUserControl, ISettingPage
	{
		private readonly IEnumerable<IAlarmControlPresenterDecorator> _decorators;
		private readonly IRtaControlNamer _rtaControlNamer;
		private IUnitOfWork _uow;
		private RtaRuleRepository _rtaRuleRepository;
		private readonly List<IRtaRule> _rules = new List<IRtaRule>();
		private AlarmControlView _view;
		private int _selectedItem =-1;

		public AlarmControl(IEnumerable<IAlarmControlPresenterDecorator> decorators, IRtaControlNamer rtaControlNamer)
		{
			_decorators = decorators;
			_rtaControlNamer = rtaControlNamer;
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
			var atype = new RtaRule(new Description(Resources.GiveAlarmAName), Color.Red, new TimeSpan(0, 0, 0), 0.0);
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
					if(item.Id.HasValue)
						_rtaRuleRepository.Remove(item);
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
			if (!_decorators.IsNullOrEmpty())
				_rules.ForEach(checkAdherenceType);
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
			if (type.ThresholdTime < TimeSpan.Zero)
			{
				warn(Resources.SetTresholdTime);
				return false;
			}
			return true;
		}

		private static void checkAdherenceType(IRtaRule _rtaRule)
		{
			if (!_rtaRule.Adherence.HasValue)
				throw new ValidationException(Resources.AdherenceCannotBeEmpty);
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
			this.label2.Text = _rtaControlNamer.PanelHeader();
			this.labelTitle.Text = _rtaControlNamer.Title();
		}

		public void LoadControl()
		{
			loadRules();
		}

		private void loadRules()
		{
			_rtaRuleRepository = new RtaRuleRepository(_uow);
			_rules.Clear();
			_rules.AddRange(_rtaRuleRepository.LoadAll());

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
			return _rtaControlNamer.TreeNodeName();
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
