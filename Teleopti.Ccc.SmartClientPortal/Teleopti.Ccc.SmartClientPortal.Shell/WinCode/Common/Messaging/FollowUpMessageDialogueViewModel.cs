using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Messaging
{
    public class FollowUpMessageDialogueViewModel:DependencyObject, IFollowUpMessageDialogueViewModel
    {
        private readonly IPushMessageDialogue _model;
        private readonly IRepositoryFactory _repositoryFactory;

		public FollowUpMessageDialogueViewModel(IPushMessageDialogue dialogue, IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory)
		{
			_model = dialogue;
			_repositoryFactory = repositoryFactory;
			SendReply = new SendReplyCommandModel(this, unitOfWorkFactory);
			DeleteDialogue = new DeleteDialogueCommandModel(this, unitOfWorkFactory);
			Messages =
				new ObservableCollection<DialogueMessageViewModel>(
					dialogue.DialogueMessages.Select(
						m =>
						new DialogueMessageViewModel
							{Created = m.Created, CreatedAsLocal = TimeZoneHelper.ConvertFromUtc(m.Created, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone), Text = m.Text, Sender = m.Sender}).ToList());
		}

        public CommandModel SendReply { get; private set; }
        public CommandModel DeleteDialogue { get; set;}
        public IPerson Receiver { get { return _model.Receiver; } }
        public ObservableCollection<DialogueMessageViewModel> Messages { get; private set; }

        public string ReplyText
        {
            get { return (string)GetValue(ReplyTextProperty); }
            set { SetValue(ReplyTextProperty, value); }
        }

        public bool IsReplied
        {
            get { return _model.IsReplied; }
        }
        
		public string GetReply(ITextFormatter formatter)
        {
			return _model.GetReply(formatter);
        }

        public bool AllowDialogueReply { get; set; }

        public static readonly DependencyProperty ReplyTextProperty =
            DependencyProperty.Register("ReplyText", typeof(string), typeof(FollowUpMessageDialogueViewModel), new UIPropertyMetadata(string.Empty));

        private class SendReplyCommandModel : RepositoryCommandModel
        {
            private readonly FollowUpMessageDialogueViewModel _commandTarget;

            public SendReplyCommandModel(FollowUpMessageDialogueViewModel model, IUnitOfWorkFactory unitOfWorkFactory) : base(unitOfWorkFactory)
            {
                _commandTarget = model;
            }

            public override string Text
            {
                get { return UserTexts.Resources.Send; }
            }

            public override void OnExecute(IUnitOfWork uow, object sender, ExecutedRoutedEventArgs e)
            {
                _commandTarget.AddAndPersist(uow);
            }

            public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = (!string.IsNullOrEmpty(_commandTarget.ReplyText) && _commandTarget.AllowDialogueReply);
                e.Handled = true;
            }
        }

        private class DeleteDialogueCommandModel : RepositoryCommandModel
        {
            private readonly FollowUpMessageDialogueViewModel _commandTarget;

            public override string Text
            {
                get { return UserTexts.Resources.Delete; }
            }

            public override void OnExecute(IUnitOfWork uow, object sender, ExecutedRoutedEventArgs e)
            {
                _commandTarget.RemoveAndPersist(uow);
            }

            public DeleteDialogueCommandModel(FollowUpMessageDialogueViewModel commandTarget, IUnitOfWorkFactory unitOfWorkFactory) : base(unitOfWorkFactory)
            {
                 _commandTarget = commandTarget;
            }
        }



		private void AddAndPersist(IUnitOfWork uow)
		{
			IPushMessageDialogueRepository repository = _repositoryFactory.CreatePushMessageDialogueRepository(uow);
			uow.Reassociate(_model.PushMessage);
			_model.DialogueReply(ReplyText, _model.PushMessage.Sender);
			repository.Add(_model);

			try
			{
				uow.PersistAll();
			}
			catch (OptimisticLockException)
			{
				// rollback changes: ask Roger if better solution exist
				_model.DialogueMessages.RemoveAt(_model.DialogueMessages.Count - 1);

				// notify user
				var message = UserTexts.Resources.RecipientAnsweredToEarlierMessage;
				MessageBox.Show(message, string.Empty, MessageBoxButton.OK, MessageBoxImage.Warning);
			}

			// refresh view models 
			Messages.Clear();
			_model.DialogueMessages.ForEach(r => Messages.Add(new DialogueMessageViewModel { Created = r.Created, CreatedAsLocal = TimeZoneHelper.ConvertFromUtc(r.Created, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone), Text = r.Text, Sender = r.Sender }));

			ReplyText = string.Empty;
		}

        private void RemoveAndPersist(IUnitOfWork uow)
        {
            IPushMessageDialogueRepository repository = _repositoryFactory.CreatePushMessageDialogueRepository(uow);
            repository.Remove(_model);
            uow.PersistAll();
        }
    }

	public class DialogueMessageViewModel
	{
		public DateTime Created { get; set; }
		public DateTime CreatedAsLocal { get; set; }
		public string Text { get; set; }
		public IPerson Sender { get; set; }
	}
}
