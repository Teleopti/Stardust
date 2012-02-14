using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Configuration.Columns
{
    public class SFGridColumnGridHelperBase
    {
        private readonly IDictionary<GridControl, object> _sfGridColumnGridHelperCollection;

        public SFGridColumnGridHelperBase()
        {
            _sfGridColumnGridHelperCollection = new Dictionary<GridControl, object>();
        }

        public SFGridColumnGridHelper<T> GetSFGridColumnGridHelper<T>(GridControl gridControl)
        {
            SFGridColumnGridHelper<T> instance = null;
            if (_sfGridColumnGridHelperCollection.ContainsKey(gridControl))
                instance = (SFGridColumnGridHelper<T>)_sfGridColumnGridHelperCollection[gridControl];
            return instance;
        }

        public void Create<T>(GridControl gridControl,
                              ReadOnlyCollection<SFGridColumnBase<T>> gridColumns,
                              IList<T> sourceList,
                              EventHandler<SFGridColumnGridHelperEventArgs<T>> newSourceEntityWanted)
        {
        	if (_sfGridColumnGridHelperCollection.ContainsKey(gridControl)) return;
        	var instance = new SFGridColumnGridHelper<T>(gridControl, gridColumns, sourceList);
        	instance.NewSourceEntityWanted += newSourceEntityWanted;
        	_sfGridColumnGridHelperCollection.Add(gridControl, instance);
        }

        public void Create<T>(GridControl gridControl,
                              ReadOnlyCollection<SFGridColumnBase<T>> gridColumns,
                              List<T> sourceList,
                              EventHandler<SFGridColumnGridHelperEventArgs<T>> newSourceEntityWanted,
                              bool allowExtendedCopyPaste)
        {
            Create(gridControl, gridColumns, sourceList, newSourceEntityWanted);
            GetSFGridColumnGridHelper<T>(gridControl).AllowExtendedCopyPaste = allowExtendedCopyPaste;
        }
    }
}
