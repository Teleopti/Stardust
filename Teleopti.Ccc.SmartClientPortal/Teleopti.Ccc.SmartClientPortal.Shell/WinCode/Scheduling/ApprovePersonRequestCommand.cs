using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Scheduling;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
	public class ApprovePersonRequestCommand : IApprovePersonRequestCommand
	{
		private readonly IViewBase _view;
		private readonly IScheduleDictionary _schedules;
		private readonly IScenario _scenario;
		private readonly IRequestPresenterCallback _callback;
		private readonly IHandleBusinessRuleResponse _handleBusinessRuleResponse;
		private readonly IPersonRequestCheckAuthorization _authorization;
		private readonly IOverriddenBusinessRulesHolder _overriddenBusinessRulesHolder;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly IGlobalSettingDataRepository _globalSettingDataRepository;
		private readonly INewBusinessRuleCollection _newBusinessRules;
		private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private readonly IPersonRequestRepository _personRequestRepository;

		public ApprovePersonRequestCommand(IViewBase view, IScheduleDictionary schedules, IScenario scenario, IRequestPresenterCallback callback, IHandleBusinessRuleResponse handleBusinessRuleResponse,
			IPersonRequestCheckAuthorization authorization, INewBusinessRuleCollection newBusinessRules, IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder, IScheduleDayChangeCallback scheduleDayChangeCallback, IGlobalSettingDataRepository globalSettingDataRepository, IPersonAbsenceAccountRepository personAbsenceAccountRepository,
			IPersonRequestRepository personRequestRepository)
		{
			_view = view;
			_schedules = schedules;
			_newBusinessRules = newBusinessRules;
			_overriddenBusinessRulesHolder = overriddenBusinessRulesHolder;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_globalSettingDataRepository = globalSettingDataRepository;
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
			_personRequestRepository = personRequestRepository;
			_scenario = scenario;
			_callback = callback;
			_handleBusinessRuleResponse = handleBusinessRuleResponse;
			_authorization = authorization;
		}

		public void Execute()
		{
			if (TryModify())
			{
				_callback.CommitUndo();
			}
			else
			{
				_callback.RollbackUndo();
			}
		}

		private bool TryModify()
		{
			var lstBusinessRuleResponseToOverride = new List<IBusinessRuleResponse>();
			var lstBusinessRuleResponse = Approve(_newBusinessRules);
			var handleBusinessRules = new HandleBusinessRules(_handleBusinessRuleResponse, _view, _overriddenBusinessRulesHolder);
			lstBusinessRuleResponseToOverride.AddRange(handleBusinessRules.Handle(lstBusinessRuleResponse, lstBusinessRuleResponseToOverride));
			if (!lstBusinessRuleResponse.Any())
				return true;
			// try again with overriden
			if (lstBusinessRuleResponseToOverride.Count > 0)
			{
				lstBusinessRuleResponseToOverride.ForEach(_newBusinessRules.DoNotHaltModify);
				lstBusinessRuleResponse = Approve(_newBusinessRules);
				lstBusinessRuleResponseToOverride = new List<IBusinessRuleResponse>();
				foreach (var response in lstBusinessRuleResponse)
				{
					if (!response.Overridden)
						lstBusinessRuleResponseToOverride.Add(response);
				}
			}
			else
			{
				return false;
			}
			//if it's more than zero now. Cancel!!!
			if (lstBusinessRuleResponseToOverride.Count > 0)
			{
				// show a MessageBox, another not overridable rule (Mandatory) might have been found later in the SheduleRange
				// will probably not happen
				_view.ShowErrorMessage(lstBusinessRuleResponse.First().Message, Resources.ViolationOfABusinessRule);
				return false;
			}
			return true;
		}

		public IList<IBusinessRuleResponse> Approve(INewBusinessRuleCollection newBusinessRules)
		{
			IRequestApprovalService service = null;
			switch (Model.PersonRequest.Request.RequestType)
			{
				case RequestType.AbsenceRequest:
					service = new AbsenceRequestApprovalService(_scenario, _schedules, newBusinessRules, _scheduleDayChangeCallback,
						_globalSettingDataRepository, new CheckingPersonalAccountDaysProvider(_personAbsenceAccountRepository));
					break;
				case RequestType.ShiftTradeRequest:
					service = new ShiftTradeRequestApprovalService(_schedules,
						new SwapAndModifyService(new SwapService(), _scheduleDayChangeCallback, TimeZoneGuardForDesktop.Instance_DONTUSE), newBusinessRules, _authorization, _personRequestRepository);
					break;
			}
			return Model.PersonRequest.Approve(service, _authorization);
		}

		public PersonRequestViewModel Model { get; set; }
	}
}