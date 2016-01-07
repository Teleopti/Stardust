using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class RequestApprovalServiceFactory : IRequestApprovalServiceFactory
	{	
		private readonly ISwapAndModifyService _swapAndModifyService;
		private readonly IGlobalSettingDataRepository _globalSettingDataRepository;

		public RequestApprovalServiceFactory(ISwapAndModifyService swapAndModifyService, IGlobalSettingDataRepository globalSettingDataRepository)
		{		
			_swapAndModifyService = swapAndModifyService;
			_globalSettingDataRepository = globalSettingDataRepository;
		}


		public IRequestApprovalService MakeRequestApprovalServiceScheduler(IScheduleDictionary scheduleDictionary, IScenario scenario)
		{
			return new RequestApprovalServiceScheduler(
				scheduleDictionary,
				scenario,
				_swapAndModifyService,
				NewBusinessRuleCollection.Minimum(),
				new ResourceCalculationOnlyScheduleDayChangeCallback(),
				_globalSettingDataRepository);
		}
	}
}
