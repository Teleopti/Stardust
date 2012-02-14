using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.InfrastructureTest.Licensing
{
    [TestFixture]
    [Category("LongRunning")]
    public class CheckLicenseAtPersistTest
    {

        [Test]
        public void VerifyRootTypesToCheck()
        {
            IDictionary<Type, ICheckLicenseForRootType> dic = new CheckLicenseAtPersist().RootTypesToCheck;
            Assert.IsTrue(dic.IsReadOnly);
            Assert.AreEqual(2, dic.Count);
            Assert.AreEqual(dic[typeof(Person)].GetType(), typeof(CheckLicenseForPerson));
            Assert.AreEqual(dic[typeof(PersonAssignment)].GetType(), typeof(CheckLicenseForPerson));
        }

        [Test]
        public void VerifyOnlyUniqueAndCorrectTypesAreCalled()
        {
            MockRepository mocks = new MockRepository();
            Type okType = typeof (Person);
            Type noType = typeof (Activity);
            ICheckLicenseForRootType okCheck = mocks.StrictMock<ICheckLicenseForRootType>();
            ICheckLicenseForRootType noCheck = mocks.StrictMock<ICheckLicenseForRootType>();
            IUnitOfWork uow = mocks.StrictMock<IUnitOfWork>();

            CheckLicenseAtPersist target = new CheckLicenseAtPersist(new Dictionary<Type, ICheckLicenseForRootType>
                                                                         {
                                                                             {okType, okCheck}, 
                                                                             {noType, noCheck}
                                                                         });
            //setting up rootchangeInfos
            RootChangeInfo changeOk1 = new RootChangeInfo(new Person(), DomainUpdateType.Update);
            RootChangeInfo changeOk2 = new RootChangeInfo(new Person(), DomainUpdateType.Insert);

            using(mocks.Record())
            {
                okCheck.Verify(uow);
            }
            using(mocks.Playback())
            {
                target.Verify(uow, new List<IRootChangeInfo>{changeOk1, changeOk2 });
            }
        }
    }
}
