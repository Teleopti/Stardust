using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Common
{
	public abstract class Payload : AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit, IDeleteTag
	{
		private bool _inContractTime;
		private ITracker _tracker;
		private bool _inWorkTime;
		private bool _inPaidTime;
		private bool _isDeleted;

		protected Payload(bool inContractTime)
		{
			_inContractTime = inContractTime;
		}

		protected Payload()
		{
		}

		/// <summary>
		/// Gets or sets a value indicating whether this payload is [in contract time].
		/// </summary>
		/// <value><c>true</c> if [in contract time]; otherwise, <c>false</c>.</value>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-02-27
		/// </remarks>
		public virtual bool InContractTime
		{
			get { return _inContractTime; }
			set { _inContractTime =value; }
		}

		public virtual ITracker Tracker
		{
			get { return _tracker; }
			set { _tracker = value; }
		}

		public virtual bool InWorkTime
		{
			get { return _inWorkTime; }
			set { _inWorkTime = value; }
		}

		public virtual bool InPaidTime
		{
			get { return _inPaidTime; }
			set { _inPaidTime = value; }
		}

		public virtual bool IsDeleted
		{
			get { return _isDeleted; }
		}

		public virtual void SetDeleted()
		{
			_isDeleted = true;
		}

		public abstract IPayload UnderlyingPayload { get; }
	}
}
