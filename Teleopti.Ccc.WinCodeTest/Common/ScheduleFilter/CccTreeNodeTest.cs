using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common.ScheduleFilter;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common.ScheduleFilter
{
    [TestFixture]
    public class CccTreeNodeTest
    {
        private CccTreeNode _target;
        private int _imageIndex = 1;
        private bool _check = true;
        private IPerson _person;
        private string _displayString = "Team one";
        private CccTreeNode _parent;

        [SetUp]
        public void Setup()
        {
            _parent = new CccTreeNode("Team one's parent", new List<IPerson> { _person }, _check, _imageIndex);
            _person = PersonFactory.CreatePerson("Adolf Gran");
            _target = new CccTreeNode(_displayString, 7, _check, _imageIndex);
            _target.Parent = _parent;
            _target.DisplayExpanded = _check;
            _target.Nodes.Add(new CccTreeNode("Kalle", 8, _check, _imageIndex));
        }

        [Test]
        public void CanCreateInstanceAndReadProperties()
        {
            Assert.IsNotNull(_target);

            Assert.AreEqual(_imageIndex, _target.ImageIndex);
            Assert.AreEqual(_check, _target.IsChecked);
            Assert.AreEqual(_displayString, _target.DisplayName);
            Assert.AreEqual(7, (int)_target.Tag);
            Assert.AreEqual(1, _target.Nodes.Count);
            Assert.AreEqual(_check, _target.DisplayExpanded);
            Assert.AreEqual(_parent, _target.Parent);
        }
    }
}
