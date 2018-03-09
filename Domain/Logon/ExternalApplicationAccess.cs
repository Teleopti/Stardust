using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Logon
{
	public class ExternalApplicationAccess : SimpleAggregateRoot, IExternalApplicationAccess
	{
		public virtual Guid PersonId { get; set; }
		public virtual string Hash { get; set; }
		public virtual string Name { get; set; }
		public virtual DateTime CreatedOn { get; set; }
	}

	public interface IExternalApplicationAccess : IAggregateRoot
	{
		Guid PersonId { get; set; }
		string Hash { get; set; }
		string Name { get; set; }
		DateTime CreatedOn { get; set; }
	}
}