using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public class UpdatePersonFinderReadModel : IUpdatePersonFinderReadModel  
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IPersonFinderReadOnlyRepository _personFinderReadOnlyRepository;
		
        public UpdatePersonFinderReadModel(IUnitOfWorkFactory unitOfWorkFactory, IPersonFinderReadOnlyRepository personFinderReadOnlyRepository)
		{
        	_unitOfWorkFactory = unitOfWorkFactory;
            _personFinderReadOnlyRepository = personFinderReadOnlyRepository;
		}

		public void Execute(bool isPerson,string ids)
		{
			using (_unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork())
			{
                if (isPerson)
				{
                    _personFinderReadOnlyRepository.UpdateFindPerson( ids);
					return;
				}    
                _personFinderReadOnlyRepository.UpdateFindPersonData( ids);
			}
		}
	}
}

