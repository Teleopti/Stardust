using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Teleopti.Analytics.Etl.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure
{
    public interface IThreadPool
    {
        event RowsUpdatedEventHandler RowsUpdatedEvent;
        void Load(IEnumerable<ITaskParameters> taskParameters, DoWork doWork);
        void Start();
        Exception ThreadError { get; }
    }
}