using System;
using System.Reflection;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Common
{
    /// <summary>
    /// Tests for AggregateRoot
    /// </summary>
    [TestFixture]
	[TestWithStaticDependenciesDONOTUSE]
	public class AggregateRootTest
    {
        private AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit _targetAggregateRootWithBusinessUnit;

        /// <summary>
        /// Determines whether this instance [can set id].
        /// </summary>
        [Test]
        public void CanSetId()
        {
            Guid newId = Guid.NewGuid();
			AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit target = new AggRootWithBusinessUnit();
            ((IEntity) target).SetId(newId);
            Assert.AreEqual(newId, target.Id);
        }

        /// <summary>
        /// Verifies the business unit can be read.
        /// </summary>
        [Test]
        public void VerifyBusinessUnitCanBeRead()
        {
			AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit target = new AggRootWithBusinessUnit();
            Assert.AreEqual(BusinessUnitUsedInTests.BusinessUnit, target.BusinessUnit);
        }


        [Test]
        public void VerifyDeleted()
        {
            IDeleteTag target = new AggRootWithNoBusinessUnit();
            Assert.IsFalse(target.IsDeleted);
            target.SetDeleted();
            Assert.IsTrue(target.IsDeleted);
        }

        [Test]
        public void VerifySetVersion()
        {
            _targetAggregateRootWithBusinessUnit = new AggRootWithBusinessUnit();
            _targetAggregateRootWithBusinessUnit.SetVersion(3);
            Assert.AreEqual(3, _targetAggregateRootWithBusinessUnit.Version);
        }

		 [Test]
		 public void ClearIdShouldClearCreatedAndUpdatedInfo()
		 {
			 var target = new AggRootWithBusinessUnit();
		 	var aggRootType = typeof (AggregateRoot_Events_ChangeInfo);
			 aggRootType.GetField("_updatedOn", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, DateTime.Now);
			 aggRootType.GetField("_updatedBy", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, new Person());
		 	((IEntity)target).SetId(Guid.NewGuid());

		 	target.ClearId();

		 	target.Id.HasValue.Should().Be.False();
			target.UpdatedBy.Should().Be.Null();
			target.UpdatedOn.HasValue.Should().Be.False();
		 }

        internal class AggRootWithNoBusinessUnit : AggregateRoot_Events_ChangeInfo, IDeleteTag
        {
            private bool _isDeleted;

            public virtual bool IsDeleted
            {
                get { return _isDeleted; }
            }

            public virtual void SetDeleted()
            {
                _isDeleted = true;
            }
        }


		internal class AggRootWithBusinessUnit : AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit, IDeleteTag
        {
            private bool _isDeleted;

            public virtual bool IsDeleted
            {
                get { return _isDeleted; }
            }

            public virtual void SetDeleted()
            {
                _isDeleted = true;
            }
        }
		internal class CreatedAndChangedTest : AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit, IDeleteTag
        {
            private bool _isDeleted;

            public override IPerson UpdatedBy
            {
                get
                {
                    IPerson person = new Person();
                    person.SetName(new Name("Jean Pierre", " Barda"));
                    return person;
                }
            }
            public override DateTime? UpdatedOn
            {
                get
                {
                    return DateTime.Now;
                }
            }

            public virtual bool IsDeleted
            {
                get { return _isDeleted; }
            }

            public virtual void SetDeleted()
            {
                _isDeleted = true;
            }
        }
    }
}