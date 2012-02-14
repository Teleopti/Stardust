using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.Principal
{
    public interface IExternalFunctionsProvider
    {
        IEnumerable<IApplicationFunction> ExternalFunctions(IUnitOfWork unitOfWork);
    }

    public class ExternalFunctionsProvider : IExternalFunctionsProvider
    {
        private readonly IRepositoryFactory _repositoryFactory;

        public ExternalFunctionsProvider(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public IEnumerable<IApplicationFunction> ExternalFunctions(IUnitOfWork unitOfWork)
        {
            return _repositoryFactory.CreateApplicationFunctionRepository(unitOfWork).ExternalApplicationFunctions();
        }
    }
}