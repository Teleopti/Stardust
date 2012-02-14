using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Type;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
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
            return;
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