﻿using System.Linq;
using System.Windows.Forms;
using Autofac;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.ExceptionHandling;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Ccc.WinCode.Payroll.PayrollExportPages;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Payroll.Forms.PayrollExportPages
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
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
            //_presenter.BuildTreeStructure();
            _selectorView.PreselectedPersonIds = _model.SelectedPersons.Select(selectedPerson => selectedPerson.Id.Value).ToList();
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

        public string PageName
        {
            get { return Resources.PersonSelection; }
        }

        public IApplicationFunction ApplicationFunction { get; set; }

    }
}
