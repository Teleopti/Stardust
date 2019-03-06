#region Imports

using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

#endregion

namespace Teleopti.Ccc.DomainTest.Scheduling
{
    /// <summary>
    /// Tests for the DayOff class
    /// </summary>
    /// <remarks>
    /// Created by: shirang
    /// Created date: 2008-10-29
    /// </remarks>
    [TestFixture]
    public class DayOffTest
    {
        private DayOffTemplate _target;
        private Description _description;
        private TimeSpan _timeSpanTargetLength;
        private TimeSpan _timeSpanFlexibility;
        private TimeSpan _timeSpanAnchor;

        /// <summary>
        /// Runs once per test
        /// </summary>
        /// <remarks>
        /// Created by: shirang
        /// Created date: 2008-10-29
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            _timeSpanTargetLength = new TimeSpan(24, 0, 0);
            _timeSpanFlexibility = new TimeSpan(8, 0, 0);
            _timeSpanAnchor = new TimeSpan(3, 3, 3, 3);
            _description = new Description("Day Off Test");
            _target = new DayOffTemplate(_description);
            
           _target.SetTargetAndFlexibility(_timeSpanTargetLength, _timeSpanFlexibility);
            _target.Anchor = _timeSpanAnchor;
            ((IDayOffTemplate)_target).SetId(Guid.NewGuid());
        }

        /// <summary>
        /// Verify that new and properties work
        /// </summary>
        /// <remarks>
        /// Created by: shirang
        /// Created date: 2008-10-29
        /// </remarks>
        [Test]
        public void VerifyDefaultProperties()
        {
            Assert.AreEqual(_description.Name, _target.Description.Name);
            Assert.AreEqual(_timeSpanTargetLength, _target.TargetLength);
            Assert.AreEqual(_timeSpanFlexibility, _target.Flexibility);
            Assert.AreEqual(_timeSpanAnchor, _target.Anchor);
            Assert.IsNull(_target.UpdatedBy);
            Assert.IsNull(_target.UpdatedOn);
        }

        /// <summary>
        /// Verifies the name of the can set.
        /// </summary>
        /// <remarks>
        /// Created by: shirang
        /// Created date: 2008-10-29
        /// </remarks>
        [Test]
        public void VerifyCanSetDescription()
        {
            var newDesc = new Description("New Description");
            _target.ChangeDescription(newDesc.Name, newDesc.ShortName);

            Assert.AreEqual(newDesc.Name,_target.Description.Name);
        }
      
        /// <summary>
        /// Verifies the protected constructor works.
        /// </summary>
        /// <remarks>
        /// Created by: shirang
        /// Created date: 2008-10-29
        /// </remarks>
        [Test]
        public void VerifyProtectedConstructorWorks()
        {
            MockRepository mocks = new MockRepository();
            DayOffTemplate internalActivity = mocks.StrictMock<DayOffTemplate>();
            Assert.IsNotNull(internalActivity);
        }

        /// <summary>
        /// Verifies whether the TargetLength property can be set.
        /// </summary>
        /// <remarks>
        /// Created by: shirang
        /// Created date: 2008-10-29
        /// </remarks>
        [Test]
        public void VerifyCanSetTargetLengthAndFlexibility()
        {
            TimeSpan newTargetTimeSpan = new TimeSpan(36, 0, 0);
            TimeSpan newFlexTimeSpan = new TimeSpan(6, 0, 0);
            _target.SetTargetAndFlexibility(newTargetTimeSpan,newFlexTimeSpan);
            Assert.AreEqual(newTargetTimeSpan, _target.TargetLength);
            Assert.AreEqual(newFlexTimeSpan, _target.Flexibility);
        }

       

        /// <summary>
        /// Verifies whether the Anchor property can be set.
        /// </summary>
        /// <remarks>
        /// Created by: shirang
        /// Created date: 2008-10-29
        /// </remarks>
        [Test]
        public void VerifyCanSetAnchor()
        {
            TimeSpan newTimeSpan = new TimeSpan(13, 0, 0);
            _target.Anchor = newTimeSpan;
            Assert.AreEqual(newTimeSpan, _target.Anchor);
        }

        /// <summary>
        /// Verifies whether the ToString returns the Name.
        /// </summary>
        /// <remarks>
        /// Created by: shirang
        /// Created date: 2008-10-29
        /// </remarks>
        [Test]
        public void VerifyToString()
        {
            _target.ChangeDescription(_description.Name, _description.ShortName);
            Assert.AreEqual(_description.Name, _target.ToString());
        }

        [Test]
        public void ShouldCreateCloneWithoutId()
        {
            DayOffTemplate cloneWithoutId = _target.NoneEntityClone();

            Assert.IsNotNull(cloneWithoutId);
            Assert.IsFalse(cloneWithoutId.Id.HasValue);
            Assert.AreNotSame(_target, cloneWithoutId);
            
            Assert.AreEqual(_target.Description, cloneWithoutId.Description);
            Assert.AreEqual(_target.Anchor, cloneWithoutId.Anchor);
            Assert.AreEqual(_target.GetOrFillWithBusinessUnit_DONTUSE(), cloneWithoutId.GetOrFillWithBusinessUnit_DONTUSE());
            Assert.AreEqual(_target.DisplayColor, cloneWithoutId.DisplayColor);
            Assert.AreEqual(_target.Flexibility, cloneWithoutId.Flexibility);
            Assert.AreEqual(_target.PayrollCode, cloneWithoutId.PayrollCode);
            Assert.AreEqual(_target.TargetLength, cloneWithoutId.TargetLength);
            Assert.AreEqual(_target.UpdatedBy, cloneWithoutId.UpdatedBy);
            Assert.AreEqual(_target.UpdatedOn, cloneWithoutId.UpdatedOn);
            Assert.AreEqual(_target.IsDeleted, cloneWithoutId.IsDeleted);
            Assert.AreEqual(_target.Version, cloneWithoutId.Version);
        }

        [Test]
        public void ShouldCreateCloneWithId()
        {
            DayOffTemplate cloneWithId = _target.EntityClone();

            Assert.IsNotNull(cloneWithId);
            Assert.IsTrue(cloneWithId.Id.HasValue);
            Assert.AreNotSame(_target, cloneWithId);
            
            Assert.AreEqual(_target.Id, cloneWithId.Id);
            Assert.AreEqual(_target.Description, cloneWithId.Description);
            Assert.AreEqual(_target.Anchor, cloneWithId.Anchor);
            Assert.AreEqual(_target.GetOrFillWithBusinessUnit_DONTUSE(), cloneWithId.GetOrFillWithBusinessUnit_DONTUSE());
            Assert.AreEqual(_target.DisplayColor, cloneWithId.DisplayColor);
            Assert.AreEqual(_target.Flexibility, cloneWithId.Flexibility);
            Assert.AreEqual(_target.PayrollCode, cloneWithId.PayrollCode);
            Assert.AreEqual(_target.TargetLength, cloneWithId.TargetLength);
            Assert.AreEqual(_target.UpdatedBy, cloneWithId.UpdatedBy);
            Assert.AreEqual(_target.UpdatedOn, cloneWithId.UpdatedOn);
            Assert.AreEqual(_target.IsDeleted, cloneWithId.IsDeleted);
            Assert.AreEqual(_target.Version, cloneWithId.Version);
        }

        [Test]
        public void ShouldCreateClone()
        {
            var clone = _target.Clone() as DayOffTemplate;

            Assert.IsNotNull(clone);
        }
    }
}