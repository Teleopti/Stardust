
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public class UpdatePersonFinderReadModel : IUpdatePersonFinderReadModel  
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IPersonFinderReadOnlyRepository _personFinderReadOnlyRepository;
		// Ola if we need to motify it should be another notification
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private readonly IScheduleChangedNotification _scheduleChangedNotification;

        public UpdatePersonFinderReadModel(IUnitOfWorkFactory unitOfWorkFactory, IPersonFinderReadOnlyRepository personFinderReadOnlyRepository, IScheduleChangedNotification scheduleChangedNotification)
		{
        	_unitOfWorkFactory = unitOfWorkFactory;
            _personFinderReadOnlyRepository = personFinderReadOnlyRepository;

			_scheduleChangedNotification = scheduleChangedNotification;
		}

		public void Execute(bool isPerson,string ids)
		{
			using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				//move the calls to repository and then
				//depending of type
                if (isPerson == true)
				{
                    _personFinderReadOnlyRepository.UpdateFindPerson( ids);
				}
                if (isPerson == false)
                {
                    _personFinderReadOnlyRepository.UpdateFindPersonData( ids);
                }
                
				
			}
			
		}

	}
}

