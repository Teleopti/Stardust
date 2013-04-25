﻿using NUnit.Framework;
using Teleopti.Ccc.Domain.Security;

namespace Teleopti.Ccc.DomainTest.Security
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Exp"), TestFixture]
    public class RegExpSpecificationTest
    {
        private RegExpSpecification target;

     

        [Test]
        public void VerifyIsSatisfiedByTrue()
        {
            target = new RegExpSpecification(@"[.{10,}]");
            Assert.IsTrue(target.IsSatisfiedBy("vilkaTeckenSomHelstMenÖver10"));
            Assert.IsFalse(target.IsSatisfiedBy("apa"));

            target = new RegExpSpecification(@"[!”#¤%&/()=\?]");
            Assert.IsTrue(target.IsSatisfiedBy("%"));
        }
    }

    
}
