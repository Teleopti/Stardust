using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Models
{
    [TestFixture]
    public class ShiftCategoryModelTest
    {
        private ShiftCategoryModel _shiftCategoryModel;
        private IShiftCategory _shiftCategory;
        private string _name;

        [SetUp]
        public void Setup()
        {
            _name = "Shift Category Name";
            _shiftCategory = new ShiftCategory(_name);

            _shiftCategoryModel = new ShiftCategoryModel(_shiftCategory);
        }

        [Test]
        public void VerifyCanSetShortName()
        {
            string newShortName = "Not So Short";
            _shiftCategoryModel.ShortName = newShortName;
            Assert.AreEqual(newShortName, _shiftCategory.Description.ShortName);

        }

        [Test]
        public void VerifyCanSetName()
        {
            string newName = "Ha I got my Name changed";
            _shiftCategoryModel.Name = newName;
            Assert.AreEqual(newName, _shiftCategory.Description.Name);
        }

        [Test]
        public void VerifyCanSetColor()
        {
            Color newColor = Color.DarkSalmon;
            _shiftCategoryModel.DisplayColor = newColor;
            Assert.AreEqual(newColor, _shiftCategory.DisplayColor);
        }

        [Test]
        public void VerifyCanGetShortName()
        {
            string newShortName = "Not So Short";
            _shiftCategoryModel.ShortName = newShortName;
            Assert.AreEqual(newShortName, _shiftCategoryModel.ShortName);

        }

        [Test]
        public void VerifyCanGetName()
        {
            string newName = "Ha I got my Name changed";
            _shiftCategoryModel.Name = newName;
            Assert.AreEqual(newName, _shiftCategoryModel.Name);
        }

        [Test]
        public void VerifyCanGetUpdateTimeText()
        {
            Assert.AreEqual(string.Empty, _shiftCategoryModel.UpdatedTimeInUserPerspective);
        }

        [Test]
        public void VerifyCanGetColor()
        {
            Color newColor = Color.DarkSalmon;
            _shiftCategoryModel.DisplayColor = newColor;
            Assert.AreEqual(newColor, _shiftCategoryModel.DisplayColor);
        }

        [Test]
        public void VerifyCanGetContainedEntity()
        {
            ShiftCategoryModel adpt = new ShiftCategoryModel(_shiftCategory);
            Assert.AreEqual(adpt.ContainedEntity, _shiftCategory);
        }

        [Test]
        public void VerifyCanGetUpdateInfo()
        {
            ShiftCategoryModel adpt = new ShiftCategoryModel(_shiftCategory);
            Assert.AreEqual(adpt.UpdatedBy, _shiftCategory.UpdatedBy);
            Assert.AreEqual(adpt.UpdatedOn, _shiftCategory.UpdatedOn);
        }
    }
}
