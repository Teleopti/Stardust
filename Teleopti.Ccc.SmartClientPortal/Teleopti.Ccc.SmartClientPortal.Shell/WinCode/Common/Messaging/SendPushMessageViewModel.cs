using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Messaging
{
    /// <summary>
    /// Class responsible for sending a pushmessage
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2009-05-27
    /// </remarks>
    public class SendPushMessageViewModel : DependencyObject
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public CommandModel SendMessageCommand { get; private set; }
        public TwoStepCommandModel AddReplyOptionCommand { get; private set; }
        public CommandModel RemoveReplyOptionCommand { get; private set; }
        public CommandModel YesNoReplyCommand { get; private set; }
        public CommandModel OkReplyCommand { get; private set; }
       
        public ObservableCollection<IPerson> Receivers { get; private set; }
        public ObservableCollection<string> ReplyOptions { get; private set; }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(SendPushMessageViewModel), new UIPropertyMetadata(string.Empty));

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(SendPushMessageViewModel), new UIPropertyMetadata(string.Empty));


        public string ReplyOptionToAdd
        {
            get { return (string)GetValue(ReplyOptionToAddProperty); }
            set { SetValue(ReplyOptionToAddProperty, value); }
        }
        public static readonly DependencyProperty ReplyOptionToAddProperty =
            DependencyProperty.Register("ReplyOptionToAdd", typeof(string), typeof(SendPushMessageViewModel), new UIPropertyMetadata(string.Empty));

        public bool AllowDialogueReply
        {
            get { return (bool)GetValue(AllowDialogueReplyProperty); }
            set { SetValue(AllowDialogueReplyProperty, value); }
        }

        public static readonly DependencyProperty AllowDialogueReplyProperty =
            DependencyProperty.Register("AllowDialogueReply", typeof(bool), typeof(SendPushMessageViewModel), new UIPropertyMetadata(true));

        public SendPushMessageViewModel()
            : this(new RepositoryFactory(), UnitOfWorkFactory.Current)
        {
        }

        public SendPushMessageViewModel(IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _repositoryFactory = repositoryFactory;
            _unitOfWorkFactory = unitOfWorkFactory;
            Receivers = new ObservableCollection<IPerson>();
            ReplyOptions = new ReplyOptionCollection();
            SetupCommands();
        }

        public virtual ISendPushMessageService CreateSendPushMessageService()
        {
            return SendPushMessageService.CreateConversation(Title, Message, AllowDialogueReply).To(Receivers).AddReplyOption(ReplyOptions);
        }

        private void SetupCommands()
        {
            SendMessageCommand = CommandModelFactory.CreateRepositoryCommandModel(SendMessage, CanSendMessage, _unitOfWorkFactory, UserTexts.Resources.Send);
            AddReplyOptionCommand = new TwoStepCommandModel(CommandModelFactory.CreateCommandModel(AddReply, CanAddReply, UserTexts.Resources.Add), UserTexts.Resources.NewReplyOption, UserTexts.Resources.Cancel);
            RemoveReplyOptionCommand = CommandModelFactory.CreateCommandModel(RemoveReply, CanRemoveReply,
                                                                              UserTexts.Resources.Delete);
            YesNoReplyCommand = CommandModelFactory.CreateCommandModel(YesNoReply, UserTexts.Resources.YesNoAnswer);
            OkReplyCommand = CommandModelFactory.CreateCommandModel(StandardReply, UserTexts.Resources.Ok);
        }

        private void StandardReply()
        {
            ReplyOptions.Clear();
            ReplyOptions.Add(UserTexts.Resources.Ok);
        }

        private void YesNoReply()
        {
            ReplyOptions.Clear();
            ReplyOptions.Add(UserTexts.Resources.Yes);
            ReplyOptions.Add(UserTexts.Resources.No);
        }

        private bool CanRemoveReply()
        {
            return CollectionViewSource.GetDefaultView(ReplyOptions).CurrentItem != null;
        }

        private void RemoveReply()
        {
            string replyToremove = (string)CollectionViewSource.GetDefaultView(ReplyOptions).CurrentItem;
            ReplyOptions.Remove(replyToremove);
        }

        private bool CanAddReply()
        {
            return (!string.IsNullOrEmpty(ReplyOptionToAdd) && !ReplyOptions.Contains(ReplyOptionToAdd));
        }

        private bool CanSendMessage()
        {
            return (!ReplyOptions.IsEmpty()
                && !Receivers.IsEmpty()
                && !String.IsNullOrEmpty(Title)
                && !String.IsNullOrEmpty(Message));
        }

        private void AddReply()
        {
            ReplyOptions.Add(ReplyOptionToAdd);
            ReplyOptionToAdd = string.Empty;
        }

        private void SendMessage(IUnitOfWork uow)
        {
            var repository = _repositoryFactory.CreatePushMessageRepository(uow);
	        var persister = new PushMessagePersister(repository, PushMessageDialogueRepository.DONT_USE_CTOR(uow),
		        new CreatePushMessageDialoguesService());
			CreateSendPushMessageService().SendConversation(persister);
            uow.PersistAll();
            ClearValue(MessageProperty);
            ClearValue(TitleProperty);
        }

        public void SetReceivers(IEnumerable<IPerson> receivers)
        {
            Receivers.Clear();
            receivers.ForEach(p => Receivers.Add(p));
        }
       
        private sealed class ReplyOptionCollection : ObservableCollection<string>
        {
            private readonly string _default = UserTexts.Resources.Ok;

            public ReplyOptionCollection()
            {
                Add(_default);
            }
        }
    }
}
