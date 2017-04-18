using System;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Messaging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common.Messaging
{
    /// <summary>
    /// TestClass, easier than mocking.....
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2009-05-29
    /// </remarks>
    public class FollowUpDialogueForTest : IFollowUpMessageDialogueViewModel
    {
    	private string _reply;
		
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public IPerson Receiver
        {
            get { throw new NotImplementedException(); }
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public ObservableCollection<DialogueMessageViewModel> Messages { get; set;}
        public string ReplyText { get; set; }
        public bool IsReplied { get; set; }
    	public string Reply
    	{
			set { _reply = value; }
    	}
		public string GetReply(ITextFormatter formatter)
		{
			if (formatter == null)
				throw new ArgumentNullException("formatter");
			
			return formatter.Format(_reply);
		}
        public bool AllowDialogueReply { get; set; }
        
    }
}
