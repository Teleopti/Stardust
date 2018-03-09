using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class ExternalApplicationAccess
	{
		public ExternalApplicationAccess(Guid personId, string name, string hash) : this()
		{
			PersonId = personId;
			Name = name;
			Hash = hash;
			CreatedOn = DateTime.UtcNow;
		}

		protected ExternalApplicationAccess()
		{
			
		}

		public virtual int Id { get; protected set; }
		public virtual Guid PersonId { get; set; }
		public virtual string Hash { get; set; }
		public virtual string Name { get; set; }
		public virtual DateTime CreatedOn { get; set; }
	}
}