using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Editor
{
    public interface IWpfShiftEditor : IShiftEditorEvents, IShiftEditor,ILayerEditor
    {
       
        /// <summary>
        /// Gets the selectable shift categories.
        /// </summary>
        /// <value>The selectable shift categories.</value>
        IList<IShiftCategory> SelectableShiftCategories { get;}

        /// <summary>
        /// Loads all the shiftcategories,absences and activities from stateholder.
        /// </summary>
        /// <param name="commonStateHolder">The common state holder.</param>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-10-07
        /// </remarks>
        void LoadFromStateHolder(ICommonStateHolder commonStateHolder);

        void EnableMoveAllLayers(bool move);
        
        void Unload();

        void SetTimeZone(TimeZoneInfo timeZoneInfo);
    }
}
