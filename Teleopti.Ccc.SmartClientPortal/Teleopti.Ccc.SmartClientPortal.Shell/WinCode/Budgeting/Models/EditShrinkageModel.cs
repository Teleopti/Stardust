using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models
{
    public class EditShrinkageModel
    {
        private readonly ICustomShrinkage _customShrinkage;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IRepositoryFactory _repositoryFactory;

        public EditShrinkageModel(ICustomShrinkage customShrinkage, IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory)
        {
            _customShrinkage = customShrinkage;
            _unitOfWorkFactory = unitOfWorkFactory;
            _repositoryFactory = repositoryFactory;
        }

        public Guid ShrinkageId{ get { return _customShrinkage.Id.GetValueOrDefault(); }}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Shrinakge")]
        public string ShrinakgeName
        {
            get { return _customShrinkage.ShrinkageName; }
        }

        public bool IncludedInAllowance
        {
            get { return _customShrinkage.IncludedInAllowance; }
        }

        public IEnumerable<IAbsence> LoadAbsences()
        {
            IList<IAbsence> loadedAbsences;

            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                initializeAbsenceCollection(uow);
                var absenceRepository = _repositoryFactory.CreateAbsenceRepository(uow);
                var absences = absenceRepository.LoadAll();
                loadedAbsences =  absences.ToArray();
            }
            return loadedAbsences;
        }

        private void initializeAbsenceCollection(IUnitOfWork uow)
        {
            if (LazyLoadingManager.IsInitialized(_customShrinkage.BudgetAbsenceCollection)) return;
            var budgetGroup = getBudgetGroup();
            uow.Reassociate(budgetGroup);
            LazyLoadingManager.Initialize(_customShrinkage.BudgetAbsenceCollection);
        }

        private IBudgetGroup getBudgetGroup()
        {
            var budgetGroup = _customShrinkage.Parent as IBudgetGroup;
            if (budgetGroup == null)
                throw new ArgumentException("Custom shrinkage doesn't has a type of IBudgetGroup as its parent.");
            return budgetGroup;
        }

        public void Save(ICustomShrinkage customShrinkage)
        {
            var budgetGroup = getBudgetGroup();
            using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                unitOfWork.Reassociate(budgetGroup);
                budgetGroup.UpdateCustomShrinkage(ShrinkageId, customShrinkage);
                unitOfWork.PersistAll();
            }
        }

        public IEnumerable<IAbsence> AddedAbsences()
        {
            return _customShrinkage.BudgetAbsenceCollection;
        }
    }
}