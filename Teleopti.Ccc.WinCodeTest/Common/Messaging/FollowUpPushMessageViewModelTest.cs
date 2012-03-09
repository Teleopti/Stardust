using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.Messaging;
using Teleopti.Ccc.WinCode.Common.Messaging.Filters;
using Teleopti.Ccc.WinCodeTest.Common.Commands;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Common.Messaging
{
    [TestFixture]
    public class FollowUpPushMessageViewModelTest
    {
        private MockRepository _mocker;
        private IRepositoryFactory _repositoryFactory;
        private IPushMessage _pushMessage;
        private string _title;
        private string _message;
        private FollowUpPushMessageViewModel _target;
        private TesterForCommandModels _testerForCommandModels;

        [SetUp]
        public void Setup()
        {
            _mocker = new MockRepository();
            _repositoryFactory = _mocker.StrictMock<IRepositoryFactory>();
            _title = "title";
            _message = "message";
            _pushMessage = new PushMessage {Title = _title, Message = _message};
            _target = new FollowUpPushMessageViewModel(_pushMessage);
            _testerForCommandModels = new TesterForCommandModels();
        }

        [Test]
        public void VerifyConstructor()
        {
           Assert.AreEqual(_title,_target.GetTitle(new NoFormatting()));
           Assert.AreEqual(_message,_target.GetMessage(new NoFormatting()));
        }     

        [Test]
        public void VerifyDelete()
        {
            IPushMessageRepository repository = _mocker.StrictMock<IPushMessageRepository>();
            IObservable<FollowUpPushMessageViewModel> observer = _mocker.StrictMock<IObservable<FollowUpPushMessageViewModel>>();
            IUnitOfWorkFactory unitOfWorkFactory = _mocker.StrictMock<IUnitOfWorkFactory>();
            IUnitOfWork uow = _mocker.StrictMock<IUnitOfWork>();

            _target = new FollowUpPushMessageViewModel(_pushMessage, _repositoryFactory, unitOfWorkFactory);
            using (_mocker.Record())
            {
                Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
                Expect.Call(_repositoryFactory.CreatePushMessageRepository(uow)).Return(repository);
                Expect.Call(() => repository.Remove(_pushMessage)); //Verifies that the message is removed
                Expect.Call(() => observer.Notify(_target));  //Verifies that we can observe delete, used for removing from collections etc..
                Expect.Call(uow.PersistAll()).Return(new List<IRootChangeInfo>()).Repeat.AtLeastOnce(); //make sure the changes are persisted
                uow.Dispose();
            }
            using(_mocker.Playback())
            {
                _target.Observables.Add(observer);

                Assert.AreEqual(UserTexts.Resources.Delete, _target.Delete.Text);
                Assert.IsTrue(_testerForCommandModels.CanExecute(_target.Delete), "Verify that we can execute the command");

                _testerForCommandModels.ExecuteCommandModel(_target.Delete);
            }
        }

        [Test]
        public void VerifyLoadDialogues()
        {
            IPerson person = PersonFactory.CreatePerson();
            IPushMessageDialogueRepository repository = _mocker.StrictMock<IPushMessageDialogueRepository>();
            IUnitOfWorkFactory unitOfWorkFactory = _mocker.StrictMock<IUnitOfWorkFactory>();
            IUnitOfWork uow = _mocker.StrictMock<IUnitOfWork>();

            IList<IPushMessageDialogue> retListFromRep = new List<IPushMessageDialogue> { new PushMessageDialogue(_pushMessage, person) };
                
            using (_mocker.Record())
            {
                Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
                Expect.Call(_repositoryFactory.CreatePushMessageDialogueRepository(uow)).Return(repository);
                Expect.Call(repository.Find(_pushMessage)).Return(retListFromRep);
                uow.Dispose();
            }
            using (_mocker.Playback())
            {
                _target = new FollowUpPushMessageViewModel(_pushMessage, _repositoryFactory, unitOfWorkFactory);

                Assert.IsTrue(_testerForCommandModels.CanExecute(_target.LoadDialogues), "Verify that we can execute the command");

                _testerForCommandModels.ExecuteCommandModel(_target.LoadDialogues);
                Assert.AreEqual(1,_target.Dialogues.Count);
            }
        }

        [Test]
        public void VerifyFilterDialogues()
        {
            ReplyOptionViewModel filter = new ReplyOptionViewModel("bah");
            _target.ReplyOptions.Add(filter);
            _target.AddFilter(filter);
            Assert.IsTrue(filter.FilterIsActive);
            ICollectionView view = CollectionViewSource.GetDefaultView(_target.Dialogues);
            Assert.IsNotNull(view.Filter);
            _target.RemoveFilter(filter);
            view = CollectionViewSource.GetDefaultView(_target.Dialogues);
            Assert.IsFalse(filter.FilterIsActive);
            Assert.IsNull(view.Filter);
        }

        [Test]
        public void VerifyNotRepliedFilter()
        {
            //Make sure that there is a not replied spec by default and that the target is set
            ReplyOptionViewModel notRepliedOption = _target.ReplyOptions.First(r => r.Filter.Filter is DialogueIsRepliedSpecification);
            Assert.IsNotNull(notRepliedOption);
            Assert.IsNotNull(notRepliedOption.FilterTarget);
        }
    }
}
