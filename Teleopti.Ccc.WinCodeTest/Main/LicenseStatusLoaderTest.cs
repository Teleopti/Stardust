using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.WinCode.Main;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Main
{
    [TestFixture]
    public class LicenseStatusLoaderTest
    {
        [Test, ExpectedException(typeof(SignatureValidationException))]
        public void ShouldLoadFromRepository()
        {
            var xml = @"<?xml version='1.0' encoding='utf-8'?><LicenseStatus/>";
            var mocks = new MockRepository();
            var rep = mocks.DynamicMock<ILicenseStatusRepository>();
            var status = mocks.DynamicMock<ILicenseStatus>();
            var target = new LicenseStatusLoader(rep);
            Expect.Call(rep.LoadAll()).Return(new List<ILicenseStatus> {status});
            Expect.Call(status.XmlString).Return(xml);
            mocks.ReplayAll();
            target.GetStatus();
            mocks.VerifyAll();
        }
    }
}