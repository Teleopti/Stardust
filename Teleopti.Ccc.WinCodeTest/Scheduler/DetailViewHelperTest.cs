using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Scheduling;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class DetailViewHelperTest
    {
        private Rectangle _rect;
        [SetUp]
        public void Setup()
        {
            _rect = new Rectangle(0, 5, 20, 10);
        }
        [Test]
        public void CanReturnAbsenceRectangle()
        {
            //Whole day and day off
            Rectangle r = new Rectangle(_rect.X, 0, _rect.Width, DetailViewHelper.AbsenceHeight);
            Assert.AreEqual(r, DetailViewHelper.GetAbsRect(_rect, DisplayMode.WholeDay));
            Assert.AreEqual(r, DetailViewHelper.GetAbsRect(_rect, DisplayMode.DayOff));

            //Begins and ends today
            r = new Rectangle(_rect.X + _rect.Width / 4, 0, _rect.Width - _rect.Width / 2, DetailViewHelper.AbsenceHeight);
            Assert.AreEqual(r, DetailViewHelper.GetAbsRect(_rect, DisplayMode.BeginsAndEndsToday));

            //Begins today
            r = new Rectangle(_rect.X + _rect.Width / 2, 0, _rect.Width / 2, DetailViewHelper.AbsenceHeight);
            Assert.AreEqual(r, DetailViewHelper.GetAbsRect(_rect, DisplayMode.BeginsToday));

            //Ends today
            r = new Rectangle(_rect.X, 0, _rect.Width - _rect.Width / 2, DetailViewHelper.AbsenceHeight);
            Assert.AreEqual(r, DetailViewHelper.GetAbsRect(_rect, DisplayMode.EndsToday));
        }

        [Test]
        public void CanReturnAssignmentRectangle()
        {
            Rectangle r;

            //Begins and ends today
            r = new Rectangle(_rect.X, _rect.Y, _rect.Width, _rect.Height);
            Assert.AreEqual(r, DetailViewHelper.GetAssRect(_rect, DisplayMode.BeginsAndEndsToday));

            //Begins today
            r = new Rectangle(_rect.X + _rect.Width / 2, _rect.Y + 2, _rect.Width, _rect.Height);
            Assert.AreEqual(r, DetailViewHelper.GetAssRect(_rect, DisplayMode.BeginsToday));

            //Ends today
            r = new Rectangle(_rect.X - _rect.Width / 2, _rect.Y + 2, _rect.Width, _rect.Height);
            Assert.AreEqual(r, DetailViewHelper.GetAssRect(_rect, DisplayMode.EndsToday));

            //whole day
            r = new Rectangle(_rect.X, _rect.Y, _rect.Width, _rect.Height);
            Assert.AreEqual(r, DetailViewHelper.GetAssRect(_rect, DisplayMode.WholeDay));
        }

        [Test]
        [ExpectedException(typeof(InvalidEnumArgumentException))]
        public void CanReturnErrorIfInvalidEnumValueIsUsed()
        {
            Rectangle r = DetailViewHelper.GetAssRect(_rect, DisplayMode.DayOff);
            Assert.IsNotNull(r);
        }

        [Test]
        [ExpectedException(typeof(InvalidEnumArgumentException))]
        public void CanReturnErrorIfInvalidEnumValueIsUsedAbs()
        {
            Rectangle r = DetailViewHelper.GetAbsRect(_rect, DisplayMode.None);
            Assert.IsNotNull(r);
        }
    }
}
