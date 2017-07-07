using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.UndoRedo;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade
{
	public class ShiftTradeApproveService : IShiftTradeApproveService
	{
		private readonly IPersonRequestCheckAuthorization _authorization;
		private readonly IDifferenceCollectionService<IPersistableScheduleData> _differenceService;
		private readonly IScheduleDifferenceSaver _scheduleDictionarySaver;
		private readonly IRequestFactory _requestFactory;
		private readonly ICurrentScenario _scenarioRepository;

		public ShiftTradeApproveService(IPersonRequestCheckAuthorization authorization,
			IDifferenceCollectionService<IPersistableScheduleData> differenceService,
			IScheduleDifferenceSaver scheduleDictionarySaver, IRequestFactory requestFactory, ICurrentScenario scenarioRepository)
		{
			_authorization = authorization;
			_differenceService = differenceService;
			_scheduleDictionarySaver = scheduleDictionarySaver;
			_requestFactory = requestFactory;
			_scenarioRepository = scenarioRepository;
		}

		public IList<IBusinessRuleResponse> AutoApprove(IPersonRequest personRequest, IRequestApprovalService approvalService
			, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			var brokenBusinessRules = personRequest.Approve(approvalService, _authorization, true);
			foreach (var range in schedulingResultStateHolder.Schedules.Values)
			{
				var diff = range.DifferenceSinceSnapshot(_differenceService);
				_scheduleDictionarySaver.SaveChanges(diff, (IUnvalidatedScheduleRangeUpdate) range);
			}
			return brokenBusinessRules;
		}

		public IList<IBusinessRuleResponse> SimulateApprove(IShiftTradeRequest shiftTradeRequest,
			INewBusinessRuleCollection allNewRules,
			ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			var undoRedoContainer = new UndoRedoContainer();
			setupUndo(undoRedoContainer, schedulingResultStateHolder);

			var requestApprovalServiceScheduler = _requestFactory.GetRequestApprovalService(allNewRules,
				_scenarioRepository.Current(), schedulingResultStateHolder, shiftTradeRequest.Parent as IPersonRequest);
			var brokenBusinessRules = requestApprovalServiceScheduler.Approve(shiftTradeRequest).ToList();

			undoRedoContainer.UndoAll();
			return brokenBusinessRules;
		}

		private static void setupUndo(IUndoRedoContainer undoRedoContainer,
			ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			if (schedulingResultStateHolder.Schedules != null)
			{
				schedulingResultStateHolder.Schedules.TakeSnapshot();
				schedulingResultStateHolder.Schedules.SetUndoRedoContainer(undoRedoContainer);
			}
		}
	}
}
