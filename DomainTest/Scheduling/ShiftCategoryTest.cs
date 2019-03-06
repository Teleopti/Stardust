using System;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
    /// <summary>
    /// Tests for ShiftCategory
    /// </summary>
    [TestFixture]
    public class ShiftCategoryTest
    {
        private ShiftCategory target;
        private string name = "Evening";

        /// <summary>
        /// Runs once per test.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            target = new ShiftCategory(name);
            ((IShiftCategory)target).SetId(Guid.NewGuid());
        }

        /// <summary>
        /// Verifies that properties work
        /// </summary>
        [Test]
        public void VerifyDefaultProperties()
        {
            Assert.AreEqual(name, target.Description.Name);
            Assert.IsNull(target.UpdatedBy);
            Assert.IsNull(target.UpdatedOn);
            Assert.IsTrue(target.Id.HasValue);
            Assert.AreEqual(string.Empty, target.Description.ShortName);
            Assert.AreEqual(0, target.DisplayColor.ToArgb());
        }

        /// <summary>
        /// Verifies that properties are set correctly
        /// </summary>
        [Test]
        public void CanSetProperties()
        {
            string localName = "Morning";
            string shortName = "MO";

            Color color = Color.FloralWhite;

            target.Description = new Description(localName,shortName);
            target.DisplayColor = color;

            Assert.AreEqual(localName, target.Description.Name);
            Assert.AreEqual(shortName, target.Description.ShortName);
            Assert.AreEqual(color.ToArgb(), target.DisplayColor.ToArgb());
        }

        /// <summary>
        /// Verifies that default color is set if no color is choosen
        /// </summary>
        [Test]
        public void CanSetDefaultColorWhenNoColor()
        {
            target = new ShiftCategory("Evening");
            target.DisplayColor = Color.Empty;
            Assert.AreEqual(Color.Red.ToArgb(), target.DisplayColor.ToArgb());
        }

        [Test]
        public void ShouldCreateCloneWithoutId()
        {
            ShiftCategory cloneWithoutId = target.NoneEntityClone();

            Assert.IsNotNull(cloneWithoutId);
            Assert.IsFalse(cloneWithoutId.Id.HasValue);
            Assert.AreNotSame(target, cloneWithoutId);

            Assert.AreEqual(target.Description, cloneWithoutId.Description);
            Assert.AreEqual(target.GetOrFillWithBusinessUnit_DONTUSE(), cloneWithoutId.GetOrFillWithBusinessUnit_DONTUSE());
            Assert.AreEqual(target.DisplayColor, cloneWithoutId.DisplayColor);

            Assert.AreEqual(target.UpdatedBy, cloneWithoutId.UpdatedBy);
            Assert.AreEqual(target.UpdatedOn, cloneWithoutId.UpdatedOn);
            Assert.AreEqual(target.IsDeleted, cloneWithoutId.IsDeleted);
            Assert.AreEqual(target.Version, cloneWithoutId.Version);
        }

        [Test]
        public void ShouldCreateCloneWithId()
        {
            ShiftCategory cloneWithId = target.EntityClone();

            Assert.IsNotNull(cloneWithId);
            Assert.IsTrue(cloneWithId.Id.HasValue);
            Assert.AreNotSame(target, cloneWithId);

            Assert.AreEqual(target.Description, cloneWithId.Description);
            Assert.AreEqual(target.GetOrFillWithBusinessUnit_DONTUSE(), cloneWithId.GetOrFillWithBusinessUnit_DONTUSE());
            Assert.AreEqual(target.DisplayColor, cloneWithId.DisplayColor);

            Assert.AreEqual(target.UpdatedBy, cloneWithId.UpdatedBy);
            Assert.AreEqual(target.UpdatedOn, cloneWithId.UpdatedOn);
            Assert.AreEqual(target.IsDeleted, cloneWithId.IsDeleted);
            Assert.AreEqual(target.Version, cloneWithId.Version);
        }

        [Test]
        public void ShouldCreateClone()
        {
            var clone = target.Clone() as ShiftCategory;

            Assert.IsNotNull(clone);
        }

        
    }
}