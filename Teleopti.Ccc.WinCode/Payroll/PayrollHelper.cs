using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WinCode.Payroll.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Payroll
{
    /// <summary>
    /// Payroll helper class
    /// </summary>
    public sealed class PayrollHelper : IPayrollHelper
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryFactory _repositoryFactory;

        /// <summary>
        /// Gets the unit of work.
        /// </summary>
        /// <value>The unit of work.</value>
        IUnitOfWork IPayrollHelper.UnitOfWork
        {
            get { return _unitOfWork; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayrollHelper"/> class.
        /// </summary>
        /// <param name="uow">The uow.</param>
        public PayrollHelper(IUnitOfWork uow) : this(uow,new RepositoryFactory())
        {
        }

        public PayrollHelper(IUnitOfWork uow, IRepositoryFactory repositoryFactory)
        {
            _unitOfWork = uow;
            _repositoryFactory = repositoryFactory;
        }

        /// <summary>
        /// Saves the specified definition set.
        /// </summary>
        /// <param name="definitionSet">The definition set.</param>
        void IPayrollHelper.Save(IMultiplicatorDefinitionSet definitionSet)
        {
            GetMultiplicatorDefinitionSetRepository().Add(definitionSet);
        }

        /// <summary>
        /// Deletes the specified definition set.
        /// </summary>
        /// <param name="definitionSet">The definition set.</param>
        void IPayrollHelper.Delete(IMultiplicatorDefinitionSet definitionSet)
        {
            GetMultiplicatorDefinitionSetRepository().Remove(definitionSet);
        }

        /// <summary>
        /// Persists all.
        /// </summary>
        void IPayrollHelper.PersistAll()
        {
            _unitOfWork.PersistAll();
        }

        /// <summary>
        /// Loads the multiplicators.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: VirajS
        /// Created date: 2009-01-20
        /// </remarks>
        IList<IMultiplicator> IPayrollHelper.LoadMultiplicatorList()
        {
            IMultiplicatorRepository multiplicatorRepository = _repositoryFactory.CreateMultiplicatorRepository(_unitOfWork);
            return multiplicatorRepository.LoadAllSortByName();
        }

        /// <summary>
        /// Gets all definition sets.
        /// </summary>
        /// <returns></returns>
        IList<IMultiplicatorDefinitionSet> IPayrollHelper.LoadDefinitionSets()
        {
            return GetMultiplicatorDefinitionSetRepository().LoadAll();
        }

        /// <summary>
        /// Gets the multiplicator definition set repository.
        /// </summary>
        private IMultiplicatorDefinitionSetRepository GetMultiplicatorDefinitionSetRepository()
        {
            return _repositoryFactory.CreateMultiplicatorDefinitionSetRepository(_unitOfWork);
        }

        /// <summary>
        /// Loads the multiplicators.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: VirajS
        /// Created date: 2009-01-20
        /// </remarks>
        /// <remarks>
        /// Created by: VirajS
        /// Created date: 2009-01-20
        /// </remarks>
        IList<IMultiplicator> IPayrollHelper.LoadMultiplicatorList(MultiplicatorType multiplicatorType)
        {
            IMultiplicatorRepository multiplicatorRepository = _repositoryFactory.CreateMultiplicatorRepository(_unitOfWork);
            return multiplicatorRepository.LoadAllByTypeAndSortByName(multiplicatorType);
        }
    }
}
