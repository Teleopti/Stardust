using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.Requests
{
    public interface IRequestHistoryViewPresenter
    {
        void ShowHistory(Guid preselectedPerson, ICollection<IPerson> filteredPersons);
    }
    public class RequestHistoryViewPresenter : IRequestHistoryViewPresenter
    {
        private readonly IRequestHistoryView _requestHistoryView;
        private readonly IEventAggregator _eventAggregator;
        private readonly ICommonAgentNameProvider _commonAgentNameProvider;
        private readonly ILoadRequestHistoryCommand _loadRequestHistoryCommand;
        private IRequestHistoryLightweight _lastHistory;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public RequestHistoryViewPresenter(IRequestHistoryView requestHistoryView, IEventAggregator eventAggregator, 
            ICommonAgentNameProvider commonAgentNameProvider, ILoadRequestHistoryCommand loadRequestHistoryCommand)
        {
            _requestHistoryView = requestHistoryView;
            _eventAggregator = eventAggregator;
            _commonAgentNameProvider = commonAgentNameProvider;
            
            _loadRequestHistoryCommand = loadRequestHistoryCommand;
            _eventAggregator.GetEvent<RequestHistoryPageChanged>().Subscribe(LoadRequests);
            _eventAggregator.GetEvent<RequestHistoryRequestChanged>().Subscribe(SetRequestDetails);
        }

        private void SetRequestDetails(IRequestHistoryLightweight obj)
        {
            if(_lastHistory != null &&  _lastHistory.Equals(obj)) return;
            _lastHistory = obj;
            var details = obj.RequestTypeText + Environment.NewLine;
            details = details + obj.Subject + Environment.NewLine + Environment.NewLine;

            details = details + obj.Info + Environment.NewLine;
            details = details + obj.Dates + Environment.NewLine + Environment.NewLine;

            details = details + obj.RequestStatusText + Environment.NewLine;
            details = details + obj.LatestChangeDateTime + ' ' + obj.SavedByFirstName + ' ' + obj.SavedByLastName + Environment.NewLine + Environment.NewLine;
            
            details = details + obj.Message + Environment.NewLine;
            
            _requestHistoryView.ShowRequestDetails(details);
        }

        private void LoadRequests(RequestHistoryPage historyPage)
        {
            _requestHistoryView.ShowRequestDetails("");
            var startRowBeforeError = _requestHistoryView.StartRow;
            var size = _requestHistoryView.PageSize;
            if (historyPage.Equals(RequestHistoryPage.First))
                _requestHistoryView.StartRow = 1;
            if (historyPage.Equals(RequestHistoryPage.Next))
                _requestHistoryView.StartRow = startRowBeforeError + size;
            if (historyPage.Equals(RequestHistoryPage.Previous))
                _requestHistoryView.StartRow = startRowBeforeError - size;

            try
            {
                _loadRequestHistoryCommand.Execute();
            }
            catch (DataSourceException e)
            {
                _requestHistoryView.ShowDataSourceException(e);
                _requestHistoryView.StartRow = startRowBeforeError;
                return;
            }
            UpdateNextPreviousState(size);
        }

        private void UpdateNextPreviousState(int size)
        {
            _requestHistoryView.SetNextEnabledState(_requestHistoryView.StartRow + size < _requestHistoryView.TotalCount);
            _requestHistoryView.SetPreviousEnabledState(_requestHistoryView.StartRow > 1);
        }

        public void ShowHistory(Guid preselectedPerson, ICollection<IPerson> filteredPersons)
        {
            try
            {
                ICollection<IRequestPerson> persons = filteredPersons.Select(person => (IRequestPerson)new RequestPerson
                {
                    Name = _commonAgentNameProvider.CommonAgentNameSettings.BuildCommonNameDescription(person),
                    Id = person.Id.GetValueOrDefault()
                }).ToList();
                _requestHistoryView.FillPersonCombo(persons, preselectedPerson);
				_loadRequestHistoryCommand.Execute();
				UpdateNextPreviousState(_requestHistoryView.PageSize);
                _requestHistoryView.ShowForm();
            }
            catch (DataSourceException dataSourceException)
            {
                _requestHistoryView.ShowDataSourceException(dataSourceException);
            }
        }
    }

    public class RequestPerson : IRequestPerson
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
    }
}