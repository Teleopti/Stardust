using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Type;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	/// <summary>
	/// Interceptor adding support for "changed roots"
	/// </summary>
	public class AggregateRootInterceptor : EmptyInterceptor
	{
		private const string createdByPropertyName = "CREATEDBY";
		private const string createdOnPropertyName = "CREATEDON";
		private const string updatedByPropertyName = "UPDATEDBY";
		private const string updatedOnPropertyName = "UPDATEDON";
		private readonly ICollection<IRootChangeInfo> modifiedRoots = new HashSet<IRootChangeInfo>();
		private readonly ICollection<IAggregateRoot> rootsWithModifiedChildren = new HashSet<IAggregateRoot>();

		private readonly VersionStateRollbackInterceptor _entityStateRollbackInterceptor = new VersionStateRollbackInterceptor();

		public AggregateRootInterceptor()
		{
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
		
		public ICollection<IRootChangeInfo> ModifiedRoots
		{
			get { return modifiedRoots; }
		}

		public override int[] FindDirty(object entity, object id, object[] currentState, object[] previousState,
												  string[] propertyNames, IType[] types)
		{
			int[] retVal = null;
			if (Iteration == InterceptorIteration.UpdateRoots)
			{
				IAggregateRoot root = entity as IAggregateRoot;
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

		public override void OnDelete(object entity, object id, object[] state, string[] propertyNames, IType[] types)
		{
			var root = entity as IAggregateRoot;
			if (root != null)
			{
				modifiedRoots.Add(new RootChangeInfo(root, DomainUpdateType.Delete));
			}
			else
			{
				var nonRoot = (IAggregateEntity)entity;
				rootsWithModifiedChildren.Add(nonRoot.Root());
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
			_entityStateRollbackInterceptor.OnSave(entity, id, state, propertyNames, types);

			var ent = entity as IEntity;
			var entityToCheck = entity as IRestrictionChecker;
			if (entityToCheck != null)
			{
				entityToCheck.CheckRestrictions();
			}

			markRoot(ent);

			ICreateInfo createInfo = entity as ICreateInfo;
			if (createInfo != null)
			{
				setCreatedProperties(createInfo, propertyNames, state);
				return true;
			}
			return false;
		}

		private static IDictionary<string, int> propertyIndexesForInsert(IEnumerable<string> properties)
		{
			List<string> listOfProperties = properties.ToList();

			IDictionary<string, int> result = new Dictionary<string, int>();

			int index = listOfProperties.FindIndex(p => p.ToUpperInvariant() == createdByPropertyName);
			if (index >= 0) result.Add(createdByPropertyName, index);

			index = listOfProperties.FindIndex(p => p.ToUpperInvariant() == createdOnPropertyName);
			if (index >= 0) result.Add(createdOnPropertyName, index);

			return result;
		}

		private static IDictionary<string, int> propertyIndexesForUpdate(IEnumerable<string> properties)
		{
			List<string> listOfProperties = properties.ToList();

			IDictionary<string, int> result = new Dictionary<string, int>();

			int index = listOfProperties.FindIndex(p => p.ToUpperInvariant() == updatedByPropertyName);
			if (index >= 0) result.Add(updatedByPropertyName, index);

			index = listOfProperties.FindIndex(p => p.ToUpperInvariant() == updatedOnPropertyName);
			if (index >= 0) result.Add(updatedOnPropertyName, index);

			return result;
		}

		private void markRoot(IEntity ent)
		{
			IAggregateEntity aggEnt = ent as IAggregateEntity;
			if (aggEnt == null) return;
			IAggregateRoot root = aggEnt.Root();
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

		private void setCreatedProperties(ICreateInfo createInfo, string[] propertyNames, object[] state)
		{
			var nu = DateTime.UtcNow;
			var props = propertyIndexesForInsert(propertyNames);
			state[props[createdByPropertyName]] = ((IUnsafePerson)TeleoptiPrincipal.Current).Person;
			state[props[createdOnPropertyName]] = nu;

			setUpdatedProperties(nu, createInfo, propertyNames, state);

			var root = createInfo as IAggregateRoot;
			if (root != null) modifiedRoots.Add(new RootChangeInfo(root, DomainUpdateType.Insert));
		}

		private void setUpdatedInfo(IAggregateRoot root, object[] currentState, string[] propertyNames)
		{
			setUpdatedProperties(DateTime.UtcNow, root, propertyNames, currentState);

			var deleteInfo = root as IDeleteTag;
			if (deleteInfo != null && deleteInfo.IsDeleted)
			{
				modifiedRoots.Add(new RootChangeInfo(root, DomainUpdateType.Delete));
			}
			else
			{
				modifiedRoots.Add(new RootChangeInfo(root, DomainUpdateType.Update));
			}
		}

		private static void setUpdatedProperties(DateTime nu, object root, string[] propertyNames, object[] currentState)
		{
			if (root is IChangeInfo)
			{
				var props = propertyIndexesForUpdate(propertyNames);
				currentState[props[updatedByPropertyName]] = ((IUnsafePerson)TeleoptiPrincipal.Current).Person;
				currentState[props[updatedOnPropertyName]] = nu;
			}
		}
	}
}