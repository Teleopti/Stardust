using System;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	public class FakePersonAssignmentWriteSideRepository : FakeWriteSideRepository<IPersonAssignment>, IPersonAssignmentWriteSideRepository
	{
		public IPersonAssignment Load(Guid personId, DateOnly date)
		{
			return Entities.Single(e => e.Person.Id.Equals(personId) && e.Date.Equals(date));
		}
	}
}