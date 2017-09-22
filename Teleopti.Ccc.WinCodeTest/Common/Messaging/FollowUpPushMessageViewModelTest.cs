using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Messaging;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Messaging.Filters;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCodeTest.Common.Commands;

namespace Teleopti.Ccc.WinCodeTest.Common.Messaging
{
	[TestFixture]
	public class FollowUpPushMessageViewModelTest
	{
		private IRepositoryFactory _repositoryFactory;
		private IPushMessage _pushMessage;
		private string _title;
		private string _message;
		private FollowUpPushMessageViewModel _target;
		private TesterForCommandModels _testerForCommandModels;
		private IUnitOfWorkFactory _unitOfWorkFactory;

		[SetUp]
		public void Setup()
		{
			_repositoryFactory = MockRepository.GenerateStrictMock<IRepositoryFactory>();
			_unitOfWorkFactory = MockRepository.GenerateStrictMock<IUnitOfWorkFactory>();
			_title = "title";
			_message = "message";
			_pushMessage = new PushMessage { Title = _title, Message = _message };
			_target = new FollowUpPushMessageViewModel(_pushMessage, _repositoryFactory, _unitOfWorkFactory);
			_testerForCommandModels = new TesterForCommandModels();
		}

		[Test]
		public void VerifyConstructor()
		{
			Assert.AreEqual(_title, _target.Title);
			Assert.AreEqual(_message, _target.Message);
		}

		[Test]
		public void VerifyDelete()
		{
			var repository = MockRepository.GenerateStrictMock<IPushMessageRepository>();
			var dialogRepository = MockRepository.GenerateStrictMock<IPushMessageDialogueRepository>();
			var observer = MockRepository.GenerateStrictMock<IObservable<FollowUpPushMessageViewModel>>();
			var uow = MockRepository.GenerateStrictMock<IUnitOfWork>();

			_target = new FollowUpPushMessageViewModel(_pushMessage, _repositoryFactory, _unitOfWorkFactory);

			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			_repositoryFactory.Stub(x => x.CreatePushMessageRepository(uow)).Return(repository);
			_repositoryFactory.Stub(x => x.CreatePushMessageDialogueRepository(uow)).Return(dialogRepository);
			repository.Stub(x => x.Remove(_pushMessage)); //Verifies that the message is removed
			dialogRepository.Stub(x => x.Remove(_pushMessage)); //Verifies that the message is removed
			observer.Stub(x => x.Notify(_target));  //Verifies that we can observe delete, used for removing from collections etc..
			uow.Stub(x => x.PersistAll()).Return(new List<IRootChangeInfo>()).Repeat.AtLeastOnce(); //make sure the changes are persisted
			uow.Stub(x => x.Dispose());

			_target.Observables.Add(observer);

			Assert.AreEqual(UserTexts.Resources.Delete, _target.Delete.Text);
			Assert.IsTrue(_testerForCommandModels.CanExecute(_target.Delete), "Verify that we can execute the command");

			_testerForCommandModels.ExecuteCommandModel(_target.Delete);
		}

		[Test]
		public void VerifyLoadDialogues()
		{
			IPerson person = PersonFactory.CreatePerson();
			var repository = MockRepository.GenerateStrictMock<IPushMessageDialogueRepository>();
			var uow = MockRepository.GenerateStrictMock<IUnitOfWork>();

			IList<IPushMessageDialogue> retListFromRep = new List<IPushMessageDialogue> { new PushMessageDialogue(_pushMessage, person) };

			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			_repositoryFactory.Stub(x => x.CreatePushMessageDialogueRepository(uow)).Return(repository);
			repository.Stub(x => x.Find(_pushMessage)).Return(retListFromRep);
			uow.Stub(x => x.Dispose());

			_target = new FollowUpPushMessageViewModel(_pushMessage, _repositoryFactory, _unitOfWorkFactory);

			Assert.IsTrue(_testerForCommandModels.CanExecute(_target.LoadDialogues), "Verify that we can execute the command");

			_testerForCommandModels.ExecuteCommandModel(_target.LoadDialogues);
			Assert.AreEqual(1, _target.Dialogues.Count);
		}

		[Test]
		public void VerifyFilterDialogues()
		{
			var filter = new ReplyOptionViewModel("bah");
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
