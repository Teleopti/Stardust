using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Budgeting.Models
{
    public class AddShrinkageModel
    {
        private readonly IBudgetGroup _budgetGroup;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IRepositoryFactory _repositoryFactory;

        public AddShrinkageModel(IBudgetGroup budgetGroup, IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory)
        {
            _budgetGroup = budgetGroup;
            _unitOfWorkFactory = unitOfWorkFactory;
            _repositoryFactory = repositoryFactory;
        }

        public ICustomShrinkage CustomShrinkageAdded { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public IEnumerable<IAbsence> LoadAbsences()
        {
            IList<IAbsence> loadedAbsences;

            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var absenceRepository = _repositoryFactory.CreateAbsenceRepository(uow);
                var absences = absenceRepository.LoadAll();
                loadedAbsences =  absences.ToArray();
            }
            return loadedAbsences;
        }

        public void Save(ICustomShrinkage customShrinkage)
        {
            using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                unitOfWork.Reassociate(_budgetGroup);
                _budgetGroup.AddCustomShrinkage(customShrinkage);
                unitOfWork.PersistAll();
            }
            CustomShrinkageAdded = customShrinkage;
        }
    }
}