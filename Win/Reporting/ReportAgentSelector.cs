using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.ExceptionHandling;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.ScheduleFilter;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Reporting
{
    public partial class ReportAgentSelector : BaseUserControl
    {
        private IList<IPerson> _selectedPersons = new List<IPerson>();
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
				ShowFilterDialog();
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

        public IList<IPerson> SelectedPersons
        {
            get { return _selectedPersons; }
        }

		public void SetSelectedPersons(IList<IPerson> persons)
		{
			_selectedPersons = persons;
		}

    	public string SelectedGroupPageKey
    	{
			get { return _selectedGroupPageKey; }
			set { _selectedGroupPageKey = value; }
    	}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void ShowFilterDialog()
        {
            using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                IRepository<IContract> contractRepository = new ContractRepository(unitOfWork);
                IContractScheduleRepository contractScheduleRepository = new ContractScheduleRepository(unitOfWork);
                IGroupPageRepository groupPageRepository = new GroupPageRepository(unitOfWork);
                IRepository<IPartTimePercentage> partTimePercentageRepository = new PartTimePercentageRepository(unitOfWork);
                IRepository<IRuleSetBag> ruleSetBagRepository = new RuleSetBagRepository(unitOfWork);
                ISkillRepository skillRepository = new SkillRepository(unitOfWork);
                IBusinessUnitRepository businessUnitRepository = new BusinessUnitRepository(unitOfWork);
                unitOfWork.Reassociate(_stateHolder.AllPermittedPersons);

				var scheduleFilterModel = new ScheduleFilterModel(_selectedPersons,
                                                                                  _stateHolder,
                                                                                  contractRepository,
                                                                                  contractScheduleRepository,
                                                                                  partTimePercentageRepository,
                                                                                  ruleSetBagRepository,
                                                                                  groupPageRepository,
                                                                                  skillRepository,
                                                                                  businessUnitRepository,
                                                                                  TimeZoneHelper.ConvertToUtc(
                                                                                      DateTime.Today,
                                                                                      TeleoptiPrincipal.Current.Regional.TimeZone));

				unitOfWork.Reassociate(scheduleFilterModel.BusinessUnit.SiteCollection);
                var scheduleFilterView = new ScheduleFilterView(scheduleFilterModel);

				if(!string.IsNullOrEmpty(_selectedGroupPageKey))
				{
					scheduleFilterView.SelectTab(_selectedGroupPageKey);
				}

                scheduleFilterView.StartPosition = FormStartPosition.Manual;
                Point pointToScreen =
                    comboBoxAdv1.PointToScreen(new Point(comboBoxAdv1.Bounds.Y - 4,
                                                              comboBoxAdv1.Bounds.Y + comboBoxAdv1.Height));
                scheduleFilterView.Location = pointToScreen;
				scheduleFilterView.AutoLocate();
                scheduleFilterView.ShowDialog();

                IEnumerable<IPerson> uniquePersons;
                var page = scheduleFilterView.SelectedTabTag() as IGroupPage;
				if (page == null)
				{
					uniquePersons = new HashSet<IPerson>(scheduleFilterModel.SelectedPersonDictionary.Values);
					_selectedGroupPageKey = string.Empty;
				}
				else
				{
					uniquePersons = new HashSet<IPerson>(scheduleFilterModel.SelectedPersons);
					_selectedGroupPageKey = page.Key;
				}

            	_selectedPersons.Clear();
                foreach (var uniquePerson in uniquePersons)
                {
                    _selectedPersons.Add(uniquePerson);
                }

                scheduleFilterView.Dispose();
                UpdateComboWithSelectedAgents();
            }
        }

        public void UpdateComboWithSelectedAgents()
        {
            var currentCultureInfo = TeleoptiPrincipal.Current.Regional.Culture;

            var builder = new StringBuilder();

            builder.Append(_selectedPersons.Count().ToString(currentCultureInfo));
            builder.Append(":");

            foreach (IPerson person in _selectedPersons)
            {
                builder.Append(person.Name);
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
    }
}
