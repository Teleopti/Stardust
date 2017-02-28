using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.WinCode.Common.Commands;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Ccc.WinCode.Common.Messaging.Filters;

namespace Teleopti.Ccc.WinCode.Common.Messaging
{
	/// <summary>
	/// showing a  conversation in a grid/listbox etc
	/// </summary>
	/// <remarks>
	/// Should handle delete and filtering
	/// Created by: henrika
	/// Created date: 2009-05-20
	/// </remarks>
	public class FollowUpPushMessageViewModel : IFollowUpPushMessageViewModel, IFilterTarget
	{
		private readonly IPushMessage _model;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxLoad")]
		public FollowUpPushMessageViewModel(IPushMessage pushMessage, IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory)
		{
			_repositoryFactory = repositoryFactory;
			_unitOfWorkFactory = unitOfWorkFactory;
			_model = pushMessage;
			Delete = new DeleteCommandModel(this, repositoryFactory, unitOfWorkFactory);
			LoadDialogues = CommandModelFactory.CreateRepositoryCommandModel(LoadAllDialogues, unitOfWorkFactory, "xxLoad Dialogues");
			Observables = new List<IObservable<FollowUpPushMessageViewModel>>();
			Dialogues = new ObservableCollection<IFollowUpMessageDialogueViewModel>();
			ReplyOptions = new List<ReplyOptionViewModel>();

			//Add a replyoption for NotReplied:
			var notRepliedOption = new ReplyOptionViewModel(Dialogues);
			notRepliedOption.FilterTarget = this;
			ReplyOptions.Add(notRepliedOption);

			pushMessage.ReplyOptions.ForEach(r =>
			                                 	{
			                                 		var replyOptionViewModel = new ReplyOptionViewModel(r, Dialogues);
			                                 		replyOptionViewModel.FilterTarget = this;
			                                 		ReplyOptions.Add(replyOptionViewModel);
			                                 	});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IList<IObservable<FollowUpPushMessageViewModel>> Observables { get; private set; }
		public ObservableCollection<IFollowUpMessageDialogueViewModel> Dialogues { get; private set; }

		public string Title
		{
			get { return _model.GetTitle(new NoFormatting()); }
		}

		public string Message
		{
			get { return _model.GetMessage(new NoFormatting()); }
		}

		public IList<ReplyOptionViewModel> ReplyOptions
		{
			get;
			private set;
		}

		public CommandModel Delete { get; private set; }
		public CommandModel LoadDialogues { get; private set; }

		private void ExecuteDelete(IPushMessageRepository repository, IPushMessageDialogueRepository dialogRepository)
		{
			Observables.ForEach(o => o.Notify(this));
			dialogRepository.Remove(_model);
			repository.Remove(_model);
		}

		private void LoadAllDialogues(IUnitOfWork uow)
		{
			Dialogues.Clear();
			var repository = _repositoryFactory.CreatePushMessageDialogueRepository(uow);
			IList<IPushMessageDialogue> dialogues = repository.Find(_model);
			dialogues.ForEach(d => Dialogues.Add(new FollowUpMessageDialogueViewModel(d,_repositoryFactory,_unitOfWorkFactory) { AllowDialogueReply = _model.AllowDialogueReply }));
			ReplyOptions.ForEach(o => o.Total = dialogues.Count);//Todo, change this to databinding instead.....
		}

		public void AddFilter(ReplyOptionViewModel replyOptionViewModel)
		{
			ReplyOptions.ForEach(RemoveFilter);
			replyOptionViewModel.FilterIsActive = true;
			replyOptionViewModel.Filter.FilterAllButCollection(Dialogues);
		}

		public void RemoveFilter(ReplyOptionViewModel replyOptionViewModel)
		{
			replyOptionViewModel.FilterIsActive = false;
			CollectionViewSource.GetDefaultView(Dialogues).Filter = null;
		}

		private class DeleteCommandModel : RepositoryCommandModel
		{
			private readonly FollowUpPushMessageViewModel _commandTarget;
			private readonly IRepositoryFactory _repositoryFactory;

			public DeleteCommandModel(FollowUpPushMessageViewModel model, IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory)
				: base(unitOfWorkFactory)
			{
				_repositoryFactory = repositoryFactory;
				_commandTarget = model;
			}
			public override string Text
			{
				get { return UserTexts.Resources.Delete; }
			}

			public override void OnExecute(IUnitOfWork uow, object sender, ExecutedRoutedEventArgs e)
			{
				IPushMessageRepository repository = _repositoryFactory.CreatePushMessageRepository(uow);
				var dialogRepository = _repositoryFactory.CreatePushMessageDialogueRepository(uow);
				_commandTarget.ExecuteDelete(repository, dialogRepository);
				uow.PersistAll();
			}
		}
	}
}
