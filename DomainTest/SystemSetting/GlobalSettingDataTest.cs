﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SystemSetting
{
    [TestFixture]
    public class GlobalSettingDataTest
    {
        private GlobalSettingData target;

        [SetUp]
        public void Setup()
        {
            target = new GlobalSettingData("Micke65");
        }

        [Test]
        public void VerifyKey()
        {
            Assert.AreEqual("Micke65", target.Key);
        }

        [Test]
        public void VerifyChangeInfoAndVersion()
        {
            Assert.IsNull(target.CreatedBy);
            Assert.IsNull(target.UpdatedBy);
            Assert.IsNull(target.CreatedOn);
            Assert.IsNull(target.UpdatedOn);
            Assert.IsNull(target.Version);
        }

        [Test]
        public void VerifyDeserialize()
        {
            SettingForTest1 original = new SettingForTest1();
            original.Prop1 = "hej";
            original.Prop2 = new List<int> { 1, 2, 3 };
            target.SetValue(original);
            SettingForTest1 newDefault = new SettingForTest1();
            SettingForTest1 returned = target.GetValue(newDefault);
            Assert.AreEqual("hej", returned.Prop1);
            Assert.AreEqual(3, returned.Prop2.Count);
            Assert.AreSame(target, ((ISettingValue)returned).BelongsTo);
        }

        [Test]
        public void VerifyDefaultIsReturnedIfFailToDeserialize()
        {
            SettingForTest1 original = new SettingForTest1();
            original.Prop1 = "hej";
            original.Prop2 = new List<int> { 1, 2, 3 };
            target.SetValue(original);
            SettingForTest2 newDefault = new SettingForTest2();
            newDefault.Prop1 = "hej då";
            newDefault.Prop2 = new List<int> { 1 };
            SettingForTest2 returned = target.GetValue(newDefault);
            Assert.AreEqual("hej då", returned.Prop1);
            Assert.AreEqual(1, returned.Prop2.Count);
            Assert.AreSame(target, ((ISettingValue)returned).BelongsTo);
        }

        [Test]
        public void VerifyDefaultIsReturnedIfNotInitialized()
        {
            SettingForTest2 newDefault = new SettingForTest2();
            newDefault.Prop1 = "hej då";
            newDefault.Prop2 = new List<int> { 1 };
            SettingForTest2 returned = target.GetValue(newDefault);
            Assert.AreEqual("hej då", returned.Prop1);
            Assert.AreEqual(1, returned.Prop2.Count);
            Assert.AreSame(target, ((ISettingValue)returned).BelongsTo);
        }

        [Test, ExpectedException(typeof(SerializationException))]
        public void VerifySerializationExceptionIfSerializationFails()
        {
            NotValid newDefault = new NotValid();
            newDefault.Prop1 = "Hallo";
            target.GetValue(newDefault);
        }

        [Test]
        public void VerifyReturnedDefaultValueIsNewInstance()
        {
            SettingForTest2 newDefault = new SettingForTest2();
            newDefault.Prop1 = "hej då";
            newDefault.Prop2 = new List<int> { 1 };
            SettingForTest2 returned = target.GetValue(newDefault);
            Assert.AreNotSame(newDefault, returned);
            Assert.AreSame(target, ((ISettingValue)returned).BelongsTo);
        }

        [Test]
        public void VerifyBusinessUnit()
        {
            exposingBu s = new exposingBu();
            Assert.IsNotNull(s.BusinessUnit);

            var identity = ((TeleoptiIdentity)TeleoptiPrincipal.Current.Identity);
            Assert.AreSame(identity.BusinessUnit, s.BusinessUnit);
            IBusinessUnit bu = new BusinessUnit("df");
            s.SetBu(bu);
            Assert.AreSame(bu, s.BusinessUnit);

        }
        
        private class exposingBu : GlobalSettingData
        {
            public void SetBu(IBusinessUnit bu)
            {
                BusinessUnit = bu;
            }
        }

        [Serializable]
        private class SettingForTest1 : SettingValue
        {
            public string Prop1 { get; set; }
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
            public IList<int> Prop2 { get; set; }

        }

        [Serializable]
        private class SettingForTest2: SettingValue
        {
            public string Prop1 { get; set; }
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
            public IList<int> Prop2 { get; set; }

        }

        private class NotValid: SettingValue
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public string Prop1 { get; set; }
        }

    }



}
