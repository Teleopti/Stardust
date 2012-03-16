using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.Requests
{
    public interface IRequestHistoryViewPresenter
    {
        void ShowHistory(Guid preSelectedPerson, ICollection<IPerson> filteredPersons);
    }
    public class RequestHistoryViewPresenter : IRequestHistoryViewPresenter
    {
        private readonly IRequestHistoryView _requestHistoryView;
        private readonly IEventAggregator _eventAggregator;
        private readonly ICommonAgentNameProvider _commonAgentNameProvider;
        private readonly ILoadRequestHistoryCommand _loadRequestHistoryCommand;

        public RequestHistoryViewPresenter(IRequestHistoryView requestHistoryView, IEventAggregator eventAggregator, 
            ICommonAgentNameProvider commonAgentNameProvider, ILoadRequestHistoryCommand loadRequestHistoryCommand)
        {
            _requestHistoryView = requestHistoryView;
            _eventAggregator = eventAggregator;
            _commonAgentNameProvider = commonAgentNameProvider;
            
            _loadRequestHistoryCommand = loadRequestHistoryCommand;
            _eventAggregator.GetEvent<RequestHistoryPersonChanged>().Subscribe(loadRequests);
        }

        private void loadRequests(string obj)
        {
            _loadRequestHistoryCommand.Execute();
        }

        public void ShowHistory(Guid preSelectedPerson, ICollection<IPerson> filteredPersons)
        {
            ICollection<IRequestPerson> persons = filteredPersons.Select(person => new RequestPerson
                                                                                       {
                                                                                           Name = _commonAgentNameProvider.CommonAgentNameSettings.BuildCommonNameDescription(person), Id = person.Id.Value
                                                                                       }).Cast<IRequestPerson>().ToList();

            _requestHistoryView.FillPersonCombo(persons, preSelectedPerson);
            _requestHistoryView.ShowForm();
        }

    }

    public class RequestPerson : IRequestPerson
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
    }
}