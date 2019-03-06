using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NHibernate;
using NHibernate.Criterion;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    /// <summary>
	/// Base class for repository tests
    /// </summary>
    public abstract class RepositoryTest<T> : DatabaseTest where T : IAggregateRoot
    {
        private Repository<T> rep;
        private T simpleEntity;

        protected override void SetupForRepositoryTest()
        {
            rep = TestRepository(new ThisUnitOfWork(UnitOfWork));
            ConcreteSetup();
            simpleEntity = CreateAggregateWithCorrectBusinessUnit();
        }

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
			if (correct is IFilterOnBusinessUnit buRef)
            {
                PersistAndRemoveFromUnitOfWork(correct);
				
                IState newStateMock = new FakeState();
                BusinessUnit buTemp = new BusinessUnit("dummy");
                PersistAndRemoveFromUnitOfWork(buTemp);
								StateHolderProxyHelper.ClearAndSetStateHolder(LoggedOnPerson, buTemp, SetupFixtureForAssembly.ApplicationData, SetupFixtureForAssembly.DataSource, newStateMock);
                T inCorrect = CreateAggregateWithCorrectBusinessUnit();
                PersistAndRemoveFromUnitOfWork(inCorrect);

                var retList = rep.LoadAll();
                Assert.IsTrue(retList.OfType<IFilterOnBusinessUnit>().All(r => r.GetOrFillWithBusinessUnit_DONTUSE().Equals(buRef.GetOrFillWithBusinessUnit_DONTUSE())));
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
			if (correct is IFilterOnBusinessUnit)
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
        
        [Test]
        public void CanLoadAndPropertiesAreSet()
        {
            Guid id = Guid.NewGuid();
            Session.Save(simpleEntity, id);
            Session.Flush();
            Session.Evict(simpleEntity); //ändrat!
            T loadedAggregate = rep.Load(id);
            Assert.AreEqual(id, loadedAggregate.Id);
			if (loadedAggregate is IFilterOnBusinessUnit buRef)
			{
				Assert.AreSame(BusinessUnitUsedInTests.BusinessUnit, buRef.BusinessUnit);
				Assert.AreSame(BusinessUnitUsedInTests.BusinessUnit, buRef.GetOrFillWithBusinessUnit_DONTUSE());
			}
            VerifyAggregateGraphProperties(loadedAggregate);
        }

        [Test]
        public void CanGetAndPropertiesAreSet()
        {
            Guid id = Guid.NewGuid();
            Session.Save(simpleEntity, id);
            Session.Flush();
            Session.Evict(simpleEntity); //ändrat!
            T loadedAggregate = rep.Get(id);
            Assert.AreEqual(id, loadedAggregate.Id);
			if (loadedAggregate is IFilterOnBusinessUnit buRef)
			{
				Assert.AreSame(BusinessUnitUsedInTests.BusinessUnit, buRef.BusinessUnit);
				Assert.AreSame(BusinessUnitUsedInTests.BusinessUnit, buRef.GetOrFillWithBusinessUnit_DONTUSE());
			}
            VerifyAggregateGraphProperties(loadedAggregate);
        }

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

        [Test]
        public virtual void VerifyAddRangeWorks()
        {
            T entity1 = CreateAggregateWithCorrectBusinessUnit();
            T entity2 = CreateAggregateWithCorrectBusinessUnit();
			IList<T> entList = new List<T>
			{
				entity1,
				entity2
			};
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

	    [Test]
	    public void VerifyAddingWhileLoggedOut()
	    {
		    var entity = CreateAggregateWithCorrectBusinessUnit();
		    Logout();
			if (entity is IChangeInfo)
				Assert.Throws<PermissionException>(() => rep.Add(entity));
			else if (entity is IFilterOnBusinessUnit)
				Assert.Throws<PermissionException>(() => rep.Add(entity));
		    else
				rep.Add(entity);
		}

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

        protected abstract Repository<T> TestRepository(ICurrentUnitOfWork currentUnitOfWork);

        private bool isMutable(T root)
        {
            return Session.SessionFactory.GetAllClassMetadata()[root.GetType().ToString()].IsMutable;
        }
    }
}