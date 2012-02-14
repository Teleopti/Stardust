using System.Data;
using System.Reflection;
using NHibernate;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.DomainTest.FakeData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
    [TestFixture]
    public class ReadUncommittedTest : DatabaseTest
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), Test]
        public void VerifyCanReadUncommittedWhenLockInDatabase()
        {
            IPerson person = PersonFactory.CreatePerson();
            person.SetId(null);

            try
            {
                SkipRollback();
                UnitOfWork.PersistAll();
                UnitOfWork.Dispose();
                using (IUnitOfWork uowWrite = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
                {
                    var session = (ISession) uowWrite.GetType().GetProperty("Session",
                                                                            BindingFlags.Instance |
                                                                            BindingFlags.NonPublic).GetValue(uowWrite,
                                                                                                             null);
                    ITransaction transaction = session.BeginTransaction(IsolationLevel.Serializable);
                    session.SaveOrUpdate(person);
                    session.Flush();
                    session.Evict(person);

                    using (IUnitOfWork uowRead = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenReadOnlyUnitOfWork(IsolationLevel.ReadUncommitted))
                    {
                        IPersonRepository rep = new PersonRepository(uowRead);
                        var persons = rep.FindAllSortByName();
                        Assert.GreaterOrEqual(persons.Count,1);
                    }
                    transaction.Rollback();
                }
            }
            catch
            {
                Assert.Fail("No locks should be applied when reading uncommitted data.");
            }
        }
    }
}
