using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    /// <summary>
    /// Test for ExternalLogOnRepository
    /// </summary>
    [TestFixture]
    [Category("BucketB")]
    public class ExternalLogOnRepositoryTest : RepositoryTest<IExternalLogOn>
    {
        /// <summary>
        /// Runs every test. Implemented by repository's concrete implementation.
        /// </summary>
        protected override void ConcreteSetup()
        {
        }

        /// <summary>
        /// Creates an aggreagte using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IExternalLogOn CreateAggregateWithCorrectBusinessUnit()
        {
            return ExternalLogOnFactory.CreateExternalLogOn();
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        /// 
        protected override void VerifyAggregateGraphProperties(IExternalLogOn loadedAggregateFromDatabase)
        {
            IExternalLogOn login = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(login.AcdLogOnName, loadedAggregateFromDatabase.AcdLogOnName);
            Assert.AreEqual(login.AcdLogOnOriginalId, loadedAggregateFromDatabase.AcdLogOnOriginalId);
            Assert.AreEqual(login.AcdLogOnMartId, loadedAggregateFromDatabase.AcdLogOnMartId);
        }

        protected override Repository<IExternalLogOn> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return ExternalLogOnRepository.DONT_USE_CTOR(currentUnitOfWork);
        }

        /// <summary>
        /// Verifies the load all logins.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-18
        /// </remarks>
        [Test]
        public void VerifyLoadAllLogins()
        {
            IExternalLogOn source1 = CreateAggregateWithCorrectBusinessUnit();
            IExternalLogOn source2 = CreateAggregateWithCorrectBusinessUnit();

            PersistAndRemoveFromUnitOfWork(source1);
            PersistAndRemoveFromUnitOfWork(source2);
			

			IList<IExternalLogOn> logins = ExternalLogOnRepository.DONT_USE_CTOR(CurrUnitOfWork).LoadAll().ToList();

            Assert.AreEqual(2,logins.Count);
            Assert.IsTrue(logins.Contains(source1));
            Assert.IsTrue(logins.Contains(source2));
        }

	    [Test]
	    public void VerifyLoadByNames()
	    {
			var source1 = CreateAggregateWithCorrectBusinessUnit();
		    source1.AcdLogOnName = "abc123";
			var source2 = CreateAggregateWithCorrectBusinessUnit();
		    source2.AcdLogOnName = "superman";
			var source3 = CreateAggregateWithCorrectBusinessUnit();
		    source3.AcdLogOnName = "john doe";

			PersistAndRemoveFromUnitOfWork(source1);
			PersistAndRemoveFromUnitOfWork(source2);
			PersistAndRemoveFromUnitOfWork(source3);

			var logins = ExternalLogOnRepository.DONT_USE_CTOR(CurrUnitOfWork).LoadByAcdLogOnNames(new[] {source1.AcdLogOnName, source2.AcdLogOnName});

			Assert.AreEqual(2, logins.Count);
			Assert.IsTrue(logins.Contains(source1));
			Assert.IsTrue(logins.Contains(source2));
		}
    }
}