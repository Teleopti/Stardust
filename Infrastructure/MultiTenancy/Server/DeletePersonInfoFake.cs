using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class DeletePersonInfoFake : IDeletePersonInfo
	{
		public void Delete(Guid personId)
		{
			WasDeleted.Add(personId);
		}

		public ICollection<Guid> WasDeleted = new List<Guid>();
	}
}