using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

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

		public IList<IExternalLogOn> LoadByAcdLogOnNames(IEnumerable<string> externalLogOnNames)
		{
			var filter = externalLogOnNames.ToList();
			return externalLogOns.Where(x => filter.Contains(x.AcdLogOnName)).ToList();
		}

		public IExternalLogOn Load(Guid id)
		{
			return externalLogOns.First(x => x.Id == id);
		}

		public IUnitOfWork UnitOfWork { get; }
	}
}