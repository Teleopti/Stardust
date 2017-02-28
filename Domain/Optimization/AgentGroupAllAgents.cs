using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class AgentGroupAllAgents : IAgentGroup
	{
		public bool Equals(IEntity other)
		{
			throw new NotImplementedException();
		}

		public Guid? Id
		{
			get { throw new NotImplementedException(); }
		}

		public void SetId(Guid? newId)
		{
			throw new NotImplementedException();
		}

		public void ClearId()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IFilter> Filters => new[] { new allAgents()};

		public string Name
		{
			get { throw new NotImplementedException(); }
		}

		public void ClearFilters()
		{
			throw new NotImplementedException();
		}

		public IAgentGroup AddFilter(IFilter filter)
		{
			throw new NotImplementedException();
		}

		public void ChangeName(string name)
		{
			throw new NotImplementedException();
		}

		private class allAgents : IFilter
		{
			public bool Equals(IEntity other)
			{
				throw new NotImplementedException();
			}

			public Guid? Id
			{
				get { throw new NotImplementedException(); }
			}

			public void SetId(Guid? newId)
			{
				throw new NotImplementedException();
			}

			public void ClearId()
			{
				throw new NotImplementedException();
			}

			public bool IsValidFor(IPerson person, DateOnly dateOnly)
			{
				return true;
			}

			public string FilterType
			{
				get { throw new NotImplementedException(); }
			}
		}
	}
}