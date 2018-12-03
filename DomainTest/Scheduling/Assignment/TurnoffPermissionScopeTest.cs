using System;
using System.Threading;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;


namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    public class TurnoffPermissionScopeTest
    {
        private IPermissionCheck obj;

        [SetUp]
        public void Setup()
        {
            obj = new ScheduleDictionary(new Scenario("cf"), new ScheduleDateTimePeriod(new DateTimePeriod()), new PersistableScheduleDataPermissionChecker(CurrentAuthorization.Make()), CurrentAuthorization.Make());
        }

        [Test]
        public void VerifyNormalCase()
        {
            IScheduleDictionary dic = (IScheduleDictionary) obj;
            Assert.IsTrue(dic.PermissionsEnabled);
            using (TurnoffPermissionScope.For(obj))
            {
                Assert.IsFalse(dic.PermissionsEnabled);                
            }
            Assert.IsTrue(dic.PermissionsEnabled);
        }

        [Test]
        public void CannotSetValueToTheSameValueTwice()
        {
            obj.UsePermissions(false);
            Assert.Throws<InvalidOperationException>(() => obj.UsePermissions(false));
        }

        [Test]
        public void VerifyMultipleThreads()
        {
            //this test verifies that each thread is waiting for turnoffpermissionscope 
            //disposal before entering another instance of turnoffpermissionscope.
            //if not, an exception will fire due to CantSetValueToTheSameValueTwice
            const int noOfThreads = 10;
            var res = new IAsyncResult[noOfThreads];
            IScheduleDictionary dic = (IScheduleDictionary)obj;
            ThreadStart dgate = () =>
                                    {
                                        using (TurnoffPermissionScope.For(obj))
                                        {
                                            Assert.IsFalse(dic.PermissionsEnabled);
                                            Thread.Sleep(5);
                                            Assert.IsFalse(dic.PermissionsEnabled);
                                        }
                                    };
            for (var i = 0; i < noOfThreads; i++)
            {
                res[i] = dgate.BeginInvoke(null, null);
            }
            for (var i = 0; i < noOfThreads; i++)
            {
                dgate.EndInvoke(res[i]);
            }
            Assert.IsTrue(dic.PermissionsEnabled);
        }
    }
}
