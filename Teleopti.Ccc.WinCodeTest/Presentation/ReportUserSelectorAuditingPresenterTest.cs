using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Presentation;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.WinCodeTest.Presentation
{
    [TestFixture]
    public class ReportUserSelectorAuditingPresenterTest
    {
        private ReportUserSelectorAuditingPresenter _target;
        private IScheduleHistoryReport _scheduleHistoryReport;
        private IReportUserSelectorAuditingView _view;
        private IUnitOfWorkFactory _unitOfWorkfactory;
        private MockRepository _mocks;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.StrictMock<IReportUserSelectorAuditingView>();
            _scheduleHistoryReport = _mocks.StrictMock<IScheduleHistoryReport>();
            _unitOfWorkfactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _target = new ReportUserSelectorAuditingPresenter(_view, _scheduleHistoryReport, _unitOfWorkfactory);
        }

        [Test]
        public void ShouldLoadViewComboWithTheItemAllFirstInList()
        {
            IPerson p1 = new Person().WithName(new Name("pf1", "pl1"));
            IPerson p2 = new Person().WithName(new Name("pf2", "pl2"));
            p1.SetId(Guid.NewGuid());
            p2.SetId(Guid.NewGuid());
            IList<IPerson> personList = new List<IPerson> { p1, p2 };
            var unitOfWork = _mocks.StrictMock<IUnitOfWork>();

            using (_mocks.Record())
            {
                Expect.Call(_unitOfWorkfactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_scheduleHistoryReport.RevisionPeople()).Return(personList);
                Expect.Call(unitOfWork.Dispose);
                Expect.Call(() =>_view.UpdateUsersCombo(new ReadOnlyCollection<ReportUserSelectorAuditingModel>(new List<ReportUserSelectorAuditingModel>()))).IgnoreArguments();
                Expect.Call(() => _view.SetSelectedUser(new ReportUserSelectorAuditingModel(Guid.NewGuid(), UserTexts.Resources.Name))).IgnoreArguments();
            }

            using(_mocks.Playback())
            {
                _target.Initialize(); 
            }

            var all = _target.RevisionList[0];
            var user1 = _target.RevisionList[1];
            var user2 = _target.RevisionList[2];


            Assert.IsNotNull(_target.RevisionList);
            Assert.AreEqual(3, _target.RevisionList.Count);
            Assert.AreEqual(Guid.Empty, all.Id);
            Assert.AreEqual(UserTexts.Resources.All, all.Text);
            Assert.AreEqual(p1.Id, user1.Id);
            Assert.AreEqual(p1.Name.ToString(NameOrderOption.FirstNameLastName), user1.Text);
            Assert.AreEqual(p2.Id, user2.Id);
            Assert.AreEqual(p2.Name.ToString(NameOrderOption.FirstNameLastName), user2.Text);
        }

        [Test]
        public void ShouldReturnAllInRevisionListWhenPersonInModelIsNull()
        {
            var person1 = new Person().WithName(new Name("pf1", "pl1"));
            var person2 = new Person().WithName(new Name("pf2", "pl2"));
            person1.SetId(Guid.NewGuid());
            person2.SetId(Guid.NewGuid());
            IList<IPerson> revisionList = new List<IPerson>{person1, person2};
            var unitOfWork = _mocks.StrictMock<IUnitOfWork>();
            var userModel = new ReportUserSelectorAuditingModel(Guid.Empty, UserTexts.Resources.All);

            using(_mocks.Record())
            {
                Expect.Call(_unitOfWorkfactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_scheduleHistoryReport.RevisionPeople()).Return(revisionList);
                Expect.Call(unitOfWork.Dispose);
                Expect.Call(() => _view.UpdateUsersCombo(new ReadOnlyCollection<ReportUserSelectorAuditingModel>(new List<ReportUserSelectorAuditingModel>()))).IgnoreArguments();
                Expect.Call(() => _view.SetSelectedUser(new ReportUserSelectorAuditingModel(Guid.NewGuid(), UserTexts.Resources.Name))).IgnoreArguments();
                Expect.Call(_view.SelectedUserModel).Return(userModel);
            }

            using(_mocks.Playback())
            {
                _target.Initialize();
                var selectedUsers =  _target.SelectedUsers();

                Assert.AreEqual(2, selectedUsers.Count);
            }
        }

        [Test]
        public void ShouldReturnSelectedUserInRevisionListWhenPersonInModelIsNotNull()
        {
            var person1 = new Person().WithName(new Name("pf1", "pl1"));
            person1.SetId(Guid.NewGuid());
            var person2 = new Person().WithName(new Name("pf2", "pl2"));
            person2.SetId(Guid.NewGuid());
            IList<IPerson> revisionList = new List<IPerson> { person1, person2};
            var unitOfWork = _mocks.StrictMock<IUnitOfWork>();
            var userModel = new ReportUserSelectorAuditingModel(person1);

            using (_mocks.Record())
            {
                Expect.Call(_unitOfWorkfactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_scheduleHistoryReport.RevisionPeople()).Return(revisionList);
                Expect.Call(unitOfWork.Dispose);
                Expect.Call(() => _view.UpdateUsersCombo(new ReadOnlyCollection<ReportUserSelectorAuditingModel>(new List<ReportUserSelectorAuditingModel>()))).IgnoreArguments();
                Expect.Call(() => _view.SetSelectedUser(new ReportUserSelectorAuditingModel(person1))).IgnoreArguments();
                Expect.Call(_view.SelectedUserModel).Return(userModel);
            }

            using (_mocks.Playback())
            {
                _target.Initialize();
                var selectedUsers = _target.SelectedUsers();

                Assert.AreEqual(1, selectedUsers.Count);
                Assert.AreEqual(person1,selectedUsers[0]);
            }   
        }
    }
}
