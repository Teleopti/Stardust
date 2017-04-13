using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Autofac;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.ExceptionHandling;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Ccc.WinCode.Payroll.PayrollExportPages;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Payroll.Forms.PayrollExportPages
{
    public partial class PersonsSelectionView : BaseUserControl, IPersonsSelectionView
    {
        private readonly PersonsSelectionPresenter _presenter;
        private readonly IPersonSelectorPresenter _personSelectorPresenter;
        private readonly IPersonSelectorView _selectorView;
        private readonly PersonsSelectionModel _model;

        public PersonsSelectionView()
        {
            InitializeComponent();
        }
		
        public PersonsSelectionView(PersonsSelectionModel model, IComponentContext componentContext) : this()
        {
            _model = model;
            _personSelectorPresenter =
                componentContext.Resolve<ILifetimeScope>().BeginLifetimeScope().Resolve<IPersonSelectorPresenter>();

            _presenter = new PersonsSelectionPresenter(this, model, new RepositoryFactory(), UnitOfWorkFactory.Current);
            _presenter.Initialize();

            _personSelectorPresenter.ApplicationFunction = model.ApplicationFunction();
            _personSelectorPresenter.ShowPersons = true;

            var view = (Control)_personSelectorPresenter.View;
            Controls.Add(view);
            view.Dock = DockStyle.Fill;

            _selectorView = _personSelectorPresenter.View;
            _selectorView.ShowCheckBoxes = true;
            _selectorView.ShowDateSelection = false;
            _selectorView.HideMenu = true;
			_selectorView.KeepInteractiveOnDuringLoad = true;
        }

        public void Populate(IAggregateRoot aggregateRoot)
        {
            var payrollExport = (IPayrollExport)aggregateRoot;
            _presenter.PopulateModel(payrollExport);

            _selectorView.PreselectedPersonIds = new HashSet<Guid>(_model.SelectedPersons.Select(selectedPerson => selectedPerson.Id.Value));
            _selectorView.SelectedPeriod = _model.SelectedPeriod;
            _personSelectorPresenter.LoadTabs();
        }

        public bool Depopulate(IAggregateRoot aggregateRoot)
        {
            try
            {
                var payrollExport = (IPayrollExport)aggregateRoot;
                _presenter.SetSelectedPeopleToPayrollExport(payrollExport, _personSelectorPresenter.CheckedPersonGuids);
            }
            catch (DataSourceException exception)
            {
                using (var view = new SimpleExceptionHandlerView(exception,
                                                                    Resources.PayrollExport,
                                                                    Resources.ServerUnavailable))
                {
                    view.ShowDialog();
                }
                return false;
            }
            
            return true;
        }

        public void SetEditMode()
        {
        }

        public string PageName => Resources.PersonSelection;

	    public IApplicationFunction ApplicationFunction { get; set; }

    }
}
