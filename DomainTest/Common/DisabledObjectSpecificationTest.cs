using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class DisabledObjectSpecificationTest
    {
        private DisabledObjectSpecification<TypeWithDisabledProperty> _target1;
        private DisabledObjectSpecification<TypeWithNoDisabledProperty> _target2;

        [SetUp]
        public void Setup()
        {
            _target1 = new DisabledObjectSpecification<TypeWithDisabledProperty>("Disabled");
            _target2 = new DisabledObjectSpecification<TypeWithNoDisabledProperty>("Disabled");
        }

        [Test]
        public void VerifyTypeWithDisabledProperty()
        {
            TypeWithDisabledProperty typeWithDisabledProperty = new TypeWithDisabledProperty(true);
            Assert.AreEqual(typeWithDisabledProperty.Disabled, _target1.IsSatisfiedBy(typeWithDisabledProperty));
        }

        [Test]
        public void VerifyTypeWithNoDisabledProperty()
        {
            TypeWithNoDisabledProperty typeWithNoDisabledProperty = new TypeWithNoDisabledProperty(true);
            Assert.IsFalse(_target2.IsSatisfiedBy(typeWithNoDisabledProperty));
        }
        
    }

    public class TypeWithDisabledProperty
    {
        public TypeWithDisabledProperty(bool disabled)
        {
            Disabled = disabled;
        }

        public bool Disabled { get; set; }
    }

    public class TypeWithNoDisabledProperty
    {
        public TypeWithNoDisabledProperty(bool isTest)
        {
            IsTest = isTest;
        }

        public bool IsTest { get; set; }
    }
}
