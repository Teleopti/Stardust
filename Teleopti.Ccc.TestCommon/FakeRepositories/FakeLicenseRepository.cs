using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Licensing;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeLicenseRepository : ILicenseRepository, ILicenseRepositoryForLicenseVerifier
	{
		private readonly Lazy<License> _license;

		public FakeLicenseRepository()
		{
			_license = new Lazy<License>(() => new License { XmlString = System.IO.File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "Teleopti_RD.xml")) });
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

		public IUnitOfWork UnitOfWork { get; set; }

		public IList<ActiveAgent> GetActiveAgents()
		{
			throw new NotImplementedException();
		}
	}
}