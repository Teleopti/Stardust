using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Type;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	/*
	RK - don't think this is needed any longer.
	Was first created to fix persist problems in scheduler. Those problems are now handled in some other way (all persist tests are green)
	If removing, one test fails (NHibernateUnitOfWorkRealTest.SetIdToNullToNewlyAddedRootsIfTranRollback) and it fails "correctly".
	However, it's probably wrong if code relies on correct rollback anyhow so it would be nice to remove this functionality all together.

	If you who reads this currently have a Dee Snider attitute - please remove this. I don't have the guts ATM.
	*/
	public class VersionStateRollbackInterceptor : EmptyInterceptor
    {
        private readonly IDictionary<object, EntityState> _entityStates = new Dictionary<object, EntityState>();

        private class EntityState
        {
            public Guid? Id { get; set; }
            public int Version { get; set; }
        }

        public void Clear()
        {
            _entityStates.Clear();
        }

        public override void AfterTransactionCompletion(ITransaction tx)
        {
            if (tx == null)
            {
                return;
            }
            if (!tx.WasRolledBack)
            {
                return;
            }
            RollbackEntityState();
        }

        public override bool OnFlushDirty(object entity, object id, object[] currentState, object[] previousState, string[] propertyNames, IType[] types)
        {
            SetEntityState(entity, false);
            return true;
        }

        public override bool OnSave(object entity, object id, object[] state, string[] propertyNames, IType[] types)
        {
            SetEntityState(entity, true);
            return true;
        }

        private void RollbackEntityState()
        {
            _entityStates.ForEach(e =>
            {
                var entity = e.Key as IEntity;
                if (entity != null)
                    entity.SetId(e.Value.Id);
                var versioned = e.Key as IVersioned;
                if (versioned != null)
                    versioned.SetVersion(e.Value.Version);
            });
        }

        private void SetEntityState(object entity, bool isNew)
        {
            if (_entityStates.ContainsKey(entity))
            {
                return;
            }

            var state = new EntityState();
            _entityStates[entity] = state;

            var identifiableEntity = entity as IEntity;
            if (identifiableEntity != null)
            {
                if (isNew)
                {
                    state.Id = null;
                }
                else
                {
                    state.Id = identifiableEntity.Id;
                }
            }
            var versionedEntity = entity as IVersioned;
            if (versionedEntity != null && versionedEntity.Version.HasValue)
            {
                state.Version = versionedEntity.Version.Value;
            }
        }
    }
}