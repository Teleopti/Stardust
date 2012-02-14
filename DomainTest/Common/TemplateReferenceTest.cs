using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
    /// <summary>
    /// Tests for the TemplateReference class
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-06-09
    /// </remarks>
    [TestFixture]
    public class TemplateReferenceTest
    {
        private ITemplateReference target;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-09
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            target = new TemplateReference();
        }

        /// <summary>
        /// Verifies the instance created.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-09W
        /// </remarks>
        [Test]
        public void VerifyInstanceCreated()
        {
            Assert.IsNotNull(target);
            ITemplateReference target2 = new TemplateReference();
            Assert.IsNotNull(target2);
        }

        /// <summary>
        /// Verifies the properties.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-09
        /// </remarks>
        [Test]
        public void VerifyProperties()
        {
            Guid guid = Guid.NewGuid();
            target.TemplateId = guid;
            int versionNumber = 437829;
            target.VersionNumber = versionNumber;
            target.TemplateName = "NewTemplate";

            Assert.AreEqual(guid, target.TemplateId);
            Assert.AreEqual(versionNumber, target.VersionNumber);
            Assert.AreEqual("NewTemplate",target.TemplateName);
            Assert.IsNull(target.DayOfWeek);
        }

        /// <summary>
        /// Verifies the day of week.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-12
        /// </remarks>
        [Test]
        public void VerifyDayOfWeek()
        {
            target.DayOfWeek = DayOfWeek.Thursday;
            string expectedName = string.Format(CultureInfo.CurrentUICulture, "<{0}>",
                                                CultureInfo.CurrentUICulture.DateTimeFormat.GetAbbreviatedDayName(
                                                    DayOfWeek.Thursday).ToUpper(CultureInfo.CurrentUICulture));
            Assert.AreEqual(expectedName,target.TemplateName);

            target.DayOfWeek = null;
            Assert.IsNull(target.DayOfWeek);
        }


        /// <summary>
        /// Verifies the equals method.
        /// </summary>
        [Test]
        public void VerifyEquals()
        {
            Guid id = Guid.NewGuid();
            int versionNumber = 4321;
            string name = "T2";
            target = new TemplateReference(id, versionNumber, name, DayOfWeek.Wednesday);
            ITemplateReference target2 = new TemplateReference(id, versionNumber, name, DayOfWeek.Wednesday);
            Assert.IsTrue(target.Equals(target2));
            Assert.IsFalse(target.Equals(null));
            Assert.IsFalse(target.Equals(new TemplateReference(id, versionNumber + 1, name, DayOfWeek.Wednesday)));
            Assert.IsFalse(target.Equals(new TemplateReference(id, versionNumber, string.Empty, null)));
            Assert.IsFalse(target.Equals((object)new TemplateReference(id, versionNumber, "T3",null)));
            Assert.IsFalse(new TemplateReference(Guid.NewGuid(), versionNumber, "T1",null).Equals(target));
        }

        /// <summary>
        /// Verifies gethashcode works.
        /// </summary>
        [Test]
        public void VerifyGetHashCodeWorks()
        {
            ITemplateReference templateRef = new TemplateReference(Guid.NewGuid(), 4738, "ff",null);
            IDictionary<ITemplateReference, int> dic = new Dictionary<ITemplateReference, int>();
            dic[templateRef] = 5;
            dic[target] = 8;
            Assert.AreEqual(5, dic[templateRef]);
        }

        /// <summary>
        /// Verifies the operators.
        /// </summary>
        [Test]
        public void VerifyOperators()
        {
            Guid id = Guid.NewGuid();
            const int versionNumber1 = 123;
            const int versionNumber2 = 723489;
            Assert.IsTrue(new TemplateReference() == new TemplateReference());
            Assert.IsFalse(new TemplateReference(id, versionNumber1, "",null) == new TemplateReference(id, versionNumber1, "ff",null));
            Assert.IsTrue(new TemplateReference(id, versionNumber1, null,null) != new TemplateReference(Guid.NewGuid(), versionNumber1, null,null));
            Assert.IsTrue(new TemplateReference(id, versionNumber1, null, null) != new TemplateReference(id, versionNumber2, null, null));
        }
    }
}
