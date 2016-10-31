using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeExternalLogOnRepository : IExternalLogOnRepository
	{
		private readonly List<IExternalLogOn> externalLogOns = new List<IExternalLogOn>();
		public void Add(IExternalLogOn root)
		{
			externalLogOns.Add(root);
		}

		public void Remove(IExternalLogOn root)
		{
			externalLogOns.Remove(root);
		}

		public IExternalLogOn Get(Guid id)
		{
			return externalLogOns.FirstOrDefault(x => x.Id == id);
		}

		public IList<IExternalLogOn> LoadAll()
		{
			return externalLogOns;
		}

		public IExternalLogOn Load(Guid id)
		{
			return externalLogOns.First(x => x.Id == id);
		}

		public IUnitOfWork UnitOfWork { get; }
	}
}