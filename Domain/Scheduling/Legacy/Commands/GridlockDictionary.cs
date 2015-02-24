using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
    [Serializable]
    public class GridlockDictionary : Dictionary<string, Gridlock>
    {
        public GridlockDictionary()
        {}

        protected GridlockDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}

        public bool HasLockType(LockType lockType)
        {
            foreach (KeyValuePair<string, Gridlock> pair in this)
            {
                if (pair.Value.LockType == lockType)
                    return true;
            }
            return false;
        }
    }
}
