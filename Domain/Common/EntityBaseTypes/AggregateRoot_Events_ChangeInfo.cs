using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common.EntityBaseTypes
{
	public abstract class AggregateRoot_Events_ChangeInfo : 
		AggregateRoot_Events,
		IChangeInfo
	{
		private IPerson _updatedBy;
		private DateTime? _updatedOn;

		public virtual IPerson UpdatedBy => _updatedBy;

		public virtual DateTime? UpdatedOn
		{
			get => _updatedOn;
			set => _updatedOn = value;
		}

		public override void ClearId()
		{
			base.ClearId();
			_updatedBy = null;
			_updatedOn = null;
		}
	}
}