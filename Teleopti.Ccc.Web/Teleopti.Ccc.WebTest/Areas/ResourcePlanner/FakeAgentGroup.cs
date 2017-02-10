using System;
using System.Collections.Generic;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	public class FakeAgentGroup : IAgentGroup
	{
		private readonly Guid _id;
		public IEnumerable<IFilter> Filters { get; }
		public string Name { get; set; }

		public void ClearFilters()
		{
		}

		public void AddFilter(IFilter filter)
		{
		}

		public bool Equals(IEntity other)
		{
			throw new NotImplementedException();
		}

		public Guid? Id { get; private set; }
		public void SetId(Guid? newId)
		{
			Id = newId;
		}

		public void ClearId()
		{
			throw new NotImplementedException();
		}
	}
}