using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public class UpdateGroupingReadModel : IUpdateGroupingReadModel 
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
		// Ola if we need to motify it should be another notification
        private readonly IScheduleChangedNotification _scheduleChangedNotification;

        public UpdateGroupingReadModel(IUnitOfWorkFactory unitOfWorkFactory, IGroupingReadOnlyRepository groupingReadOnlyRepository,IScheduleChangedNotification scheduleChangedNotification)
		{
        	_unitOfWorkFactory = unitOfWorkFactory;
        	_groupingReadOnlyRepository = groupingReadOnlyRepository;

			_scheduleChangedNotification = scheduleChangedNotification;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public void Execute(int type,string ids)
		{
			using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				//move the calls to repository and then
				//depending of type
				if (type == 1)
				{
				    _groupingReadOnlyRepository.UpdateGroupingReadModel(ids);
				}
                if (type == 2)
                {
                    _groupingReadOnlyRepository.UpdateGroupingReadModelGroupPage(ids);
                }
                if (type == 3)
                {
                    _groupingReadOnlyRepository.UpdateGroupingReadModelData(ids);
                }
				
				
			}
			
		}

	}
}
