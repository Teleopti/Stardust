using System;
using System.Threading;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public sealed class TurnoffPermissionScope : IDisposable
    {
        private readonly IPermissionCheck _entity;

        private TurnoffPermissionScope(IPermissionCheck entity)
        {
            _entity = entity;
            _entity.UsePermissions(false);
        }

        public void Dispose()
        {
            _entity.UsePermissions(true);
            Monitor.Exit(_entity.SynchronizationObject);
        }

        /// <summary>
        /// if IPermissionCheck.Synchronization
        /// </summary>
        /// <param name="objectToTurnoffPermissionsOn"></param>
        /// <returns></returns>
        public static IDisposable For(IPermissionCheck objectToTurnoffPermissionsOn)
        {
            Monitor.Enter(objectToTurnoffPermissionsOn.SynchronizationObject);
            return new TurnoffPermissionScope(objectToTurnoffPermissionsOn);
        }
    }
}
