using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Main;

namespace Teleopti.Ccc.WinCodeTest.Main
{
    [TestFixture]
    public class LicenseStatusLoaderTest
    {
        [Test]
        public void ShouldLoadFromRepository()
        {
            var xml = @"<?xml version='1.0' encoding='utf-8'?><LicenseStatus/>";
            var mocks = new MockRepository();
            var repFactory = mocks.DynamicMock<IRepositoryFactory>();
            var rep = mocks.DynamicMock<ILicenseStatusRepository>();
            var uow = mocks.DynamicMock<IUnitOfWork>();
            var status = mocks.DynamicMock<ILicenseStatus>();
            var target = new LicenseStatusLoader(repFactory);
            Expect.Call(repFactory.CreateLicenseStatusRepository(uow)).Return(rep);
            Expect.Call(rep.LoadAll()).Return(new List<ILicenseStatus> {status});
            Expect.Call(status.XmlString).Return(xml);
            mocks.ReplayAll();
			Assert.Throws<SignatureValidationException>(() => target.GetStatus(uow));
            mocks.VerifyAll();
        }
    }
}