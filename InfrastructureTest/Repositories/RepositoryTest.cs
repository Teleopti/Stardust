using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NHibernate;
using NHibernate.Criterion;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    /// <summary>
	/// Base class for repository testsPersistAndRemoveFromUnitOfWork
    /// </summary>
    public abstract class RepositoryTest<T> : DatabaseTest where T : IAggregateRoot
    {
        private Repository<T> rep;
        private T simpleEntity;


        #region SetUpAndTeardownMethods
        protected override void SetupForRepositoryTest()
        {
            rep = TestRepository(UnitOfWork);
            ConcreteSetup();
            simpleEntity = CreateAggregateWithCorrectBusinessUnit();
        }
        #endregion

        #region Tests

        /// <summary>
        /// Verifies that incorrect business unit is not readable.
        /// </summary>
        /// <remarks>
        /// Will only be run by types with BelongsToBusinessUnit = true
        /// </remarks>
        [Test]
        public virtual void VerifyIncorrectBusinessUnitIsNotReadable()
        {
            T correct = CreateAggregateWithCorrectBusinessUnit();
            IBelongsToBusinessUnit buRef = correct as IBelongsToBusinessUnit;
            if (buRef!=null)
            {
                PersistAndRemoveFromUnitOfWork(correct);

                MockRepository newMock = new MockRepository();
                IState newStateMock = newMock.StrictMock<IState>();
                BusinessUnit buTemp = new BusinessUnit("dummy");
                PersistAndRemoveFromUnitOfWork(buTemp);
								StateHolderProxyHelper.ClearAndSetStateHolder(newMock, LoggedOnPerson, buTemp, SetupFixtureForAssembly.ApplicationData, SetupFixtureForAssembly.DataSource, newStateMock);
                T inCorrect = CreateAggregateWithCorrectBusinessUnit();
                PersistAndRemoveFromUnitOfWork(inCorrect);

                IList<T> retList = rep.LoadAll();
                Assert.IsTrue(retList.All(r => ((IBelongsToBusinessUnit)r).BusinessUnit.Equals(buRef.BusinessUnit)));
            }
            else
            {

                Assert.IsNull(
                    correct.GetType().GetProperty("BusinessUnit", BindingFlags.Public | BindingFlags.Instance),
                    "Property BusinessUnit exists on " + correct.GetType().Name +
                    ". IBelongsToBusinessUnit is not impl. Have you forgot this?");
            }
        }

        [Test]
        public void VerifyMappedBusinessUnitExists()
        {
            T correct = CreateAggregateWithCorrectBusinessUnit();
            IBelongsToBusinessUnit buRef = correct as IBelongsToBusinessUnit;
            if (buRef!=null)
            {
                try
                {
                    Session.CreateCriteria(correct.GetType())
                        .Add(Restrictions.Eq("BusinessUnit", null))
                        .List();
                }
                catch (QueryException)
                {
                    Assert.Fail("Type " + correct.GetType().Name + " implements IBelongsToBusinessUnit. Remember to map BU and corresponding filter in mapping file !");
                }
            }
        }
        


        /// <summary>
        /// Can not create repository when user not logged on.
        /// </summary>
        [Test]
        public void CannotCallDatabaseWhenNotLoggedOn()
        {
	        if (!rep.ValidateUserLoggedOn)
				Assert.Ignore("this repository should be available even if not logged on");
			Logout();
	        Assert.Throws<PermissionException>(() => rep.Load(Guid.NewGuid()));
        }


        /// <summary>
        /// Determines whether this instance can load entity by id.
        /// Object should not be lazy loaded (agg root)
        /// </summary>
        [Test]
        public void CanLoadAndPropertiesAreSet()
        {
            Guid id = Guid.NewGuid();
            Session.Save(simpleEntity, id);
            Session.Flush();
            Session.Evict(simpleEntity); //�ndrat!
            T loadedAggregate = rep.Load(id);
            Assert.AreEqual(id, loadedAggregate.Id);
            IBelongsToBusinessUnit buRef = loadedAggregate as IBelongsToBusinessUnit;
            if (buRef!=null)
                Assert.AreSame(BusinessUnitFactory.BusinessUnitUsedInTest, buRef.BusinessUnit);
            VerifyAggregateGraphProperties(loadedAggregate);
        }

        [Test]
        public void CanGetAndPropertiesAreSet()
        {
            Guid id = Guid.NewGuid();
            Session.Save(simpleEntity, id);
            Session.Flush();
            Session.Evict(simpleEntity); //�ndrat!
            T loadedAggregate = rep.Get(id);
            Assert.AreEqual(id, loadedAggregate.Id);
            IBelongsToBusinessUnit buRef = loadedAggregate as IBelongsToBusinessUnit;
            if (buRef != null)
                Assert.AreSame(BusinessUnitFactory.BusinessUnitUsedInTest, buRef.BusinessUnit);
            VerifyAggregateGraphProperties(loadedAggregate);
        }


        /// <summary>
        /// Determines whether this instance can load all entities.
        /// Objects should not be lazy loaded (agg root)
        /// </summary>
        [Test]
        public void CanLoadAllEntities()
        {
            T simpleEntity1 = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(simpleEntity1);
            ICollection<T> coll = rep.LoadAll();
            Assert.AreEqual(1, coll.Count);
        }


        /// <summary>
        /// Can count database entities of one type.
        /// </summary>
        [Test]
        public void CanCountDatabaseEntities()
        {
            PersistAndRemoveFromUnitOfWork(simpleEntity);
            Assert.AreEqual(1, rep.CountAllEntities());
        }

        /// <summary>
        /// Verifies the unitofwork is not null when creating a unitofwork.
        /// </summary>
        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void VerifyUnitOfWorkIsNotNull()
        {
            rep = TestRepository(null);
        }

        /// <summary>
        /// Verifies that Add & remove works.
        /// </summary>
        [Test]
        public virtual void VerifyAddAndRemoveWorks()
        {
            T entity = CreateAggregateWithCorrectBusinessUnit();
            rep.Add(entity);
            Assert.IsTrue(UnitOfWork.Contains(entity));
            Assert.IsTrue(UnitOfWork.IsDirty());
            UnitOfWork.Remove(entity);
            Assert.IsFalse(UnitOfWork.Contains(entity));
        }

        /// <summary>
        /// Verifies that addrange() works.
        /// </summary>
        [Test]
        public virtual void VerifyAddRangeWorks()
        {
            T entity1 = CreateAggregateWithCorrectBusinessUnit();
            T entity2 = CreateAggregateWithCorrectBusinessUnit();
            IList<T> entList = new List<T>();
            entList.Add(entity1);
            entList.Add(entity2);
            rep.AddRange(entList);
            Assert.IsTrue(UnitOfWork.Contains(entity1));
            Assert.IsTrue(UnitOfWork.Contains(entity2));
        }


        [Test]
        public void VerifyRemovedItemsCannotBeRead()
        {
            T root = CreateAggregateWithCorrectBusinessUnit();
            if(isMutable(root))
            {
                rep.Add(root);
                Session.Flush();

                rep.Remove(root);
                Session.Flush();

                Assert.IsFalse(rep.LoadAll().Contains(root));                
            }
        }

        [Test]
        public void VerifyCreatedPropSetsCorrectly()
        {
            if(simpleEntity is ICreateInfo)
            {
                DateTime nu = DateTime.UtcNow;
                rep.Add(simpleEntity);
                Session.Flush();
                Session.Evict(simpleEntity);
                Guid id = simpleEntity.Id.Value;
								var loadedEntity = (ICreateInfo)rep.Load(id);

                Assert.Less(nu.Subtract(new TimeSpan(0, 0, 1)), loadedEntity.CreatedOn);
                Assert.Greater(nu.AddMinutes(1), loadedEntity.CreatedOn);
                Assert.AreEqual(LoggedOnPerson.Id, loadedEntity.CreatedBy.Id);
            }
            else
            {
                Type entityType = simpleEntity.GetType();
                Assert.IsFalse(Session.SessionFactory.GetClassMetadata(entityType).PropertyNames.Contains("CreatedBy"), 
                        "CreatedBy prop found - have you forgot to impl IChangeInfo for " + entityType.Name + "?");
            }
        }

        #endregion

        #region Abstract methods

        /// <summary>
        /// Runs every test. Can be overriden by repository's concrete implementation.
        /// </summary>
        protected virtual void ConcreteSetup() { }

    

        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected abstract T CreateAggregateWithCorrectBusinessUnit();


        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected abstract void VerifyAggregateGraphProperties(T loadedAggregateFromDatabase);

        #endregion


        protected abstract Repository<T> TestRepository(IUnitOfWork unitOfWork);

        private bool isMutable(T root)
        {
            return Session.SessionFactory.GetAllClassMetadata()[root.GetType().ToString()].IsMutable;
        }
    }

}