using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Requests;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.Requests
{
    [TestFixture]
    public class LoadRequestHistoryCommandTest
    {
        private MockRepository _mocks;
        private ILoadRequestHistoryCommand _target;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IRepositoryFactory _repositoryFactory;
        private IRequestHistoryView _requestHistoryView;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
            _requestHistoryView = _mocks.StrictMock<IRequestHistoryView>();
            _target = new LoadRequestHistoryCommand(_unitOfWorkFactory, _repositoryFactory, _requestHistoryView);
        }

        [Test]
        public void ShouldGetPersonAndStartRowAndCallRepository()
        {
            var uow = _mocks.StrictMock<IStatelessUnitOfWork>();
            var guid = Guid.NewGuid();
            var rep = _mocks.StrictMock<IRequestHistoryReadOnlyRepository>();
            Expect.Call(_unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork()).Return(uow);
            Expect.Call(_requestHistoryView.StartRow).Return(1);
            Expect.Call(_requestHistoryView.PageSize).Return(50);
            Expect.Call(() => _requestHistoryView.TotalCount = 0);
            Expect.Call(_requestHistoryView.SelectedPerson).Return(guid);
            
            Expect.Call(_repositoryFactory.CreateRequestHistoryReadOnlyRepository(uow)).Return(rep);
            Expect.Call(rep.LoadOnPerson(guid, 1, 51)).Return(new List<IRequestHistoryLightweight>{new RequestHistoryLightweight{TotalCount = 5}});
            Expect.Call(() => _requestHistoryView.TotalCount = 5);
            Expect.Call(() => _requestHistoryView.FillRequestList(new ListViewItem[0])).IgnoreArguments();
            Expect.Call(uow.Dispose);
            _mocks.ReplayAll();
            _target.Execute();
            _mocks.VerifyAll();
        }
    }

    
}