using System;
using System.Collections.Generic;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.Requests
{
    [TestFixture]
    public class RequestHistoryViewPresenterTest
    {
        private MockRepository _mocks;
        private  IRequestHistoryView _requestHistoryView;
        private  IEventAggregator _eventAggregator;
        private ICommonAgentNameProvider _commonAgentNameProvider;
        private  ICommonNameDescriptionSetting _commonAgentNameSettings;
        private IRequestHistoryViewPresenter _requestHistoryViewPresenter;
        private ILoadRequestHistoryCommand _loadRequestHistoryCommand;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _requestHistoryView = _mocks.StrictMock<IRequestHistoryView>();
            _eventAggregator = new EventAggregator();
            _commonAgentNameProvider = _mocks.StrictMock<ICommonAgentNameProvider>();
            _commonAgentNameSettings = _mocks.StrictMock<ICommonNameDescriptionSetting>();
            _loadRequestHistoryCommand = _mocks.StrictMock<ILoadRequestHistoryCommand>();
            _requestHistoryViewPresenter = new RequestHistoryViewPresenter(_requestHistoryView,_eventAggregator,
                _commonAgentNameProvider, _loadRequestHistoryCommand);
        }

        [Test]
        public void ShouldGetFilteredPersonsAndLoadComboInViewOnShow()
        {
            var guid = Guid.NewGuid();
            var person = _mocks.StrictMock<IPerson>();
            
            Expect.Call(_commonAgentNameProvider.CommonAgentNameSettings).Return(_commonAgentNameSettings).Repeat.
                AtLeastOnce();
            Expect.Call(_commonAgentNameSettings.BuildCommonNameDescription(person)).Return("");
            Expect.Call(person.Id).Return(guid);
            Expect.Call(() => _requestHistoryView.FillPersonCombo(new List<IRequestPerson>(), guid)).IgnoreArguments();
            Expect.Call(_requestHistoryView.ShowForm);
            _mocks.ReplayAll();
            _requestHistoryViewPresenter.ShowHistory(guid, new List<IPerson>{person});
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldLoadRequestOnPersonChange()
        {
            Expect.Call(_loadRequestHistoryCommand.Execute);
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<RequestHistoryPersonChanged>().Publish("");
        }
    }

    
    
}