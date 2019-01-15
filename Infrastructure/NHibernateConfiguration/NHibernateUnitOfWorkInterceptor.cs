using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Collection;
using NHibernate.Type;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	// Dont ever use for analytics!
	public class NHibernateUnitOfWorkInterceptor : EmptyInterceptor
	{
		private readonly IUpdatedBy _updatedBy;
		private const string createdByPropertyName = "CREATEDBY";
		private const string createdOnPropertyName = "CREATEDON";
		private const string updatedByPropertyName = "UPDATEDBY";
		private const string updatedOnPropertyName = "UPDATEDON";
		private readonly ICollection<IRootChangeInfo> modifiedRoots = new HashSet<IRootChangeInfo>();
		private readonly ICollection<IAggregateRoot> rootsWithModifiedChildren = new HashSet<IAggregateRoot>();
		private readonly ICurrentPreCommitHooks _currentPreCommitHooks;

		public NHibernateUnitOfWorkInterceptor(IUpdatedBy updatedBy, ICurrentPreCommitHooks currentPreCommitHooks)
		{
			_updatedBy = updatedBy;
			_currentPreCommitHooks = currentPreCommitHooks;
			Iteration = InterceptorIteration.Normal;
		}

		public void Clear()
		{
			rootsWithModifiedChildren.Clear();
			modifiedRoots.Clear();
			Iteration = InterceptorIteration.Normal;
		}

		public InterceptorIteration Iteration { get; set; }
		
		public IEnumerable<IRootChangeInfo> ModifiedRoots => modifiedRoots;

		public override int[] FindDirty(object entity, object id, object[] currentState, object[] previousState,
												  string[] propertyNames, IType[] types)
		{
			int[] retVal = null;
			if (Iteration == InterceptorIteration.UpdateRoots)
			{
				if (entity is IAggregateRoot root && rootsWithModifiedChildren.Contains(root) && !modifiedRootsContainsRoot(root))
				{
					IDictionary<string, int> props = propertyIndexesForUpdate(propertyNames);
					retVal = new[] { props[updatedByPropertyName], props[updatedOnPropertyName] };
				}
				else
				{
					retVal = new int[0];
				}
			}
			return retVal;
		}

		public override void OnCollectionUpdate(object collection, object key)
		{
			var modifiedColl = (IPersistentCollection) collection;
			var owner = modifiedColl.Owner;
			if (owner is IAggregateRoot aggregateRoot)
			{
				modifiedRoots.Add(new RootChangeInfo(aggregateRoot, DomainUpdateType.Update));
			}
			else
			{
				//THIS IS WRONG - doesn't have to be IAggregateEntity any longer. Fix when we find out when this code is run
				var entityOwner = (IAggregateEntity)owner;
				rootsWithModifiedChildren.Add(entityOwner.Root());
			}
		}

		public override void OnDelete(object entity, object id, object[] state, string[] propertyNames, IType[] types)
		{
			if (entity is IAggregateRoot root)
			{
				modifiedRoots.Add(new RootChangeInfo(root, DomainUpdateType.Delete));
				return;
			}

			if (entity is IAggregateEntity aggregateEntity)
			{
				rootsWithModifiedChildren.Add(aggregateEntity.Root());
			}
		}

		public override bool OnFlushDirty(object entity, object id, object[] currentState, object[] previousState,
													 string[] propertyNames, IType[] types)
		{
			var ent = entity as IEntity;
			var entityToCheck = entity as IRestrictionChecker;
			entityToCheck?.CheckRestrictions();

			if (ent is IAggregateRoot root)
			{
				_currentPreCommitHooks.Current().ForEach(cph => cph.BeforeCommit(root, propertyNames, currentState));
				setUpdatedInfo(root, currentState, propertyNames);
				return true;
			}
			if (Iteration == InterceptorIteration.Normal)
				markRoot(ent);

			return false;
		}

		public override bool OnSave(object entity, object id, object[] state, string[] propertyNames, IType[] types)
		{
			var ret = false;
			var ent = entity as IEntity;
			var entityToCheck = entity as IRestrictionChecker;
			entityToCheck?.CheckRestrictions();

			markRoot(ent);

			if (setCreatedProperties(entity, propertyNames, state))
			{
				ret = true;
			}

			if (entity is IAggregateRoot root)
			{
				_currentPreCommitHooks.Current().ForEach(cph => cph.BeforeCommit(root, propertyNames, state));
				modifiedRoots.Add(new RootChangeInfo(root, DomainUpdateType.Insert));
			}
				
			if (setUpdatedProperties(entity, propertyNames, state))
			{
				ret = true;
			}

			return ret;
		}

		private static IDictionary<string, int> propertyIndexesForInsert(IEnumerable<string> properties)
		{
			var listOfProperties = properties.ToList();

			IDictionary<string, int> result = new Dictionary<string, int>();

			int index =
				listOfProperties.FindIndex(p => createdByPropertyName.Equals(p, StringComparison.InvariantCultureIgnoreCase));
			if (index >= 0) result.Add(createdByPropertyName, index);

			index = listOfProperties.FindIndex(p => createdOnPropertyName.Equals(p, StringComparison.InvariantCultureIgnoreCase));
			if (index >= 0) result.Add(createdOnPropertyName, index);

			return result;
		}

		private static IDictionary<string, int> propertyIndexesForUpdate(IEnumerable<string> properties)
		{
			List<string> listOfProperties = properties.ToList();

			IDictionary<string, int> result = new Dictionary<string, int>();

			int index =
				listOfProperties.FindIndex(p => p.Equals(updatedByPropertyName, StringComparison.InvariantCultureIgnoreCase));
			if (index >= 0) result.Add(updatedByPropertyName, index);

			index = listOfProperties.FindIndex(p => p.Equals(updatedOnPropertyName, StringComparison.InvariantCultureIgnoreCase));
			if (index >= 0) result.Add(updatedOnPropertyName, index);

			return result;
		}

		private void markRoot(IEntity ent)
		{
			if (!(ent is IAggregateEntity aggEnt)) 
				return;
			var root = aggEnt.Root();
			rootsWithModifiedChildren.Add(root);
		}

		private bool modifiedRootsContainsRoot(IAggregateRoot root)
		{
			foreach (RootChangeInfo modifiedRoot in modifiedRoots)
			{
				if (modifiedRoot.Root.Equals(root))
					return true;
			}
			return false;
		}

		private bool setCreatedProperties(object root, IEnumerable<string> propertyNames, IList<object> state)
		{
			if (root is ICreateInfo)
			{
				var nu = DateTime.UtcNow;
				var props = propertyIndexesForInsert(propertyNames);
				state[props[createdByPropertyName]] = _updatedBy.Person();
				state[props[createdOnPropertyName]] = nu;
				return true;
			}
			return false;
		}

		private void setUpdatedInfo(IAggregateRoot root, object[] currentState, string[] propertyNames)
		{
			setUpdatedProperties(root, propertyNames, currentState);

			if (root is IDeleteTag deleteInfo && deleteInfo.IsDeleted)
				modifiedRoots.Add(new RootChangeInfo(root, DomainUpdateType.Delete));
			else
				modifiedRoots.Add(new RootChangeInfo(root, DomainUpdateType.Update));
		}

		private bool setUpdatedProperties(object root, IEnumerable<string> propertyNames, object[] currentState)
		{
			if (root is IChangeInfo)
			{
				var nu = DateTime.UtcNow;
				var props = propertyIndexesForUpdate(propertyNames);
				currentState[props[updatedByPropertyName]] = _updatedBy.Person();
				currentState[props[updatedOnPropertyName]] = nu;
				return true;
			}
			return false;
		}
	}
}
 