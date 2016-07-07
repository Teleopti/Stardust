﻿using System.Collections.Generic;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Presentation
{
    public interface IShiftCategoryLimitationView
    {
        GridControl ShiftCategoryLimitationGrid { get; }
        void SetupGrid();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        IList<IShiftCategory> ShiftCategories{ get; set;}
        string Name { get; set; }
        void SetSchedulePeriodList(IList<ISchedulePeriod> schedulePeriods);
        void InitializePresenter();
        void SetDirtySchedulePeriod(ISchedulePeriod schedulePeriod);
    }
}
