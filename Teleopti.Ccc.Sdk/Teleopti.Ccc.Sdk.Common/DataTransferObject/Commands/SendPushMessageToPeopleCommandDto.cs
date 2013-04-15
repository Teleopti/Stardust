using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
	/// <summary>
	/// Specify a command to send a push message to people.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2013/03/")]
	public class SendPushMessageToPeopleCommandDto : CommandDto
	{
		private ICollection<Guid> _recipients;
		private ICollection<string> _replyOptions;

		public SendPushMessageToPeopleCommandDto()
		{
			ReplyOptions = new Collection<string>();
			Recipients = new Collection<Guid>();
		}

		/// <summary>
		/// Gets and sets the mandatory person Id's for the recipients.
		/// </summary>
		/// <value>The person Id's.</value>
		/// <remarks>The list of people for a single message is limited to 50 recipients.</remarks>
		[DataMember]
		public ICollection<Guid> Recipients
		{
			get { return _recipients; }
			private set
			{
				if (value!=null)
				{
					_recipients = new List<Guid>(value);
				}
			}
		}

		/// <summary>
		/// Gets and sets the mandatory title.
		/// </summary>
		/// <value>The subject for the message.</value>
		[DataMember]
		public string Title { get; set; }

		/// <summary>
		/// Gets and sets the mandatory message.
		/// </summary>
		/// <value>The message to send.</value>
		[DataMember]
		public string Message { get; set; }

		/// <summary>
		/// Gets and sets whether a free text answer is allowed for this message.
		/// </summary>
		/// <value>Default value is false.</value>
		[DataMember]
		public bool AllowReply { get; set; }

		/// <summary>
		/// Gets and sets the different options displayed as button alternatives for the receiver.
		/// </summary>
		/// <value>A list of possible selections, examples are ["Yes","No"] or ["OK"].</value>
		[DataMember]
		public ICollection<string> ReplyOptions
		{
			get { return _replyOptions; }
			private set
			{
				if (value!=null)
				{
					_replyOptions = new List<string>(value);
				}
			}
		}
	}
}