using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Messaging;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Messaging
{
    /// <summary>
    /// Sending and followup for Pushmessages
    /// </summary>
    /// <remarks>
    /// Add public accessors to this control
    /// </remarks>
    public partial class PushMessageControl : UserControl
    {
        private enum ReplyState
        {
            Standard,
            YesNo,
            Custom
        }

        private ReplyState _replyState = ReplyState.Standard;

        public SendPushMessageViewModel SendPushMessageModel { get; private set;}
        public FollowUpServiceViewModel FollowUpPushMessageModel { get; private set; }

        public PushMessageControl()
        {
            InitializeComponent();
        }

    	public PushMessageControl(IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory) : this()
    	{
			IPerson currentPerson = ((ITeleoptiPrincipalWithUnsafePerson)TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal).UnsafePerson();
			SendPushMessageModel = new SendPushMessageViewModel(repositoryFactory,unitOfWorkFactory);
			if (currentPerson != null)
			{
				FollowUpPushMessageModel = new FollowUpServiceViewModel(currentPerson,repositoryFactory,unitOfWorkFactory);
			}
			
			DataContext = this;
    	}

        public void SetReceivers(IEnumerable<IPerson> receivers)
        {
            SendPushMessageModel.SetReceivers(receivers);
        }
        
        private void SetReplyState(IEnumerable<string> replyoptions,ReplyState replyState)
        {
            if (_replyState!=replyState)
            {
                _replyState = replyState;
                SendPushMessageModel.ReplyOptions.Clear();
                replyoptions.ForEach(o => SendPushMessageModel.ReplyOptions.Add(o));
            }
        }

        private void Standard_GotFocus(object sender, RoutedEventArgs e)
        {
            SetReplyState(new List<string> { UserTexts.Resources.Ok },ReplyState.Standard);
            HelpProvider.SetHelpString(this, "PeopleWorksheet+MessagesStandard");
        }

        private void YesNo_GotFocus(object sender, RoutedEventArgs e)
        {
            SetReplyState(new List<string> { UserTexts.Resources.Yes, UserTexts.Resources.No }, ReplyState.YesNo);
            HelpProvider.SetHelpString(this, "PeopleWorksheet+MessagesYesNo");
        }

        private void Custom_GotFocus(object sender, RoutedEventArgs e)
        {
            SetReplyState(new List<string>(),ReplyState.Custom);
            HelpProvider.SetHelpString(this, "PeopleWorksheet+MessagesCustom");
        }

        private void FollowUp_GotFocus(object sender, RoutedEventArgs e)
        {
            HelpProvider.SetHelpString(this, "PeopleWorksheet+MessagesFollowUp");
        }
    }
}
