using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade
{
	public class ShiftTradePendingReasonsService : IShiftTradePendingReasonsService
	{
		private readonly IRequestFactory _requestFactory;
		private readonly ICurrentScenario _scenarioRepository;
		

		public ShiftTradePendingReasonsService(IRequestFactory requestFactory, ICurrentScenario scenarioRepository )
		{
			_requestFactory = requestFactory;
			_scenarioRepository = scenarioRepository;
		}

		public void SetBrokenBusinessRulesFieldOnPersonRequest(IEnumerable<IBusinessRuleResponse> ruleRepsonses, IPersonRequest personRequest)
		{
			var ruleTypes = ruleRepsonses.Select(r => r.TypeOfRule);
			var rulesToSave = NewBusinessRuleCollection.GetFlagFromRules(ruleTypes);
			personRequest.TrySetBrokenBusinessRule(rulesToSave);
		}


		public void SimulateApproveAndSetBusinessRuleResponsesOnFail(IShiftTradeRequest shiftTradeRequest, INewBusinessRuleCollection allNewRules, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			var undoRedoContainer = new UndoRedoContainer(new DoNothingScheduleDayChangeCallBack(), 400);
			setupUndo (undoRedoContainer, schedulingResultStateHolder);

			var requestApprovalServiceScheduler = _requestFactory.GetRequestApprovalService(allNewRules, _scenarioRepository.Current(), schedulingResultStateHolder);
			var brokenBusinessRules = requestApprovalServiceScheduler.ApproveShiftTrade(shiftTradeRequest).ToList();
			
			undoRedoContainer.UndoAll();

			SetBrokenBusinessRulesFieldOnPersonRequest (brokenBusinessRules, shiftTradeRequest.Parent as IPersonRequest );
		}

		private static void setupUndo(IUndoRedoContainer undoRedoContainer, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			if (schedulingResultStateHolder.Schedules != null)
			{
				schedulingResultStateHolder.Schedules.TakeSnapshot();
				schedulingResultStateHolder.Schedules.SetUndoRedoContainer(undoRedoContainer);
			}
		}

	}

	public class ShiftTradePendingReasonsService39473ToggleOff : IShiftTradePendingReasonsService
	{
		public void SimulateApproveAndSetBusinessRuleResponsesOnFail (IShiftTradeRequest shiftTradeRequest, INewBusinessRuleCollection allNewRules, ISchedulingResultStateHolder schedulingResultStateHolder)
		{}

		public void SetBrokenBusinessRulesFieldOnPersonRequest (IEnumerable<IBusinessRuleResponse> ruleRepsonses, IPersonRequest personRequest)
		{}
	}

}
