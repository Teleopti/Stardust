using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Editor
{
    public interface IWpfShiftEditor : IShiftEditorEvents, IShiftEditor,ILayerEditor
    {
        IList<IShiftCategory> SelectableShiftCategories { get;}

        void LoadFromStateHolder(CommonStateHolder commonStateHolder);

        void EnableMoveAllLayers(bool move);
        
        void Unload();

        void SetTimeZone(TimeZoneInfo timeZoneInfo);
    }
}
