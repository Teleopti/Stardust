using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.AgentPortalCode.Common;

namespace Teleopti.Ccc.AgentPortalCodeTest.Common
{
    [TestFixture]
    public class AbsenceTest
    {
        private Absence _target;
        private string _name;
        private string _shortName;
        private string _id;
        private Color _color;

        [SetUp]
        public void Setup()
        {
            _name = "name";
            _shortName = "shortName";
            _id = "id";
            _color = Color.Blue;
            _target = new Absence(_name, _shortName, _id, _color);    
        }

        [Test]
        public void ShouldSetProperties()
        {
            Assert.AreEqual(_name, _target.Name);
            Assert.AreEqual(_shortName, _target.ShortName);
            Assert.AreEqual(_id, _target.Id);
            Assert.AreEqual(_color, _target.Color);
        }
    }
}
