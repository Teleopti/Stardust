using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autofac;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.ExceptionHandling;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Reporting
{
    public partial class ReportPersonSelector : BaseUserControl
    {
        private HashSet<Guid> _selectedAgentGuids = new HashSet<Guid>();
		private DateOnlyPeriod _dateOnlyPeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
        private IApplicationFunction _applicationFunction;
        private IComponentContext _componentContext;
        private ICollection<IPerson> _selectedAgents = new List<IPerson>();
        private string _selectedGroupPage;

        public ReportPersonSelector()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!DesignMode && StateHolderReader.IsInitialized)
            {
                comboBoxAdv1.PopupContainer.Popup += PopupContainer_Popup;
                comboBoxAdv1.PopupContainer.CloseUp += PopupContainer_CloseUp;

                SetTexts();
            }
        }

        void PopupContainer_CloseUp(object sender, Syncfusion.Windows.Forms.PopupClosedEventArgs e)
        {
            showFilterDialog();
        }

        void PopupContainer_Popup(object sender, EventArgs e)
        {
            comboBoxAdv1.PopupContainer.HidePopup();
        }

        // sätt denna efter att hämtat från settings
        public void Init(HashSet<Guid> selectedAgentGuids, IComponentContext componentContext, IApplicationFunction applicationFunction, string selectedGroupPage)
        {
            _selectedAgentGuids = selectedAgentGuids;
            _componentContext = componentContext;
            _applicationFunction = applicationFunction;
            _selectedGroupPage = selectedGroupPage;
            getAgentsFromGuids();
            updateComboWithSelectedAgents();
        }

        private void showFilterDialog()
        {
            Cursor = Cursors.WaitCursor;

            ReportPersonsSelectionView personsSelectionView = null;

            try
            {
				personsSelectionView = new ReportPersonsSelectionView(_dateOnlyPeriod, _selectedAgentGuids, _componentContext, _applicationFunction, _selectedGroupPage);

                Point pointToScreen =
                    comboBoxAdv1.PointToScreen(new Point(comboBoxAdv1.Bounds.Y - 4,
                                                         comboBoxAdv1.Bounds.Y + comboBoxAdv1.Height));
                personsSelectionView.StartPosition = FormStartPosition.Manual;
                personsSelectionView.Location = pointToScreen;
                personsSelectionView.AutoLocate();
                DialogResult dialogResult = personsSelectionView.ShowDialog();

                if (dialogResult == DialogResult.OK)
                {
                    _selectedAgentGuids = personsSelectionView.SelectedAgentGuids();
                    getAgentsFromGuids();
                    updateComboWithSelectedAgents();
                    _selectedGroupPage = personsSelectionView.SelectedGroupPageKey;
                }
            }
            finally 
            {
                if (personsSelectionView != null) personsSelectionView.Dispose();
            }
            
            Cursor = Cursors.Default;
        }

        private void getAgentsFromGuids()
        {
            try
            {
                using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    _selectedAgents = new PersonRepository(new ThisUnitOfWork(uow)).FindPeople(_selectedAgentGuids);
                }
            }
            catch (DataSourceException dataSourceException)
            {
                using (var view = new SimpleExceptionHandlerView(dataSourceException,
                                                           Resources.Reports,
                                                           Resources.ServerUnavailable))
                {
                    view.ShowDialog();
                }
                return;
            }
        }
    	private void updateComboWithSelectedAgents()
        {
            CultureInfo currentCultureInfo = TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.Culture;

            var builder = new StringBuilder();

            builder.Append(_selectedAgents.Count().ToString(currentCultureInfo));
            builder.Append(":");

            foreach (IPerson person in _selectedAgents)
            {
                builder.Append(person.Name);
                builder.Append(", ");
            }
            if (_selectedAgents.Count > 0)
                builder.Remove(builder.Length - 2, 2);

            comboBoxAdv1.Text = builder.ToString();
        }

        public override bool HasHelp
        {
            get
            {
                return false;
            }
        }

        public ICollection<IPerson> SelectedAgents
        {
            get { return _selectedAgents; }
        }

        public HashSet<Guid> SelectedAgentGuids
        {
            get { return _selectedAgentGuids; }
        }

        public void SetPeriod(DateOnlyPeriod period)
        {
			_dateOnlyPeriod = period;
        }

        public string SelectedGroupPage()
        {
            return _selectedGroupPage;
        }
    }
}
