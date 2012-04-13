using System;
using System.Collections.Generic;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
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
        	Expect.Call(_requestHistoryView.PageSize).Return(50);
			Expect.Call(_requestHistoryView.StartRow).Return(1).Repeat.AtLeastOnce();
			Expect.Call(() => _requestHistoryView.SetNextEnabledState(false));
			Expect.Call(_requestHistoryView.TotalCount).Return(40);
			Expect.Call(() => _requestHistoryView.SetPreviousEnabledState(false));
            Expect.Call(_requestHistoryView.ShowForm);
            Expect.Call(() => _loadRequestHistoryCommand.Execute());
            _mocks.ReplayAll();
            _requestHistoryViewPresenter.ShowHistory(guid, new List<IPerson>{person});
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldLoadRequestOnPersonChange()
        {
            Expect.Call(() => _requestHistoryView.ShowRequestDetails(""));
            Expect.Call(_requestHistoryView.PageSize).Return(50);
            Expect.Call(_loadRequestHistoryCommand.Execute);
            Expect.Call(_requestHistoryView.StartRow = 1);
            Expect.Call(_requestHistoryView.StartRow).Return(1).Repeat.AtLeastOnce();
            Expect.Call(() => _requestHistoryView.SetNextEnabledState(false));
            Expect.Call(_requestHistoryView.TotalCount).Return(40);
            Expect.Call(() => _requestHistoryView.SetPreviousEnabledState(false));
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<RequestHistoryPageChanged>().Publish(RequestHistoryPage.First);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldLoadRequestOnNextPage()
        {
            Expect.Call(() => _requestHistoryView.ShowRequestDetails(""));
            Expect.Call(_requestHistoryView.PageSize).Return(50);
            Expect.Call(_loadRequestHistoryCommand.Execute);
            Expect.Call(_requestHistoryView.StartRow).Return(1);
            Expect.Call(_requestHistoryView.StartRow = 51);
            Expect.Call(_requestHistoryView.StartRow).Return(51).Repeat.Twice();
            Expect.Call(() => _requestHistoryView.SetNextEnabledState(false));
            Expect.Call(_requestHistoryView.TotalCount).Return(90);
            Expect.Call(() => _requestHistoryView.SetPreviousEnabledState(true));
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<RequestHistoryPageChanged>().Publish(RequestHistoryPage.Next);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldLoadRequestOnPreviousPage()
        {
            Expect.Call(() => _requestHistoryView.ShowRequestDetails(""));
            Expect.Call(_requestHistoryView.PageSize).Return(50);
            Expect.Call(_loadRequestHistoryCommand.Execute);
            Expect.Call(_requestHistoryView.StartRow).Return(101);
            Expect.Call(_requestHistoryView.StartRow = 51);
            Expect.Call(_requestHistoryView.StartRow).Return(51).Repeat.Twice();
            Expect.Call(() => _requestHistoryView.SetNextEnabledState(true));
            Expect.Call(_requestHistoryView.TotalCount).Return(120);
            Expect.Call(() => _requestHistoryView.SetPreviousEnabledState(true));
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<RequestHistoryPageChanged>().Publish(RequestHistoryPage.Previous);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldSetDetailsOnView()
        {
            var history = new RequestHistoryLightweight();
            Expect.Call(() => _requestHistoryView.ShowRequestDetails("")).IgnoreArguments();
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<RequestHistoryRequestChanged>().Publish(history);
            _eventAggregator.GetEvent<RequestHistoryRequestChanged>().Publish(history);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldShowErrorOnDataSourceException()
        {
            var err = new DataSourceException();
            Expect.Call(() => _requestHistoryView.ShowRequestDetails(""));
            Expect.Call(_requestHistoryView.StartRow).Return(101);
            Expect.Call(_requestHistoryView.PageSize).Return(50);
            Expect.Call(_requestHistoryView.StartRow = 1);
            Expect.Call(_loadRequestHistoryCommand.Execute).Throw(err);
            Expect.Call(() => _requestHistoryView.ShowDataSourceException(err));
            Expect.Call(_requestHistoryView.StartRow = 101);
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<RequestHistoryPageChanged>().Publish(RequestHistoryPage.First);
            _mocks.VerifyAll();
        }
    }

    
    
}