using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Shifts;
using Teleopti.Ccc.WinCode.Shifts.Interfaces;
using Teleopti.Ccc.WinCode.Shifts.Models;
using Teleopti.Ccc.WinCode.Shifts.Presenters;
using Teleopti.Interfaces.Domain;
using Rhino.Mocks;

namespace Teleopti.Ccc.WinCodeTest.Shifts
{
    [TestFixture]
    public class GridClassTypeTest
    {
        private GridClassType _target;

        [SetUp]
        public void Setup()
        {
            _target = new GridClassType("Test", typeof(ActivityAbsoluteStartExtender));
        }

        [Test]
        public void VerifyCanAccessProperties()
        {
            Assert.AreEqual("Test", _target.Name);
            Assert.AreEqual(typeof(ActivityAbsoluteStartExtender), _target.ClassType);
        }
    }
}
