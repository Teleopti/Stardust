using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Collection;
using NHibernate.Type;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public class AggregateRootInterceptor : EmptyInterceptor
	{
		private readonly IUpdatedBy _updatedBy;
		private const string createdByPropertyName = "CREATEDBY";
		private const string createdOnPropertyName = "CREATEDON";
		private const string updatedByPropertyName = "UPDATEDBY";
		private const string updatedOnPropertyName = "UPDATEDON";
		private readonly ICollection<IRootChangeInfo> modifiedRoots = new HashSet<IRootChangeInfo>();
		private readonly ICollection<IAggregateRoot> rootsWithModifiedChildren = new HashSet<IAggregateRoot>();

		private readonly VersionStateRollbackInterceptor _entityStateRollbackInterceptor = new VersionStateRollbackInterceptor();

		public AggregateRootInterceptor(IUpdatedBy updatedBy)
		{
			_updatedBy = updatedBy;
			Iteration = InterceptorIteration.Normal;
		}

		public void Clear()
		{
			rootsWithModifiedChildren.Clear();
			modifiedRoots.Clear();
			_entityStateRollbackInterceptor.Clear();
			Iteration = InterceptorIteration.Normal;
		}

		public override void AfterTransactionCompletion(ITransaction tx)
		{
			_entityStateRollbackInterceptor.AfterTransactionCompletion(tx);
		}

		public InterceptorIteration Iteration { get; set; }
		
		public IEnumerable<IRootChangeInfo> ModifiedRoots
		{
			get { return modifiedRoots; }
		}

		public IEnumerable<IAggregateRoot> RootsWithModifiedChildren
		{
			get { return rootsWithModifiedChildren; }
		}

		public override int[] FindDirty(object entity, object id, object[] currentState, object[] previousState,
												  string[] propertyNames, IType[] types)
		{
			int[] retVal = null;
			if (Iteration == InterceptorIteration.UpdateRoots)
			{
				var root = entity as IAggregateRoot;
				if (root != null && rootsWithModifiedChildren.Contains(root) && !modifiedRootsContainsRoot(root))
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
			var aggregateRoot = owner as IAggregateRoot;
			if (aggregateRoot != null)
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
			var root = entity as IAggregateRoot;
			if (root != null)
			{
				modifiedRoots.Add(new RootChangeInfo(root, DomainUpdateType.Delete));
				return;
			}
			var aggregateEntity = entity as IAggregateEntity;
			if (aggregateEntity != null)
			{
				rootsWithModifiedChildren.Add(aggregateEntity.Root());
			}
		}

		public override bool OnFlushDirty(object entity, object id, object[] currentState, object[] previousState,
													 string[] propertyNames, IType[] types)
		{
			_entityStateRollbackInterceptor.OnFlushDirty(entity, id, currentState, previousState, propertyNames, types);

			var ent = entity as IEntity;
			var entityToCheck = entity as IRestrictionChecker;
			if (entityToCheck != null)
			{
				entityToCheck.CheckRestrictions();
			}

			var root = ent as IAggregateRoot;
			if (root != null)
			{
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
			_entityStateRollbackInterceptor.OnSave(entity, id, state, propertyNames, types);

			var ent = entity as IEntity;
			var entityToCheck = entity as IRestrictionChecker;
			if (entityToCheck != null)
			{
				entityToCheck.CheckRestrictions();
			}

			markRoot(ent);

			if (setCreatedProperties(entity, propertyNames, state))
			{
				ret = true;
			}

			var root = entity as IAggregateRoot;
			if (root != null) 
				modifiedRoots.Add(new RootChangeInfo(root, DomainUpdateType.Insert));
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
			var aggEnt = ent as IAggregateEntity;
			if (aggEnt == null) 
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

			var deleteInfo = root as IDeleteTag;
			if (deleteInfo != null && deleteInfo.IsDeleted)
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
 