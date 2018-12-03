using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Collection
{
    [TestFixture]
    public class SkillStaffPeriodDictionaryTest
    {
        private SkillStaffPeriodDictionary _target;
        private ISkillStaffPeriod _ssp1;
        private ISkillStaffPeriod _ssp2;
        private DateTimePeriod _dtp1;
        private DateTimePeriod _dtp2;
        private DateTimePeriod _dtpWrongKey;
        //private MockRepository _mocks;
        //private ISkillStaffPeriodDictionary _org;
        private ISkill _skill;

        [SetUp]
        public void Setup()
        {
            _skill = SkillFactory.CreateSkill("hej");
            _skill.DefaultResolution = 60;
            _target = new SkillStaffPeriodDictionary(_skill);
            _dtp1 = new DateTimePeriod(new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 11, 0, 0, DateTimeKind.Utc));
            _dtp2 = new DateTimePeriod(new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 13, 0, 0, DateTimeKind.Utc));
            _dtpWrongKey = new DateTimePeriod(2004, 1, 1, 2005, 1, 1);
			_ssp1 = new SkillStaffPeriod(_dtp1, new Task(), new ServiceAgreement());
			_ssp2 = new SkillStaffPeriod(_dtp2, new Task(), new ServiceAgreement());
            _target.Add(_ssp1);
            _target.Add(_ssp2);

        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsFalse(_target.IsReadOnly);
            Assert.AreEqual(_ssp2, _target[_dtp2]);
            Assert.AreEqual(2, _target.Values.Count);
            Assert.AreEqual(2, _target.Keys.Count);
        }

        [Test]
        public void VerifyContainsKeyAndContains()
        {
            Assert.IsTrue(_target.Contains(new KeyValuePair<DateTimePeriod, ISkillStaffPeriod>(_dtp1, _ssp1)));
            Assert.IsFalse(_target.ContainsKey(_dtpWrongKey));
        }

        [Test]
        public void VerifyRemove()
        {
            Assert.AreEqual(2, _target.SkillOpenHoursCollection.Count);
            _target.Remove(new KeyValuePair<DateTimePeriod, ISkillStaffPeriod>(_dtp1, _ssp1));
            Assert.AreEqual(1, _target.SkillOpenHoursCollection.Count);
            Assert.AreEqual(_dtp2, _target.SkillOpenHoursCollection[0]);
        }

        [Test]
        public void VerifyClearAndCount()
        {
            Assert.AreEqual(2, _target.Count);
            Assert.AreEqual(2, _target.SkillOpenHoursCollection.Count);
            _target.Clear();
            Assert.AreEqual(0, _target.Count);
            Assert.AreEqual(0, _target.SkillOpenHoursCollection.Count);
        }

        [Test]
        public void VerifyEnumerator()
        {
            Assert.IsNotNull(_target.GetEnumerator());
            Assert.IsNotNull(((IEnumerable) _target).GetEnumerator());
        }

        [Test]
        public void VerifyInvalidKey()
        {
            _target.Clear();
            Assert.Throws<InvalidConstraintException>(() => _target.Add(_dtp1, _ssp2));
        }

        [Test]
        public void VerifyKeySetNotImplemented()
        {
	        Assert.Throws<NotImplementedException>(() => _target[_dtp1] = _ssp2);
        }

        [Test]
        public void VerifyTryGetValue()
        {
            ISkillStaffPeriod ret;
            _target.TryGetValue(_dtp2, out ret);
            Assert.IsNotNull(ret);
        }

        //[Test]
        //public void VerifyCopyTo()
        //{
        //    KeyValuePair<DateTimePeriod, ISkillStaffPeriod>[] array = new KeyValuePair<DateTimePeriod, ISkillStaffPeriod>[0];
        //    using (_mocks.Record())
        //    {
        //        _org.CopyTo(array, 1);
        //    }
        //    using (_mocks.Playback())
        //    {
        //        _target.CopyTo(array, 1);
        //    }

        //    //KeyValuePair<DateTimePeriod, ISkillStaffPeriod>[] array = new KeyValuePair<DateTimePeriod, ISkillStaffPeriod>[0];
        //    //_target.CopyTo(array, 2);
        //    //Assert.AreEqual(2, array.Length);
        //}

        [Test]
        public void VerifyOpenHoursCollection()
        {
            DateTimePeriod dtp = new DateTimePeriod(new DateTime(2000, 1, 1, 13, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 14, 0, 0, DateTimeKind.Utc));
			ISkillStaffPeriod ssp = new SkillStaffPeriod(dtp, new Task(), new ServiceAgreement());
            Assert.AreEqual(2, _target.SkillOpenHoursCollection.Count);
            _target.Add(ssp);
            Assert.AreEqual(2, _target.SkillOpenHoursCollection.Count);
            dtp = new DateTimePeriod(1998, 1, 1, 1999, 1, 1);
			ssp = new SkillStaffPeriod(dtp, new Task(), new ServiceAgreement());
            _target.Add(ssp);
            Assert.AreEqual(3, _target.SkillOpenHoursCollection.Count);
        }

        [Test]
        public void VerifyTryGetResolutionAdjustedValue()
        {
            DateTimePeriod dtp1 = new DateTimePeriod(new DateTime(2000, 1, 1, 17, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 17, 30, 0, DateTimeKind.Utc));
            IResourceCalculationPeriod ssp;
            Assert.IsFalse(_target.TryGetResolutionAdjustedValue(_skill,dtp1, out ssp));
            dtp1 = new DateTimePeriod(new DateTime(2000, 1, 1, 10, 15, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 10, 30, 0, DateTimeKind.Utc));
            Assert.IsTrue(_target.TryGetResolutionAdjustedValue(_skill, dtp1, out ssp));
            Assert.AreEqual(_dtp1, ((ISkillStaffPeriod)ssp).Period);
        }
    }
}
