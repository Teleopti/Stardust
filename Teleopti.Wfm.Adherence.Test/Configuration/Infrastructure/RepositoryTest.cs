using System;
using System.Linq;
using System.Reflection;
using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Adherence.Configuration.Repositories;

namespace Teleopti.Wfm.Adherence.Test.Configuration.Infrastructure
{
	public abstract class RepositoryTest<T> : DatabaseTest where T : IAggregateRoot
	{
		private T _simpleEntity;

		protected override void SetupForRepositoryTest()
		{
			ConcreteSetup();
			_simpleEntity = CreateAggregateWithCorrectBusinessUnit();
		}

		protected abstract Repository<T> ResolveRepository();

		[Test]
		public void VerifyIncorrectBusinessUnitIsNotReadable()
		{
			var correct = CreateAggregateWithCorrectBusinessUnit();
			var buRef = correct as IFilterOnBusinessUnit;
			var buRefId = correct as IFilterOnBusinessUnitId;
			if (buRef != null || buRefId != null)
			{
				PersistAndRemoveFromUnitOfWork(correct);

				var buTemp = new BusinessUnit("dummy");
				PersistAndRemoveFromUnitOfWork(buTemp);
				var inCorrect = CreateAggregateWithCorrectBusinessUnit();
				PersistAndRemoveFromUnitOfWork(inCorrect);

				var retList = ResolveRepository().LoadAll();
				if (buRef != null)
					Assert.IsTrue(retList.All(r => ((IFilterOnBusinessUnit) r).GetOrFillWithBusinessUnit_DONTUSE().Equals(buRef.GetOrFillWithBusinessUnit_DONTUSE())));
				if (buRefId != null)
					Assert.IsTrue(retList.All(r => ((IFilterOnBusinessUnitId) r).BusinessUnit.Equals(buRefId.BusinessUnit)));
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
			if (aggregate is IFilterOnBusinessUnit || aggregate is IFilterOnBusinessUnitId)
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
			Session.Save(_simpleEntity, id);
			Session.Flush();
			Session.Evict(_simpleEntity);
			var loadedAggregate = ResolveRepository().Load(id);
			Assert.AreEqual(id, loadedAggregate.Id);
			if (loadedAggregate is IFilterOnBusinessUnit buRef)
				Assert.AreEqual(BusinessUnit, buRef.BusinessUnit);
			if (loadedAggregate is IFilterOnBusinessUnitId buId)
				Assert.AreEqual(BusinessUnit.Id.Value, buId.BusinessUnit);
			VerifyAggregateGraphProperties(_simpleEntity, loadedAggregate);
		}

		[Test]
		public void CanGetAndPropertiesAreSet()
		{
			var id = Guid.NewGuid();
			Session.Save(_simpleEntity, id);
			Session.Flush();
			Session.Evict(_simpleEntity);
			var loadedAggregate = ResolveRepository().Get(id);
			Assert.AreEqual(id, loadedAggregate.Id);
			if (loadedAggregate is IFilterOnBusinessUnit buRef)
			{
				Assert.AreEqual(BusinessUnit, buRef.BusinessUnit);
				Assert.AreEqual(BusinessUnit, buRef.GetOrFillWithBusinessUnit_DONTUSE());
			}
			if (loadedAggregate is IFilterOnBusinessUnitId buId)
				Assert.AreEqual(BusinessUnit.Id.Value, buId.BusinessUnit);
			VerifyAggregateGraphProperties(_simpleEntity, loadedAggregate);
		}

		[Test]
		public void VerifyAddAndRemoveWorks()
		{
			T entity = CreateAggregateWithCorrectBusinessUnit();
			ResolveRepository().Add(entity);
			Assert.IsTrue(UnitOfWork.Contains(entity));
			Assert.IsTrue(UnitOfWork.IsDirty());
			UnitOfWork.Remove(entity);
			Assert.IsFalse(UnitOfWork.Contains(entity));
		}

		[Test]
		public void VerifyRemovedItemsCannotBeRead()
		{
			var root = CreateAggregateWithCorrectBusinessUnit();
			if (!isMutable(root)) return;

			ResolveRepository().Add(root);
			Session.Flush();

			ResolveRepository().Remove(root);
			Session.Flush();

			Assert.IsFalse(ResolveRepository().LoadAll().Contains(root));
		}

		[Test]
		public void VerifyAddingWhileLoggedOut()
		{
			var entity = CreateAggregateWithCorrectBusinessUnit();
			EndUnitOfWork();
			Logout();
			using (Container.Resolve<IDataSourceScope>().OnThisThreadUse(InfraTestConfigReader.TenantName()))
			{
				OpenUnitOfWork();
				switch (entity)
				{
					case IChangeInfo _:
					case IFilterOnBusinessUnit _:
						Assert.Throws<PermissionException>(() => ResolveRepository().Add(entity));
						break;
					default:
						ResolveRepository().Add(entity);
						break;
				}
				EndUnitOfWork();
			}
		}

		protected virtual void ConcreteSetup()
		{
		}

		protected abstract T CreateAggregateWithCorrectBusinessUnit();

		protected abstract void VerifyAggregateGraphProperties(T saved, T loaded);

		private bool isMutable(T root)
		{
			return Session.SessionFactory.GetAllClassMetadata()[root.GetType().ToString()].IsMutable;
		}
	}
}