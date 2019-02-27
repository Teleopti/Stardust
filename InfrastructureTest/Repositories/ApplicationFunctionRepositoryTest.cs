using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture, Category("BucketB")]
    public class ApplicationFunctionRepositoryTest : RepositoryTest<IApplicationFunction>
    {
        /// <summary>
        /// Runs every test. Implemented by repository's concrete implementation.
        /// </summary>
        protected override void ConcreteSetup()
        {
        }

        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IApplicationFunction CreateAggregateWithCorrectBusinessUnit()
        {
            ApplicationFunction real = new ApplicationFunction("LTP");
            return real;
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IApplicationFunction loadedAggregateFromDatabase)
        {
            Assert.IsNotNull(loadedAggregateFromDatabase);
            Assert.AreEqual("LTP", CreateAggregateWithCorrectBusinessUnit().FunctionCode);
        }

        [Test]
        public void VerifyParentCanBeSetAndPersisted()
        {
            ApplicationFunction parent = new ApplicationFunction("parent");
            PersistAndRemoveFromUnitOfWork(parent);
            ApplicationFunction real = new ApplicationFunction("real");
            real.Parent = parent;
            PersistAndRemoveFromUnitOfWork(real);
            //load
            ApplicationFunction loaded = Session.Load<ApplicationFunction>(real.Id.Value);
            Assert.AreEqual(real, loaded);
            Assert.AreEqual(parent, loaded.Parent);
        }

        [Test]
        public void VerifyFindAllSortedByName()
        {
            ApplicationFunction sortOrder2 = new ApplicationFunction("B");
            //sortOrder2.FunctionDescription = "B";
            PersistAndRemoveFromUnitOfWork(sortOrder2);
            ApplicationFunction sortOrder1 = new ApplicationFunction("A");
            //sortOrder1.FunctionDescription = "A";
            PersistAndRemoveFromUnitOfWork(sortOrder1);

            IList<IApplicationFunction> functions = ApplicationFunctionRepository.DONT_USE_CTOR(UnitOfWork).GetAllApplicationFunctionSortedByCode();

            //load
            Assert.AreEqual(functions[0], sortOrder1);
            Assert.AreEqual(functions[1], sortOrder2);
        }

        [Test]
        public void VerifyFindExternalSortedByName()
        {
            ApplicationFunction sortOrder2 = new ApplicationFunction("B");
            sortOrder2.ForeignSource = DefinedForeignSourceNames.SourceMatrix;
            PersistAndRemoveFromUnitOfWork(sortOrder2);

            ApplicationFunction sortOrder1 = new ApplicationFunction("A");
            sortOrder1.ForeignSource = DefinedForeignSourceNames.SourceRaptor;
            PersistAndRemoveFromUnitOfWork(sortOrder1);

            var functions = ApplicationFunctionRepository.DONT_USE_CTOR(UnitOfWork).ExternalApplicationFunctions();

            //load
            functions.Single().Should().Be.EqualTo(sortOrder2);
        }

        [Test]
        public void ShouldGetPropertiesFromDomain()
        {
            ApplicationFunction applicationFunction = new ApplicationFunction(DefinedRaptorApplicationFunctionPaths.OpenRaptorApplication);
            applicationFunction.ForeignSource = DefinedForeignSourceNames.SourceRaptor;
            applicationFunction.ForeignId = DefinedRaptorApplicationFunctionForeignIds.OpenRaptorApplication;
            PersistAndRemoveFromUnitOfWork(applicationFunction);
            
            IList<IApplicationFunction> functions = ApplicationFunctionRepository.DONT_USE_CTOR(UnitOfWork).GetAllApplicationFunctionSortedByCode();

            //load
            Assert.AreEqual(functions[0], applicationFunction);
        }

		  [Test]
		  public void ShouldGetChildFunctions()
		  {
			  ApplicationFunction parent = new ApplicationFunction("Parent");
			  ApplicationFunction child1 = new ApplicationFunction("Child1");
			  child1.Parent = parent;
			  ApplicationFunction child2 = new ApplicationFunction("Child3");
			  child2.Parent = parent;

			  PersistAndRemoveFromUnitOfWork(parent);
			  PersistAndRemoveFromUnitOfWork(child1);
			  PersistAndRemoveFromUnitOfWork(child2);

			  IList<IApplicationFunction> functions = ApplicationFunctionRepository.DONT_USE_CTOR(UnitOfWork).GetChildFunctions(parent.Id.Value);

			  //load
			  Assert.AreEqual(functions.Count, 2);
		  }

        protected override Repository<IApplicationFunction> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return ApplicationFunctionRepository.DONT_USE_CTOR(currentUnitOfWork);
        }
    }
}
