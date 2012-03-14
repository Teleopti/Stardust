using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.WinCode.Common.Commands;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Common.Messaging
{
   

    public class FollowUpMessageDialogueViewModel:DependencyObject, IFollowUpMessageDialogueViewModel
    {
      

        #region fields & props
        private IPushMessageDialogue _model;
        private IRepositoryFactory _repositoryFactory;   
        
        public CommandModel SendReply { get; private set; }
        public CommandModel DeleteDialogue { get; set;}
        public IPerson Receiver { get { return _model.Receiver; } }
        public ObservableCollection<IDialogueMessage> Messages { get; private set; }
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

       

       
       
        #endregion

        public FollowUpMessageDialogueViewModel(IPushMessageDialogue dialogue)
            : this(dialogue, new RepositoryFactory(), UnitOfWorkFactory.Current)
        {
            
        }



        public FollowUpMessageDialogueViewModel(IPushMessageDialogue dialogue, IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _model = dialogue;
            _repositoryFactory = repositoryFactory;
            SendReply = new SendReplyCommandModel(this, unitOfWorkFactory);
            DeleteDialogue = new DeleteDialogueCommandModel(this, unitOfWorkFactory);
            Messages = new ObservableCollection<IDialogueMessage>(dialogue.DialogueMessages);
        }

        //CommandModel for adding a message to the dialogue
        private class SendReplyCommandModel : RepositoryCommandModel
        {
            private FollowUpMessageDialogueViewModel _commandTarget;


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
            private FollowUpMessageDialogueViewModel _commandTarget;

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
            _model.DialogueReply(ReplyText,_model.PushMessage.Sender);
            repository.Add(_model);
           
            Messages.Clear();
            _model.DialogueMessages.ForEach(r => Messages.Add(r));
            
            uow.PersistAll();
            ReplyText = string.Empty;
        }

        private void RemoveAndPersist(IUnitOfWork uow)
        {
            IPushMessageDialogueRepository repository = _repositoryFactory.CreatePushMessageDialogueRepository(uow);
            repository.Remove(_model);
            uow.PersistAll();
        }
    }
}
