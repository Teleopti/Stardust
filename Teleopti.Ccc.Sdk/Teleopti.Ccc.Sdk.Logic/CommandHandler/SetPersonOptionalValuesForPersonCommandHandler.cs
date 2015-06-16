using System.Linq;
using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
    public class SetPersonOptionalValuesForPersonCommandHandler : IHandleCommand<SetPersonOptionalValuesForPersonCommandDto>
    {
        private readonly IOptionalColumnRepository _optionalColumnRepository;
        private readonly IPersonRepository _personRepository;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

        public SetPersonOptionalValuesForPersonCommandHandler(IOptionalColumnRepository optionalColumnRepository, IPersonRepository personRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory)
        {
            _optionalColumnRepository = optionalColumnRepository;
            _personRepository = personRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public void Handle(SetPersonOptionalValuesForPersonCommandDto command)
        {
            command.Result = new CommandResultDto();
            if (command.OptionalValueCollection.Count == 0) return;

            using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
            {
                var person = _personRepository.Get(command.PersonId);
                if (person == null) throw new FaultException(string.Format(System.Globalization.CultureInfo.InvariantCulture, "No person was found with the given Id ({0}).", command.PersonId));
                checkIfAuthorized(person, DateOnly.Today);

                var columns = _optionalColumnRepository.GetOptionalColumns<Person>();

                foreach (var optionalValueDto in command.OptionalValueCollection)
                {
                    var column = columns.FirstOrDefault(c => c.Name == optionalValueDto.Key);
                    if (column == null) throw new FaultException(string.Format(System.Globalization.CultureInfo.InvariantCulture, "No column was found with the given name ({0}).", optionalValueDto.Key));

                    if (string.IsNullOrEmpty(optionalValueDto.Value))
                    {
                        var columnValue = person.GetColumnValue(column);
                        if (columnValue != null)
                        {
                            person.RemoveOptionalColumnValue(columnValue);
                        }
                    }
                    else
                    {
                        person.AddOptionalColumnValue(new OptionalColumnValue(optionalValueDto.Value), column);
                    }
                }

                unitOfWork.PersistAll();

                command.Result.AffectedId = command.PersonId;
                command.Result.AffectedItems = 1;
            }
        }

        private static void checkIfAuthorized(IPerson person, DateOnly dateOnly)
        {
            var authorizationInstance = PrincipalAuthorization.Instance();
            if (
                !authorizationInstance.IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage, dateOnly,
                                                   person))
            {
                throw new FaultException(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                                                       "You're not allowed to work with this person ({0}).", person.Name));
            }
        }
    }
}