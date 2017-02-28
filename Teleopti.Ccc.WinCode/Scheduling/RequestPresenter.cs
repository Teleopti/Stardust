using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public class RequestPresenter : IRequestPresenter
    {
        private readonly IPersonRequestCheckAuthorization _authorization;
        private IDictionary<Guid, IPerson> _filteredPersonDictionary;
        private IUndoRedoContainer _undoRedo;

        public RequestPresenter(IPersonRequestCheckAuthorization authorization)
        {
            _authorization = authorization;
        }
		
        public void InitializeFileteredPersonDictionary(IDictionary<Guid, IPerson> filteredPersonDictionary)
        {
            _filteredPersonDictionary = filteredPersonDictionary;
        }
        /// <summary>
        /// Filter list from specified expression
        /// </summary>
        /// <param name="adapterList"></param>
        /// <param name="filterExpression"></param>
        /// <returns></returns>
        public IList<PersonRequestViewModel> FilterAdapters(IList<PersonRequestViewModel> adapterList, IList<string> filterExpression)
        {
			var filteredData = searchText(adapterList, filterExpression);
            return filteredData;
        }

		public IList<PersonRequestViewModel> FilterAdapters(IList<PersonRequestViewModel> adapterList,
		                                                           IEnumerable<Guid> filteredPersons)
		{
			return adapterList.Where(p => filteredPersons.Contains(p.PersonRequest.Person.Id.GetValueOrDefault())).ToList();
		}

        private IList<PersonRequestViewModel> searchText(IEnumerable<PersonRequestViewModel> data,
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

        private bool findText(string filterText, PersonRequestViewModel element)
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
	        _undoRedo?.CommitBatch();
        }

        public void RollbackUndo()
        {
	        _undoRedo?.RollbackBatch();
        }
    }
}
