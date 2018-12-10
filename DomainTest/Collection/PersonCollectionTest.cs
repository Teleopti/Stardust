using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Collection
{
    [TestFixture]
    public class PersonCollectionTest
    {
        private IPersonCollection _target;
        private IPerson _p1;
        private IPerson _p2;
        private DateOnly _dt;
        private TimeZoneInfo _timeZoneInfo;

        [SetUp]
        public void Setup()
        {
            _timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            IList<IPerson> data = new List<IPerson>();
            _p1 = PersonFactory.CreatePerson("Kalle", "Kula");
            _p1.PermissionInformation.SetDefaultTimeZone(_timeZoneInfo);
            data.Add(_p1);
            _p2 = PersonFactory.CreatePerson("Kalle1", "Kula1");
            _p2.PermissionInformation.SetDefaultTimeZone(_timeZoneInfo);
            data.Add(_p2);
            _dt = new DateOnly(2000, 1, 1);
            _target = new PersonCollection("func", data, _dt);
        }

        [Test]
        public void VerifyAllPermittedPersons()
        {
            var mocks = new MockRepository();
            var authorization = mocks.StrictMock<IAuthorization>();
            using(mocks.Record())
            {
                Expect.Call(authorization.IsPermitted("func", _dt, _p1)).Return(false);
                Expect.Call(authorization.IsPermitted("func", _dt, _p2)).Return(true);
            }
            using (mocks.Playback())
            {
                using (CurrentAuthorization.ThreadlyUse(authorization))
                {
                    var ret = _target.AllPermittedPersons;
                    Assert.AreEqual(1, ret.Count());
                    Assert.AreEqual(_p2, ret.First());
                }
            }
        }
    }
}
