using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard;

namespace Teleopti.Ccc.WinCodeTest.Common.Clipboard
{
    [TestFixture]
    public class ClipTest
    {
        Clip<string> _clip1;
        Clip<string> _clip2;
        Clip<int> _clip3;
        Clip<int> _clip4;
        string _stringObject;
        int _intObject;

        [SetUp]
        public void Setup()
        {
            _stringObject = "test";
            _intObject = 1;
            _clip1 = new Clip<string>(1, 2, _stringObject);
            _clip2 = new Clip<string>(1, 2, _stringObject);
            _clip3 = new Clip<int>(0, 0, _intObject);
            _clip4 = new Clip<int>(0, 1, _intObject);
        }

        [Test]
        public void CanCreateClip()
        {
            Assert.IsNotNull(_clip1);
        }

        [Test]
        public void CheckGetSet()
        {
            Assert.AreEqual(1, _clip2.RowOffset);
            Assert.AreEqual(2, _clip2.ColOffset);
            Assert.AreSame(_stringObject, _clip2.ClipValue);
        }

        [Test]
        public void CheckGetHashCode()
        {
            IDictionary<Clip<int>, int> dictionary = new Dictionary<Clip<int>, int>();
            dictionary[_clip3] = 1;
            dictionary[_clip4] = 2;
            Assert.AreEqual(1, dictionary[_clip3]);
            Assert.AreEqual(2, dictionary[_clip4]);
        }

        [Test]
        public void CheckEquals()
        {
            Assert.IsFalse(_clip2.Equals(null));
            Assert.IsTrue(_clip1.Equals((object)_clip2));
            Assert.IsTrue(_clip1.Equals(_clip2));
            Assert.IsFalse(_clip3.Equals(_clip4));
        }

        [Test]
        public void CheckOperators()
        {
            Assert.IsFalse(_clip3 == _clip4);
            Assert.IsTrue(_clip1 == _clip2);
            Assert.IsFalse(_clip1 != _clip2);
        }
    }
}
