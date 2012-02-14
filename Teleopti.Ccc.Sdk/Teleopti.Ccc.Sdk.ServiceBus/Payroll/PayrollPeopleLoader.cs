using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
    public class PayrollPeopleLoader : IPayrollPeopleLoader
    {
        private readonly IRepositoryFactory _repositoryFactory;

        public PayrollPeopleLoader(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public IPerson GetOwningPerson(RunPayrollExport message, IUnitOfWork unitOfWork)
        {
            var personRepository = _repositoryFactory.CreatePersonRepository(unitOfWork);
            return personRepository.Get(message.OwnerPersonId);
        }

        public IEnumerable<IPerson> GetPeopleForExport(RunPayrollExport message, DateTimePeriod payrollExportPeriod, IUnitOfWork unitOfWork)
        {
            var personRepository = _repositoryFactory.CreatePersonRepository(unitOfWork);
            var allPeople = personRepository.FindPeopleInOrganizationLight(payrollExportPeriod);
            return allPeople.Where(p => message.ExportPersonIdCollection.Contains(p.Id.GetValueOrDefault()));
        }
    }
}