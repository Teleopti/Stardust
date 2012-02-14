﻿using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
    //rk - cut/paste from aggregateroottest
    [TestFixture]
    public class LocalizedUpdateInfoTest
    {
        private LocalizedUpdateInfo target;

        [SetUp]
        public void Setup()
        {
            target = new LocalizedUpdateInfo();
        }

        [Test]
        public void VerifyCreatedText()
        {
            ICreateInfo testRoot = new AggregateRootTest.CreatedAndChangedTest();
            string created = target.CreatedText(testRoot, "Created by:");
            Assert.IsTrue(created.Length > 0);
        }
        [Test]
        public void VerifyUpdatedText()
        {
            AggregateRootTest.CreatedAndChangedTest testRoot = new AggregateRootTest.CreatedAndChangedTest();
            string updated = target.UpdatedByText(testRoot, "Updated by:");
            Assert.IsTrue(updated.Length > 0);
        }

        [Test]
        public void VerifyCreatedTextWhenValuesAreNull()
        {
            var rootCreatedTest = new AggregateRootTest.AggRootWithNoBusinessUnit();
            string created = target.CreatedText(rootCreatedTest, "Created by:");
            Assert.IsTrue(created.Length == 0);
        }
        [Test]
        public void VerifyUpdatedTextWhenValuesAreNull()
        {
            var rootCreatedTest = new AggregateRootTest.AggRootWithNoBusinessUnit();
            string updated = target.UpdatedByText(rootCreatedTest, "Updated by:");
            Assert.IsTrue(updated.Length == 0);
        }
        [Test]
        public void VerifyCanGetCreateTimeTextWhenNull()
        {
            var rootCreatedTest = new AggregateRootTest.AggRootWithNoBusinessUnit();
            Assert.AreEqual(string.Empty, target.CreatedTimeInUserPerspective(rootCreatedTest));
        }
        [Test]
        public void VerifyCanGetUpdateTimeTextWhenNull()
        {
            var rootCreatedTest = new AggregateRootTest.AggRootWithNoBusinessUnit();
            Assert.AreEqual(string.Empty, target.UpdatedTimeInUserPerspective(rootCreatedTest));
        }
        [Test]
        public void VerifyCanGetCreateTimeText()
        {
            var testRoot = new AggregateRootTest.CreatedAndChangedTest();
            Assert.IsTrue(!string.IsNullOrEmpty(target.CreatedTimeInUserPerspective(testRoot)));
        }
        [Test]
        public void VerifyCanGetUpdateTimeText()
        {
            var testRoot = new AggregateRootTest.CreatedAndChangedTest();
            Assert.IsTrue(!string.IsNullOrEmpty(target.UpdatedTimeInUserPerspective(testRoot)));
        }
    }
}
