using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.WinCode.Budgeting.Models;
using Teleopti.Ccc.WinCode.Budgeting.Views;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Budgeting.Presenters
{
    public class AddShrinkagePresenter
    {
        private readonly IAddShrinkageForm _view;
        private readonly AddShrinkageModel _model;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IRepositoryFactory _repositoryFactory;

        public AddShrinkagePresenter(IAddShrinkageForm view, AddShrinkageModel model, IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory)
        {
            _view = view;
            _model = model;
            _unitOfWorkFactory = unitOfWorkFactory;
            _repositoryFactory = repositoryFactory;
        }

        public void Initialize()
        {
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var absenceRepository = _repositoryFactory.CreateAbsenceRepository(uow);
                var absences = absenceRepository.LoadAll();
                Absences = absences.ToArray();
            }
        }
        
        public IEnumerable<IAbsence> Absences { get; private set; }

        public ICustomShrinkage CustomShrinkageAdded
        {
            get { return _model.CustomShrinkageAdded; }
        }

        public void Save(ICustomShrinkage customShrinkage)
        {
            using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                unitOfWork.Reassociate(_model.BudgetGroup);
                _model.BudgetGroup.AddCustomShrinkage(customShrinkage);
                unitOfWork.PersistAll();
            }
            _model.CustomShrinkageAdded = customShrinkage;
        }

        public void RemoveAllAbsences()
        {
            _view.RemoveSelectedAbsences();
        }

        public void AddAbsences()
        {
            _view.AddSelectedAbsences();
        }

        public void UpdateBudgetGroup(IBudgetGroup budgetGroup)
        {
            _model.BudgetGroup = budgetGroup;
        }
    }
}