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
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Wfm.Adherence.Configuration.Repositories;

namespace Teleopti.Wfm.Adherence.Test.Configuration.Infrastructure
{
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

		[Test]
		public void VerifyIncorrectBusinessUnitIsNotReadable()
		{
			var correct = CreateAggregateWithCorrectBusinessUnit();
			var buRef = correct as IBelongsToBusinessUnit;
			var buRefId = correct as IBelongsToBusinessUnitId;
			if (buRef != null || buRefId != null)
			{
				PersistAndRemoveFromUnitOfWork(correct);

				var buTemp = new BusinessUnit("dummy");
				PersistAndRemoveFromUnitOfWork(buTemp);
				var inCorrect = CreateAggregateWithCorrectBusinessUnit();
				PersistAndRemoveFromUnitOfWork(inCorrect);

				var retList = rep.LoadAll();
				if (buRef != null)
					Assert.IsTrue(retList.All(r => ((IBelongsToBusinessUnit) r).BusinessUnit.Equals(buRef.BusinessUnit)));
				if (buRefId != null)
					Assert.IsTrue(retList.All(r => ((IBelongsToBusinessUnitId) r).BusinessUnit.Equals(buRefId.BusinessUnit)));
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
			var aggregate = CreateAggregateWithCorrectBusinessUnit();
			if (aggregate is IBelongsToBusinessUnit || aggregate is IBelongsToBusinessUnitId)
				try
				{
					Session.CreateCriteria(aggregate.GetType())
						.Add(Restrictions.Eq("BusinessUnit", null))
						.List();
				}
				catch (QueryException)
				{
					Assert.Fail("Type " + aggregate.GetType().Name + " implements IBelongsToBusinessUnit. Remember to map BU and corresponding filter in mapping file !");
				}
		}

		[Test]
		public void CanLoadAndPropertiesAreSet()
		{
			var id = Guid.NewGuid();
			Session.Save(simpleEntity, id);
			Session.Flush();
			Session.Evict(simpleEntity);
			var loadedAggregate = rep.Load(id);
			Assert.AreEqual(id, loadedAggregate.Id);
			if (loadedAggregate is IBelongsToBusinessUnit buRef)
				Assert.AreEqual(BusinessUnitFactory.BusinessUnitUsedInTest, buRef.BusinessUnit);
			if (loadedAggregate is IBelongsToBusinessUnitId buId)
				Assert.AreEqual(BusinessUnitFactory.BusinessUnitUsedInTest.Id.Value, buId.BusinessUnit);
			VerifyAggregateGraphProperties(simpleEntity, loadedAggregate);
		}

		[Test]
		public void CanGetAndPropertiesAreSet()
		{
			var id = Guid.NewGuid();
			Session.Save(simpleEntity, id);
			Session.Flush();
			Session.Evict(simpleEntity);
			var loadedAggregate = rep.Get(id);
			Assert.AreEqual(id, loadedAggregate.Id);
			if (loadedAggregate is IBelongsToBusinessUnit buRef)
				Assert.AreEqual(BusinessUnitFactory.BusinessUnitUsedInTest, buRef.BusinessUnit);
			if (loadedAggregate is IBelongsToBusinessUnitId buId)
				Assert.AreEqual(BusinessUnitFactory.BusinessUnitUsedInTest.Id.Value, buId.BusinessUnit);
			VerifyAggregateGraphProperties(simpleEntity, loadedAggregate);
		}

		[Test]
		public void VerifyAddAndRemoveWorks()
		{
			T entity = CreateAggregateWithCorrectBusinessUnit();
			rep.Add(entity);
			Assert.IsTrue(UnitOfWork.Contains(entity));
			Assert.IsTrue(UnitOfWork.IsDirty());
			UnitOfWork.Remove(entity);
			Assert.IsFalse(UnitOfWork.Contains(entity));
		}

		[Test]
		public void VerifyAddRangeWorks()
		{
			var entity1 = CreateAggregateWithCorrectBusinessUnit();
			var entity2 = CreateAggregateWithCorrectBusinessUnit();
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
			var root = CreateAggregateWithCorrectBusinessUnit();
			if (!isMutable(root)) return;

			rep.Add(root);
			Session.Flush();

			rep.Remove(root);
			Session.Flush();

			Assert.IsFalse(rep.LoadAll().Contains(root));
		}

		[Test]
		public void VerifyCreatedPropSetsCorrectly()
		{
			var nu = DateTime.UtcNow;
			if (simpleEntity is ICreateInfo)
			{
				rep.Add(simpleEntity);
				Session.Flush();
				Session.Evict(simpleEntity);
				var id = simpleEntity.Id.Value;
				var loadedEntity = (ICreateInfo) rep.Load(id);

				Assert.Less(nu.Subtract(new TimeSpan(0, 0, 1)), loadedEntity.CreatedOn);
				Assert.Greater(nu.AddMinutes(1), loadedEntity.CreatedOn);
				Assert.AreEqual(LoggedOnPerson.Id, loadedEntity.CreatedBy.Id);
			}
			else
			{
				var entityType = simpleEntity.GetType();
				Assert.IsFalse(Session.SessionFactory.GetClassMetadata(entityType).PropertyNames.Contains("CreatedBy"),
					"CreatedBy prop found - have you forgot to impl IChangeInfo for " + entityType.Name + "?");
			}
		}

		[Test]
		public void VerifyAddingWhileLoggedOut()
		{
			var entity = CreateAggregateWithCorrectBusinessUnit();
			Logout();
			switch (entity)
			{
				case IChangeInfo _:
				case IBelongsToBusinessUnit _:
					Assert.Throws<PermissionException>(() => rep.Add(entity));
					break;
				default:
					rep.Add(entity);
					break;
			}
		}

		protected virtual void ConcreteSetup()
		{
		}

		protected abstract T CreateAggregateWithCorrectBusinessUnit();

		protected abstract void VerifyAggregateGraphProperties(T saved, T loaded);

		protected abstract Repository<T> TestRepository(ICurrentUnitOfWork currentUnitOfWork);

		private bool isMutable(T root)
		{
			return Session.SessionFactory.GetAllClassMetadata()[root.GetType().ToString()].IsMutable;
		}
	}
}