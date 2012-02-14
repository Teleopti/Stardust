using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;

namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class ClipTest
    {
        Clip _clip;
        Clip _clip2;
        //ClipboardAssignment _clipAssignment;
        string _stringObject;
   
        [Test]
        public void CanCreateClip()
        {
            //_clipAssignment = new ClipboardAssignment();
            _stringObject = "test";

            _clip = new Clip(0, 0, _stringObject);
            Assert.IsNotNull(_clip);
        }

        [Test]
        public void CheckGetSet()
        {
            //_clipAssignment = new ClipboardAssignment();

            _clip2 = new Clip(1, 2, _stringObject);

            Assert.AreEqual(1, _clip2.RowOffset);
            Assert.AreEqual(2, _clip2.ColOffset);
            Assert.AreSame(_stringObject, _clip2.ClipObject);
        }
    }
}