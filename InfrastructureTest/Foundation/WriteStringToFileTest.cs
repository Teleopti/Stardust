using System.IO;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
    [TestFixture]
    public class WriteStringToFileTest
    {
        private IWriteToFile target;
        const string fileName = "test.txt";

        [SetUp]
        public void Setup()
        {
            target = new WriteStringToFile();
        }

        [Test]
        public void FileCanBeSaved()
        {
            const string content = "arne anka";
            target.Save(fileName, content);

            string read = File.ReadAllText(fileName);
            Assert.AreEqual(content, read);
            removeFile();
        }

        private static void removeFile()
        {
            File.Delete(fileName);
        }
    }
}
