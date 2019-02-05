using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common.EntityBaseTypes
{
	public abstract class AggregateRoot_Events_Versioned_BusinessUnitId : 
		AggregateRoot_Events,
		IVersioned,
		IBelongsToBusinessUnitId
	{
		private Guid? _businessUnit;

		public virtual Guid? BusinessUnit
		{
			get => _businessUnit;
			set => _businessUnit = value;
		}

		private int? _version;

		public virtual int? Version => _version;

		public virtual void SetVersion(int version)
		{
			_version = version;
		}
	}
}