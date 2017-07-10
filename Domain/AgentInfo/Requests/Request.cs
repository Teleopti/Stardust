using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public abstract class Request : AggregateEntity, IRequest
	{
		private DateTimePeriod _period;
		private string _textForNotification = string.Empty;

		protected Request(DateTimePeriod period)
		{
			_period = period;
		}

		protected Request() { }

		protected void SetPeriod(DateTimePeriod period)
		{
			_period = period;
		}

		public virtual DateTimePeriod Period
		{
			get { return _period; }
		}

		public abstract void Deny(IPerson denyPerson);

		public abstract void Cancel();

		public abstract string GetDetails(CultureInfo cultureInfo);

		public virtual string TextForNotification
		{
			get { return _textForNotification; }
			set { _textForNotification = value; }
		}

		public virtual bool ShouldNotifyWithMessage
		{
			get { return !string.IsNullOrEmpty(TextForNotification); }
		}

		public virtual IList<IPerson> ReceiversForNotification
		{
			get { return new List<IPerson> { Person }; }
		}

		protected internal abstract IEnumerable<IBusinessRuleResponse> Approve(IRequestApprovalService approvalService);

		public virtual IPerson Person
		{
			get
			{
				IPersonRequest personRequest = Parent as IPersonRequest;
				return personRequest == null ? null : personRequest.Person;
			}
		}

		/// <summary>
		/// Description for type of request
		/// </summary>
		public abstract string RequestTypeDescription { get; set; }

		public abstract RequestType RequestType { get; }

		/// <summary>
		/// Description for the payload
		/// </summary>
		public abstract Description RequestPayloadDescription { get; }

		/// <summary>
		/// NoneEnityClone
		/// </summary>
		/// <returns></returns>
		public virtual IRequest NoneEntityClone()
		{
			IRequest part = (IRequest)MemberwiseClone();
			part.SetId(null);
			return part;
		}

		/// <summary>
		/// EntiyClone
		/// </summary>
		/// <returns></returns>
		public virtual IRequest EntityClone()
		{
			IRequest part = (IRequest)MemberwiseClone();
			return part;
		}

		/// <summary>
		/// Clone
		/// </summary>
		/// <returns></returns>
		public virtual object Clone()
		{
			return NoneEntityClone();
		}

		#region PersonfromTo
		public virtual IPerson PersonFrom
		{
			get
			{
				return null;

			}
		}

		public virtual IPerson PersonTo
		{
			get
			{
				return null;
			}
		}
		#endregion //PersonfromTo

		
	}
}