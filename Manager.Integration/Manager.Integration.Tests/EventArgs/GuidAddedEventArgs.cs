using System;

namespace Manager.Integration.Test.EventArgs
{
    public class GuidAddedEventArgs
    {
        public Guid Guid { get; private set; }

        public GuidAddedEventArgs(Guid guid)
        {
            Guid = guid;
        }
    }
}