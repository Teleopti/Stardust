using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.AgentPortal.Common.Configuration.Columns
{
    public class SFGridColumnGridHelperBase
    {
        private readonly IDictionary<GridControl, object> sfGridColumnGridHelperCollection;

        public SFGridColumnGridHelperBase()
        {
            this.sfGridColumnGridHelperCollection = new Dictionary<GridControl, object>();
        }

        public SFGridColumnGridHelper<T> GetSFGridColumnGridHelper<T>(GridControl gridControl)
        {
            SFGridColumnGridHelper<T> instance = null;
            if (this.sfGridColumnGridHelperCollection.ContainsKey(gridControl))
                instance = (SFGridColumnGridHelper<T>)this.sfGridColumnGridHelperCollection[gridControl];
            return instance;
        }

        public void Create<T>(GridControl gridControl,
                              ReadOnlyCollection<SFGridColumnBase<T>> gridColumns,
                              List<T> sourceList,
                              EventHandler<SFGridColumnGridHelperEventArgs<T>> newSourceEntityWanted)
        {
            if (!this.sfGridColumnGridHelperCollection.ContainsKey(gridControl))
            {
                SFGridColumnGridHelper<T> instance = new SFGridColumnGridHelper<T>(gridControl, gridColumns, sourceList);
                instance.NewSourceEntityWanted += newSourceEntityWanted;
                this.sfGridColumnGridHelperCollection.Add(gridControl, instance);
            }
        }
    }
}
