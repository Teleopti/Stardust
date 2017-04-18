using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Editor
{
    public interface ILayerEditor
    {
        /// <summary>
        /// Gets or sets the selectable payloads.
        /// </summary>
        /// <value>The selectable payloads.</value>
        /// <remarks>
        /// Absences/Activities etc..
        /// </remarks>
        IList<IPayload> SelectablePayloads { get; }

 
    }
}
