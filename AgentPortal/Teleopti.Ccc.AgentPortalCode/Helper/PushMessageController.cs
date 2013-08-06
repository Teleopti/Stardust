using System;
using System.Collections.ObjectModel;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.AgentPortalCode.Helper
{
    public class PushMessageController
    {
        private readonly PersonDto _affectedPerson;

        public event EventHandler<PushMessageHelperEventArgs> NumberOfUnreadMessagesChanged;
        public event EventHandler<PushMessageHelperEventArgs> MessageAdded;
        public event EventHandler<PushMessageHelperEventArgs> MessageDeleted;
        
        public static PushMessageController CreatePushMessageHelper()
        {
            return new PushMessageController(StateHolder.Instance.State.SessionScopeData.LoggedOnPerson);
        }

        private PushMessageController(PersonDto affectedPerson)
        {
            _affectedPerson = affectedPerson;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public int GetNumberOfUnhandledMessages()
        {
            var loadedPushMessageDialogueDto =
                SdkServiceHelper.OrganizationService.GetPushMessageDialoguesNotRepliedTo(_affectedPerson);
            return loadedPushMessageDialogueDto.Count;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public Collection<MessagePresenterObject> GetMessagePresenterObjects()
        {
            var loadedPushMessageDialogueDto = SdkServiceHelper.OrganizationService.GetPushMessageDialoguesNotRepliedTo(_affectedPerson);
            var messagePresenterObjects = new Collection<MessagePresenterObject>();
            foreach (PushMessageDialogueDto dto in loadedPushMessageDialogueDto)
            {
                var messagePresenterObject = new MessagePresenterObject(dto);
                messagePresenterObjects.Add(messagePresenterObject);
            }
            return messagePresenterObjects;
        }

        public void ReplyAndUpdate(ReplyMessageEventArgs e,object originalSource)
        {
            SdkServiceHelper.OrganizationService.SetReply(e.PresenterObject.Dto, e.OptionChecked);
            if (e.PresenterObject.Dto.PushMessage.AllowDialogueReply)
            {
                SdkServiceHelper.OrganizationService.SetDialogueReply(e.PresenterObject.Dto, e.TextMessage, _affectedPerson);
            }
            e.PresenterObject.Dto = SdkServiceHelper.OrganizationService.GetPushMessageDialogue(e.PresenterObject.Dto);
            int numberOfUnHandledMessages = GetNumberOfUnhandledMessages();

            RaiseNumberOfUnreadMessagesChanged(new PushMessageHelperEventArgs(numberOfUnHandledMessages, originalSource));
        }

        //Note: If possible, move the messagebrokercode here as well instead of calling this method
        public void MessageChanged(EventMessageArgs args)
        {
            if (typeof(IPushMessageDialogue).IsAssignableFrom(args.Message.InterfaceType) && (args.Message.ReferenceObjectId == _affectedPerson.Id))
            {
                //note cache this value?
                int numberOfUnreadMessages = GetNumberOfUnhandledMessages();
                var pushMessageHelperEventArgs = new PushMessageHelperEventArgs(numberOfUnreadMessages,this);

                switch (args.Message.DomainUpdateType)
                {
                    case DomainUpdateType.Insert:
                		var handlerAdded = MessageAdded;
						if (handlerAdded != null)
                        {
                            var pushMessageDialogueDto = new PushMessageDialogueDto();
                            pushMessageDialogueDto.Id = args.Message.DomainObjectId;
                            PushMessageDialogueDto loadedPushMessageDialogueDto = SdkServiceHelper.OrganizationService.GetPushMessageDialogue(pushMessageDialogueDto);
                            
                            if(loadedPushMessageDialogueDto!=null && !loadedPushMessageDialogueDto.IsReplied )
                            {
                                string title = loadedPushMessageDialogueDto.PushMessage.Title;
                                string message = loadedPushMessageDialogueDto.Message; //Translated

                                pushMessageHelperEventArgs = new PushMessageHelperEventArgs(title, message, numberOfUnreadMessages,this);
								handlerAdded(this, pushMessageHelperEventArgs);
                            }
                        }
                        RaiseNumberOfUnreadMessagesChanged(pushMessageHelperEventArgs);
                        break;
                   
                    case DomainUpdateType.Delete:
                		var handlerDeleted = MessageDeleted;
                        if (handlerDeleted!=null)
                        {
                            handlerDeleted(this, pushMessageHelperEventArgs);
                        }
                        RaiseNumberOfUnreadMessagesChanged(pushMessageHelperEventArgs);
                        break;

                    case DomainUpdateType.Update:
                        RaiseNumberOfUnreadMessagesChanged(pushMessageHelperEventArgs);
                        break;
                }
            }
        }

        private void RaiseNumberOfUnreadMessagesChanged(PushMessageHelperEventArgs args)
        {
        	var handler = NumberOfUnreadMessagesChanged;
            if (handler!= null)
            {
            	handler(this, args);
            }
        }
    }

    public class PushMessageHelperEventArgs : EventArgs
    {
        public string Title { get; private set; }
        public string Message { get; private set; }
        public int NoOfMails { get; private set; }
        public object OriginalSource { get; private set; }

        public PushMessageHelperEventArgs(int unreadMessages, object originalSource)
        {
            OriginalSource = originalSource;
            NoOfMails = unreadMessages;
            Title = string.Empty;
            Message = string.Empty;
        }

        public PushMessageHelperEventArgs(string title, string message, int unreadMessages, object originalSource)
            : this(unreadMessages, originalSource)
        {
            Title = TruncateTo(title,50);
            Message = TruncateTo(message,50);
        }

        private static string TruncateTo(string text, int maxLength)
        {
            if (text.Length <= maxLength)
            {
                return text;
            }
            return text.Substring(0, maxLength) + UserTexts.Resources.ThreeDots;
        }
    }
}
