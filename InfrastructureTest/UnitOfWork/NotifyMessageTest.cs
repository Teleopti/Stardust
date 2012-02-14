using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.DomainTest.Helper;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Messaging.Interfaces.Events;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
    [TestFixture]
    [Category("LongRunning")]
    public class NotifyMessageTest : DatabaseTest
    {
        private IMessageBroker messBroker;
        private MockRepository mocks;
        private IUnitOfWork uow;

        protected override void SetupForRepositoryTest()
        {
            mocks = new MockRepository();
            messBroker = mocks.CreateMock<IMessageBroker>();
            SkipRollback();
            uow = CreateUnitOfWorkWithMessageMock();
        }



        [Test]
        public void VerifyInsertsAreKeptInChangedRootList()
        {
            bool ok = false;
            Person per = new Person();
            per.Name = new Name("peter", "westlin");
            new Repository(UnitOfWork).Add(per);
            foreach (ChangedRootsKey rootsKey in UnitOfWork.PersistAll())
            {
                if(rootsKey.Id==per.Id.Value && rootsKey.RootType.Equals(typeof(Person)))
                {
                    ok = true;
                    break;
                }
            }
            Assert.IsTrue(ok);
            //clean up
            new Repository(UnitOfWork).Remove(per);
            UnitOfWork.PersistAll();
        }

        [Test]
        public void VerifyUpdatesAreKeptInChangedRootListAndMessageBrokerIsNotified()
        {
            
            bool ok = false;
            Activity actTemp = new Activity("for test");
            new ActivityRepository(UnitOfWork).Add(actTemp);
            UnitOfWork.PersistAll();

            Activity act = new ActivityRepository(uow).Load(actTemp.Id.Value);
            act.Description = new Description("changed");


            new Repository(uow).Add(act);

            using(mocks.Record())
            {
                messBroker.SendEventMessage(act.Id.Value, typeof(Activity).Name, DomainUpdateType.Update); //gör om denna till insert
                messBroker.SendEventMessage(act.Id.Value, typeof(Activity).Name, DomainUpdateType.Update);
            }
            using(mocks.Playback())
            {
                foreach (ChangedRootsKey rootsKey in uow.PersistAll())
                {
                    if (rootsKey.Id == act.Id.Value && rootsKey.RootType.Equals(typeof(Activity)))
                    {
                        ok = true;
                        break;
                    }
                }

                Assert.IsTrue(ok);
                //clean up
                new Repository(uow).Remove(act);
                uow.PersistAll();
            }
        }

        private IUnitOfWork CreateUnitOfWorkWithMessageMock()
        {
            return ((NHibernateUnitOfWorkFactory)SetupFixtureForAssembly.DataSources.Application).CreateAndOpenUnitOfWork(messBroker);
        }
    }
}
