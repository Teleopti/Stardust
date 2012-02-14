using System;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.FileImport;

namespace Teleopti.Ccc.WinCodeTest.FileImport
{
    [TestFixture]
    public class FileImportExceptionTest
    {
        private FileImportException _target;

        [SetUp]
        public void Setup()
        {
            _target = new FileImportException();
        }
        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
            _target = new FileImportException("Fel!");
            Assert.IsNotNull(_target);
            _target = new FileImportException("Fel igen!", new ArgumentException("Nu är det fel!"));
            Assert.IsNotNull(_target);
        }
    }
}
