using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeLicenseRepository : ILicenseRepository
	{
		public void Add(ILicense root)
		{
			throw new NotImplementedException();
		}

		public void Remove(ILicense root)
		{
			throw new NotImplementedException();
		}

		public ILicense Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<ILicense> LoadAll()
		{
			return new ILicense[]
			{
				new License {XmlString = File.ReadAllText("License.xml")}
			}.ToList();
		}

		public ILicense Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<ILicense> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; }

		public IList<ActiveAgent> GetActiveAgents()
		{
			throw new NotImplementedException();
		}
	}
}