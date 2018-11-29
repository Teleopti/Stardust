using System;
using System.Runtime.Serialization;
using System.Security;
using NHibernate;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Infrastructure.Foundation
{
    /// <summary>
    /// Optimistic lock exception.
    /// Two users are modifying the same aggregate. The last persist
    /// will get this exception. Holds info of what root and what entity id.
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-04-08
    /// </remarks>
    [Serializable]
    public class OptimisticLockException : DataSourceException
    {
        private readonly Guid _rootId;
        private readonly string _entityName = string.Empty;

        public OptimisticLockException(string message, Exception innerException) : base(message, innerException)
        {
            StaleObjectStateException optLockInfo = innerException as StaleObjectStateException;
            if(optLockInfo!=null)
            {
                _rootId = (Guid)optLockInfo.Identifier;
                _entityName = optLockInfo.EntityName;
            }
        }

        public OptimisticLockException(string message, Guid rootId, string entityName) : base(message)
        {
            _rootId = rootId;
            _entityName = entityName;
        }

        protected OptimisticLockException(SerializationInfo info,
                                      StreamingContext context) : base(info, context)
        {
            _rootId = (Guid)info.GetValue("RootId", typeof (Guid));
            _entityName = info.GetString("EntityName");
        }

        public OptimisticLockException(string message) : base(message)
        {
        }

        public OptimisticLockException()
        {
        }

        public Guid RootId => _rootId;

	    public string EntityName => _entityName;

	    public bool HasEntityInformation => !string.IsNullOrEmpty(_entityName);


	    [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            InParameter.NotNull(nameof(info), info);

            info.AddValue("RootId", _rootId);
            info.AddValue("EntityName", _entityName);
            base.GetObjectData(info, context);
        }
    }
}
