using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeLicenseRepository : ILicenseRepository, ILicenseRepositoryForLicenseVerifier
	{
		private readonly Lazy<License> _license;

		public FakeLicenseRepository()
		{
			_license = new Lazy<License>(() => new License { XmlString = System.IO.File.ReadAllText("license.xml") });
		}

		public ILicenseRepository MakeRepository(IUnitOfWorkFactory unitOfWorkFactory)
		{
			return this;
		}

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
			return new ILicense[] { _license.Value }.ToList();
		}

		public ILicense Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<ILicense> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; set; }

		public IList<ActiveAgent> GetActiveAgents()
		{
			throw new NotImplementedException();
		}
	}
}