using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Payroll;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Logic;
using DateOnlyPeriod = Teleopti.Interfaces.Domain.DateOnlyPeriod;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
    public class PayrollPeopleLoader : IPayrollPeopleLoader
    {
        private readonly IRepositoryFactory _repositoryFactory;

        public PayrollPeopleLoader(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public IPerson GetOwningPerson(RunPayrollExportEvent message, IUnitOfWork unitOfWork)
        {
            var personRepository = _repositoryFactory.CreatePersonRepository(unitOfWork);
            return personRepository.Get(message.OwnerPersonId);
        }

        public IEnumerable<IPerson> GetPeopleForExport(RunPayrollExportEvent message, DateOnlyPeriod payrollExportPeriod, IUnitOfWork unitOfWork)
        {
            var personRepository = _repositoryFactory.CreatePersonRepository(unitOfWork);
            var allPeople = personRepository.FindAllAgentsLight(payrollExportPeriod.Convert());
            return allPeople.Where(p => message.ExportPersonIdCollection.Contains(p.Id.GetValueOrDefault()));
        }
    }
}