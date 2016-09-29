using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeExternalLogOnRepository : IExternalLogOnRepository
	{
		public void Add(IExternalLogOn root)
		{
		}

		public void Remove(IExternalLogOn root)
		{
		}

		public IExternalLogOn Get(Guid id)
		{
			return null;
		}

		public IList<IExternalLogOn> LoadAll()
		{
			return null;
		}

		public IExternalLogOn Load(Guid id)
		{
			return null;
		}

		public IUnitOfWork UnitOfWork { get; }
	}
}