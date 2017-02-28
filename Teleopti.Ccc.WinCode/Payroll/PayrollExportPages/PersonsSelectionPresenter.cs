using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Payroll.PayrollExportPages
{
    public class PersonsSelectionPresenter
    {
        private readonly IPersonsSelectionView _view;
        private readonly PersonsSelectionModel _model;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public PersonsSelectionPresenter(IPersonsSelectionView view, PersonsSelectionModel model, IRepositoryFactory repositoryFactory,
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            _view = view;
            _model = model;
            _repositoryFactory = repositoryFactory;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public void Initialize()
        {
            _view.ApplicationFunction = _model.ApplicationFunction();
        }

        public void SetSelectedPeopleToPayrollExport(IPayrollExport payrollExport, HashSet<Guid> selectedPersonGuids)
        {
            if (payrollExport == null) throw new ArgumentNullException(nameof(payrollExport));
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var persons = _repositoryFactory.CreatePersonRepository(uow).FindPeople(selectedPersonGuids);
                payrollExport.ClearPersons();
                payrollExport.AddPersons(persons);
            }
        }

        public void PopulateModel(IPayrollExport payrollExport)
        {
            if (payrollExport == null) throw new ArgumentNullException(nameof(payrollExport));
            foreach (var person in payrollExport.Persons)
            {
                _model.AddPerson(person);
            }
            _model.SelectedPeriod = payrollExport.Period;
        }

        
    }
}
