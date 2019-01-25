using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common.EntityBaseTypes
{
	public abstract class VersionedAggregateRootWithBusinessUnitId : VersionedAggregateRoot,
		IBelongsToBusinessUnitId
	{
		private Guid? _businessUnit;

		public virtual Guid? BusinessUnit
		{
			get => _businessUnit ?? (_businessUnit = ServiceLocator_DONTUSE.CurrentBusinessUnit.CurrentId());
			protected set => _businessUnit = value;
		}
	}
}