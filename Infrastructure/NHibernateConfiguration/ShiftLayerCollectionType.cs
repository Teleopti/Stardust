using System.Collections;
using System.Collections.Generic;
using NHibernate.Collection;
using NHibernate.Collection.Generic;
using NHibernate.Engine;
using NHibernate.Envers.Entities.Mapper;
using NHibernate.Envers.Entities.Mapper.Relation;
using NHibernate.Persister.Collection;
using NHibernate.Type;
using NHibernate.UserTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	/// <summary>
	/// Not needed when/if https://github.com/nhibernate/nhibernate-core/pull/545 is released on our nhib version
	/// </summary>
	public class ShiftLayerCollectionType : IUserCollectionType, ICustomCollectionMapperFactory
	{
		private const string shiftLayerCollectionRole = "Teleopti.Ccc.Domain.Scheduling.Assignment.PersonAssignment.ShiftLayers";

		public IPersistentCollection Instantiate(ISessionImplementor session, ICollectionPersister persister)
		{
			return new PersistentGenericList<IShiftLayer>(session);
		}

		public IPersistentCollection Wrap(ISessionImplementor session, object collection)
		{
			return new PersistentGenericList<IShiftLayer>(session, (IList<IShiftLayer>)collection);
		}

		public IEnumerable GetElements(object collection)
		{
			return (IEnumerable)collection;
		}

		public bool Contains(object collection, object entity)
		{
			throw new System.NotImplementedException();
		}

		public object IndexOf(object collection, object entity)
		{
			throw new System.NotImplementedException();
		}

		public object ReplaceElements(object original, object target, ICollectionPersister persister, object owner, IDictionary copyCache, ISessionImplementor session)
		{
			var elemType = GetElementType(session.Factory);
			var elementsEqual = AreCollectionElementsEqual((IEnumerable)original, ((IEnumerable)target).GetEnumerator(), elemType, session);

			// TODO: does not work for EntityMode.DOM4J yet!
			Clear(target);

			// copy elements into newly empty target collection
			IEnumerable iter = (IEnumerable)original;
			foreach (object obj in iter)
			{
				Add(target, elemType.Replace(obj, null, session, owner, copyCache));
			}

			// if the original is a PersistentCollection, and that original
			// was not flagged as dirty, then reset the target's dirty flag
			// here after the copy operation.
			// One thing to be careful of here is a "bare" original collection
			// in which case we should never ever ever reset the dirty flag
			// on the target because we simply do not know...
			IPersistentCollection originalPc = original as IPersistentCollection;
			IPersistentCollection resultPc = target as IPersistentCollection;
			if (resultPc != null)
			{
				if (elementsEqual || (originalPc != null && !originalPc.IsDirty))
					resultPc.ClearDirty();
			}

			return target;
		}

		protected void Clear(object collection)
		{
			((IList<IShiftLayer>)collection).Clear();
		}

		protected void Add(object collection, object element)
		{
			((IList<IShiftLayer>)collection).Add((IShiftLayer)element);
		}

		public IType GetElementType(ISessionFactoryImplementor factory)
		{
			return factory.GetCollectionPersister(shiftLayerCollectionRole).ElementType;
		}

		private static bool AreCollectionElementsEqual(IEnumerable originalEnumerator, IEnumerator targetEnumerator, IType elementType, ISessionImplementor session)
		{
			var entityMode = session.EntityMode;
			foreach (var originalElement in originalEnumerator)
			{
				if (targetEnumerator.MoveNext())
				{
					var targetElement = targetEnumerator.Current;
					if (!elementType.IsEqual(targetElement, originalElement, entityMode))
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}

			return !targetEnumerator.MoveNext();
		}

		public object Instantiate(int anticipatedSize)
		{
			return anticipatedSize <= 0 ? new List<IShiftLayer>() : new List<IShiftLayer>(anticipatedSize + 1);
		}

		public IPropertyMapper Create(IEnversProxyFactory enversProxyFactory, CommonCollectionMapperData commonCollectionMapperData, MiddleComponentData elementComponentData, MiddleComponentData indexComponentData, bool embeddableElementType)
		{
			return new ListCollectionMapper<IShiftLayer>(enversProxyFactory, commonCollectionMapperData, typeof(IList<IShiftLayer>), elementComponentData, indexComponentData, embeddableElementType);
		}
	}
}