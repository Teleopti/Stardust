using System.Linq;
using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
    public class SetPersonOptionalValuesForPersonCommandHandler : IHandleCommand<SetPersonOptionalValuesForPersonCommandDto>
    {
        private readonly IOptionalColumnRepository _optionalColumnRepository;
        private readonly IPersonRepository _personRepository;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ICurrentAuthorization _currentAuthorization;

		public SetPersonOptionalValuesForPersonCommandHandler(IOptionalColumnRepository optionalColumnRepository, IPersonRepository personRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory, ICurrentAuthorization currentAuthorization)
        {
            _optionalColumnRepository = optionalColumnRepository;
            _personRepository = personRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
			_currentAuthorization = currentAuthorization;
		}

        public void Handle(SetPersonOptionalValuesForPersonCommandDto command)
        {
            command.Result = new CommandResultDto();
            if (command.OptionalValueCollection.Count == 0) return;

            using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
            {
                var person = _personRepository.Get(command.PersonId);
                if (person == null) throw new FaultException(string.Format(System.Globalization.CultureInfo.InvariantCulture, "No person was found with the given Id ({0}).", command.PersonId));
				person.VerifyCanBeModifiedByCurrentUser(_currentAuthorization);
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
                        person.SetOptionalColumnValue(new OptionalColumnValue(optionalValueDto.Value), column);
                    }
                }

                unitOfWork.PersistAll();

                command.Result.AffectedId = command.PersonId;
                command.Result.AffectedItems = 1;
            }
        }
    }
}