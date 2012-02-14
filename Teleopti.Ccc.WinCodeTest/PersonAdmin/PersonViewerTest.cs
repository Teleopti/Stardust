using NUnit.Framework;
using Teleopti.Ccc.WinCode.PeopleAdmin;

namespace Teleopti.Ccc.WinCodeTest.PersonAdmin
{
    /// <summary>
    /// Tests for PersonViewer
    /// </summary>
    [TestFixture]
    public class PersonViewerTest
    {
        private PersonViewer _target;


        [SetUp]
        public void Setup()
        {
            _target = new PersonViewer("Malsara", "0194-9345-445405", 7);
        }

        [Test]
        public void VerifyPropertiesAreNotNull()
        {
            Assert.IsNotEmpty(_target.Id);
            Assert.IsNotEmpty(_target.Name);
            Assert.AreEqual(7, _target.ImageIndex);
        }


    }

}
