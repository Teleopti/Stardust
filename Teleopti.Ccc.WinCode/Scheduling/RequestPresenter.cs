using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public class RequestPresenter : IRequestPresenter
    {
        private readonly IPersonRequestCheckAuthorization _authorization;
        private static IDictionary<Guid, IPerson> _filteredPersonDictionary;
        
        private IUndoRedoContainer _undoRedo;

        public RequestPresenter(IPersonRequestCheckAuthorization authorization)
        {
            _authorization = authorization;
        }


        public static void InitializeFileteredPersonDictionary(IDictionary<Guid, IPerson> filteredPersonDictionary)
        {
            _filteredPersonDictionary = filteredPersonDictionary;
        }
        /// <summary>
        /// Filter list from specified expression
        /// </summary>
        /// <param name="adapterList"></param>
        /// <param name="filterExpression"></param>
        /// <returns></returns>
        public static IList<PersonRequestViewModel> FilterAdapters(IList<PersonRequestViewModel> adapterList, IList<string> filterExpression)
        {
			var filteredData = searchText(adapterList, filterExpression);
            return filteredData;
        }

		public static IList<PersonRequestViewModel> FilterAdapters(IList<PersonRequestViewModel> adapterList,
		                                                           IEnumerable<Guid> filteredPersons)
		{
			return adapterList.Where(p => filteredPersons.Contains(p.PersonRequest.Person.Id.GetValueOrDefault())).ToList();
		}

        private static IList<PersonRequestViewModel> searchText(IEnumerable<PersonRequestViewModel> data,
                                                 IList<string> filterExpression
                                                 )
        {
            var filteredRequest = new List<PersonRequestViewModel>();

            foreach (var personRequestViewModel in data)
            {
                var element = personRequestViewModel;
                var isElementFoundList = new List<bool>();
                
                foreach (string expression in filterExpression)
                {
                    var filterText = expression;
                    var isTextFound = findText(filterText, element);
                    isElementFoundList.Add(isTextFound);
                }

                if(!isElementFoundList.Contains(false))
                    filteredRequest.Add(element);
            }

            return filteredRequest;
        }

        private static bool findText(string filterText, PersonRequestViewModel element)
        {
            var requestedDate = element.RequestedDate;
            var personRequestStartDate = element.PersonRequest.Request.Period.StartDateTime;
            var personRequestEndDate = element.PersonRequest.Request.Period.EndDateTime;

            var isInFilteredPersonList = _filteredPersonDictionary.ContainsKey(element.PersonRequest.Person.Id.GetValueOrDefault());

            if (requestedDate.Contains(filterText) && isInFilteredPersonList)
                return true;

            if (foundDateTextIfAny(personRequestStartDate, personRequestEndDate, filterText) && isInFilteredPersonList)
                return true;

            if (foundTextInRequestIfAny(filterText, element) && isInFilteredPersonList)
                return true;

            return false;
        }

        private static bool foundTextInRequestIfAny(string filterText, PersonRequestViewModel element)
        {
            var filterTextInLowerCase = filterText.ToLower();

            if (element.Message != null && element.Message.ToLower().Contains(filterTextInLowerCase))
                return true;
			if (element.Name != null && element.Name.ToLower().Contains(filterTextInLowerCase))
                return true;
            if (element.Subject != null && element.Subject.ToLower().Contains(filterTextInLowerCase))
                return true;
			if (element.RequestType != null && element.RequestType.ToLower().Contains(filterTextInLowerCase))
				return true;
			if (element.Details != null && element.Details.ToLower().Contains(filterTextInLowerCase))
                return true;
            if (element.StatusText != null && element.StatusText.ToLower().Contains(filterTextInLowerCase))
                return true;

            var request = element.PersonRequest.Request as AbsenceRequest;

            if (request != null)
            {
                var absenceRequest = request;
                var absenceName = absenceRequest.Absence.Name;

                if (absenceName.ToLower().Contains(filterTextInLowerCase))
                    return true;
            }

            return false;
        }

        private static bool foundDateTextIfAny(DateTime startDate, DateTime endDate, string filterText)
        {
            var numOfDaysInBetween = (endDate - startDate).Days;
            var requestedPeriodList = new List<string>();

            if (numOfDaysInBetween > 0)
            {
                for (var i = 0; i < numOfDaysInBetween; i++)
                {
                    var date = startDate.AddDays(i);
                    requestedPeriodList.Add(date.ToString());

                    if (date.Equals(endDate))
                        break;
                }

                if (requestedPeriodList.Contains(filterText))
                    return true;
            }
            else
            {
                if (startDate.ToString().Contains(filterText))
                    return true;

                if (endDate.ToString().Contains(filterText))
                    return true;
            }

            return false;
        }

        public void SetUndoRedoContainer(IUndoRedoContainer container)
        {
            _undoRedo = container;
        }

        /// <summary>
        /// Approves the or deny.
        /// </summary>
        /// <param name="personRequestViewModels">The request view adaptors.</param>
        /// <param name="command">The command.</param>
        /// <param name="replyText"></param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-10-08
        /// </remarks>
        public void ApproveOrDeny(IList<PersonRequestViewModel> personRequestViewModels, IHandlePersonRequestCommand command, string replyText)
        {
            foreach (PersonRequestViewModel model in personRequestViewModels)
            {
                if (model.IsPending && _authorization.HasEditRequestPermission(model.PersonRequest))
                {
                    if (_undoRedo != null)
                    {
                        IPersonRequest req = model.PersonRequest;
                        _undoRedo.CreateBatch(string.Format(CultureInfo.CurrentUICulture,Resources.UndoRedoPersonRequest, req.Person.Name));
                        _undoRedo.SaveState(req);
                    }
                    model.PersonRequest.Reply(replyText);
                    command.Model = model;
                    command.Execute();
                }
            }
        }

        public void Reply(IList<PersonRequestViewModel> personRequestViewModels, string replyText)
        {
            foreach (PersonRequestViewModel model in personRequestViewModels)
            {
                if (model.IsPending)
                {
                    if (_undoRedo != null)
                    {
                        IPersonRequest req = model.PersonRequest;
                        _undoRedo.SaveState(req);
                        model.PersonRequest.Reply(replyText);
                    }
                }
            }
        }

        public void CommitUndo()
        {
            if (_undoRedo!=null) _undoRedo.CommitBatch();
        }

        public void RollbackUndo()
        {
            if (_undoRedo!=null) _undoRedo.RollbackBatch();
        }
    }

    public class DenyPersonRequestCommand : IHandlePersonRequestCommand
    {
        private readonly IRequestPresenterCallback _callback;
        private readonly IPersonRequestCheckAuthorization _authorization;
        private const string DenyReasonResourceKey = "RequestDenyReasonSupervisor";

        public DenyPersonRequestCommand(IRequestPresenterCallback callback, IPersonRequestCheckAuthorization authorization)
        {
            _callback = callback;
            _authorization = authorization;
        }

        public void Execute()
        {
            Model.PersonRequest.Deny(null, DenyReasonResourceKey,_authorization);
            _callback.CommitUndo();
        }

        public PersonRequestViewModel Model
        {
            get; set;
        }
    }

    public interface IRequestPresenterCallback
    {
        void CommitUndo();
        void RollbackUndo();
    }

    public interface IHandlePersonRequestCommand : IExecutableCommand
    {
        PersonRequestViewModel Model { get; set; }
    }

    public interface IApprovePersonRequestCommand : IHandlePersonRequestCommand
    {
        IList<IBusinessRuleResponse> Approve(INewBusinessRuleCollection newBusinessRules);
    }

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

        public ApprovePersonRequestCommand(IViewBase view, IScheduleDictionary schedules, IScenario scenario, IRequestPresenterCallback callback, IHandleBusinessRuleResponse handleBusinessRuleResponse,
            IPersonRequestCheckAuthorization authorization, INewBusinessRuleCollection newBusinessRules, IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder, IScheduleDayChangeCallback scheduleDayChangeCallback, IGlobalSettingDataRepository globalSettingDataRepository)
        {
            _view = view;
            _schedules = schedules;
            _newBusinessRules = newBusinessRules;
            _overriddenBusinessRulesHolder = overriddenBusinessRulesHolder;
            _scheduleDayChangeCallback = scheduleDayChangeCallback;
            _globalSettingDataRepository = globalSettingDataRepository;
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
            var lstBusinessRuleResponse = Approve( _newBusinessRules);
            var handleBusinessRules = new HandleBusinessRules(_handleBusinessRuleResponse, _view, _overriddenBusinessRulesHolder);
            lstBusinessRuleResponseToOverride.AddRange(handleBusinessRules.Handle(lstBusinessRuleResponse, lstBusinessRuleResponseToOverride));
            if (!lstBusinessRuleResponse.Any())
                return true;
            // try again with overriden
            if (lstBusinessRuleResponseToOverride.Count > 0)
            {
                lstBusinessRuleResponseToOverride.ForEach(_newBusinessRules.Remove);
                lstBusinessRuleResponse = Approve( _newBusinessRules);
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
            var service = new RequestApprovalServiceScheduler(_schedules, _scenario, new SwapAndModifyService(new SwapService(), _scheduleDayChangeCallback), newBusinessRules, _scheduleDayChangeCallback, _globalSettingDataRepository);

            return Model.PersonRequest.Approve(service, _authorization);
        }

        public PersonRequestViewModel Model { get; set; }
    }
}
