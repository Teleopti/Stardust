using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace Teleopti.Ccc.Win.SmartParts
{
    /// <summary>
    /// Represent a Grid like Smart Client Workspace
    /// </summary>
    /// <remarks>
    /// Created by: Sumedah
    /// Created date: 2008-08-04
    /// </remarks>
    public partial class GridWorkspace : UserControl
    {
        /// <summary>
        /// Holds reference to internale table layout panel
        /// </summary>
        private TableLayoutPanel _tableLayoutPanel;

        /// <summary>
        /// Store the current Size of the Grid
        /// </summary>
        private GridSizeType _gridSize=GridSizeType.TwoByTwo;
   
        /// <summary>
        /// Gets or sets the size of the grid.
        /// </summary>
        /// <value>The size of the grid.</value>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-04
        /// </remarks>
        public GridSizeType GridSize
        {
            get { return _gridSize; }
            set 
            {
                if (_gridSize!=value)
                {
                    _gridSize = value;
                    SetWorkspaceSize(_gridSize);
                }
            }
        }

        /// <summary>
        /// Gets the smart parts.
        /// </summary>
        /// <value>The smart parts.</value>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-05
        /// </remarks>
        public ReadOnlyCollection<SmartPartBase> SmartParts
        {
            get { return GetSmartPartList(); }
        }
        
        /// <summary>
        /// Occurs when [workspace grid size changed].
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-09-29
        /// </remarks>
        public event EventHandler<EventArgs> WorkspaceGridSizeChanged;

        /// <summary>
        /// Shows the specified smart part in specified grid cell.
        /// </summary>
        /// <param name="smartPart">The smart part.</param>
        /// <param name="smartPartInfo">The smartpart info.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-04
        /// </remarks>
        public void Show(SmartPartBase smartPart, SmartPartInformation smartPartInfo)
        {
            // Grid Filled
            if (SmartParts.Count == (int)_gridSize) return;

            smartPart.SmartPartHeaderTitle = smartPartInfo.SmartPartHeaderTitle;
            if (smartPartInfo.GridColumn != null && smartPartInfo.GridRow != null)
            {
                smartPart.Dock = DockStyle.Fill;
                _tableLayoutPanel.Controls.Add(smartPart, (int) smartPartInfo.GridColumn,
                                               (int) smartPartInfo.GridRow);
            }
            else
                _tableLayoutPanel.Controls.Add(smartPart);
        }
        
        /// <summary>
        /// Determines whether [is smart part exists] [the specified smart part id].
        /// </summary>
        /// <param name="smartPartId">The smart part id.</param>
        /// <returns>
        /// 	<c>true</c> if [is smart part exists] [the specified smart part id]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-05
        /// </remarks>
        public bool IsSmartPartExists(string smartPartId)
        {
            foreach (SmartPartBase smartPart in SmartParts)
            {
                if(smartPart.SmartPartId==smartPartId)
                    return true;
            }

            return false;
        }
        
        /// <summary>
        /// Gets list loaded SmartPart by it's type 
        /// </summary>
        /// <param name="smartPartType">Type of the smart part.</param>
        /// <returns>List of smart part of current type</returns>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-09-24
        /// </remarks>
        public IList<SmartPartBase> GetSmartPartByType(string smartPartType)
        {
            IList<SmartPartBase> loadedSmartPartCollection=new List<SmartPartBase>();
            foreach (SmartPartBase smartPart in SmartParts)
            {
                if (smartPart.GetType().ToString() == smartPartType)
                {
                    loadedSmartPartCollection.Add(smartPart);
                }
            }
            return loadedSmartPartCollection;
        }

        /// <summary>
        /// Removes all smart part.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-09-26
        /// </remarks>
        public void RemoveAllSmartPart()
        {
            foreach (IDisposable control in _tableLayoutPanel.Controls)
            {
                control.Dispose();
            }
            _tableLayoutPanel.Controls.Clear();
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GridWorkspace"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-04
        /// </remarks>
        public GridWorkspace()
        {
            InitializeComponent();

            // Setting Zone workspace name to main work space
            zoneWorkspace.Name = "MainWorkspace";

            InitializeWorksapce();
        }

        /// <summary>
        /// Initializes the worksapce.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-04
        /// </remarks>
        private void InitializeWorksapce()
        {
            _tableLayoutPanel = new TableLayoutPanel();
            _tableLayoutPanel.Dock = DockStyle.Fill;
            SetWorkspaceSize(_gridSize);
            Controls.Clear();
            Controls.Add(_tableLayoutPanel);
        }

        /// <summary>
        /// Sets the size of the workspace.
        /// </summary>
        /// <param name="gridSize">Size of the grid.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-04
        /// </remarks>
        private void SetWorkspaceSize(GridSizeType gridSize)
        {
            _tableLayoutPanel.Visible = false;
            switch (gridSize)
            {
                case GridSizeType.None:
                case GridSizeType.OneByOne:
                    {
                        _tableLayoutPanel.RowStyles.Insert(0, new RowStyle(SizeType.Percent, 100F));
                        _tableLayoutPanel.ColumnStyles.Insert(0, new ColumnStyle(SizeType.Percent, 100F));
                        _tableLayoutPanel.RowCount = 1;
                        _tableLayoutPanel.ColumnCount = 1;
                        RemoveControls(1);
                        break;
                    }
                case GridSizeType.TwoByOne:
                    {
                        _tableLayoutPanel.RowStyles.Insert(0, new RowStyle(SizeType.Percent, 100F));
                        _tableLayoutPanel.RowStyles.Insert(1, new RowStyle(SizeType.Percent, 100F));
                        _tableLayoutPanel.ColumnStyles.Insert(0, new ColumnStyle(SizeType.Percent, 100F));
                        _tableLayoutPanel.RowCount = 2;
                        _tableLayoutPanel.ColumnCount = 1;
                        RemoveControls(2);
                        break;
                    }
                case GridSizeType.ThreeByOne:
                    {
                        _tableLayoutPanel.RowStyles.Insert(0, new RowStyle(SizeType.Percent, 100F));
                        _tableLayoutPanel.RowStyles.Insert(1, new RowStyle(SizeType.Percent, 100F));
                        _tableLayoutPanel.RowStyles.Insert(2, new RowStyle(SizeType.Percent, 100F));
                        _tableLayoutPanel.ColumnStyles.Insert(0, new ColumnStyle(SizeType.Percent, 100F));
                        _tableLayoutPanel.RowCount = 3;
                        _tableLayoutPanel.ColumnCount = 1;
                        RemoveControls(3);
                        break;
                    }
                case GridSizeType.TwoByTwo:
                    {
                        _tableLayoutPanel.RowStyles.Insert(0, new RowStyle(SizeType.Percent, 100F));
                        _tableLayoutPanel.ColumnStyles.Insert(0, new ColumnStyle(SizeType.Percent, 100F));
                        _tableLayoutPanel.RowStyles.Insert(1, new RowStyle(SizeType.Percent, 100F));
                        _tableLayoutPanel.ColumnStyles.Insert(1, new ColumnStyle(SizeType.Percent, 100F));
                        _tableLayoutPanel.RowCount = 2;
                        _tableLayoutPanel.ColumnCount = 2;
                        RemoveControls(4);
                        break;
                    }
                case GridSizeType.ThreeByThree:
                    {
                        _tableLayoutPanel.RowStyles.Insert(0, new RowStyle(SizeType.Percent, 100F));
                        _tableLayoutPanel.ColumnStyles.Insert(0, new ColumnStyle(SizeType.Percent, 100F));
                        _tableLayoutPanel.RowStyles.Insert(1, new RowStyle(SizeType.Percent, 100F));
                        _tableLayoutPanel.ColumnStyles.Insert(1, new ColumnStyle(SizeType.Percent, 100F));
                        _tableLayoutPanel.RowStyles.Insert(2, new RowStyle(SizeType.Percent, 100F));
                        _tableLayoutPanel.ColumnStyles.Insert(2, new ColumnStyle(SizeType.Percent, 100F));
                        _tableLayoutPanel.RowCount = 3;
                        _tableLayoutPanel.ColumnCount = 3;
                        break;
                    }
            }
            OnWorkspaceGridSizeChanged(EventArgs.Empty);
            _tableLayoutPanel.Visible = true;
        }

        /// <summary>
        /// Removes the controls.
        /// </summary>
        /// <param name="startIndex">The start index.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-04
        /// </remarks>
        private void RemoveControls(int startIndex)
        {
            if (_tableLayoutPanel.Controls.Count > startIndex)
            {
                int looplength = _tableLayoutPanel.Controls.Count - startIndex;
                for (int i = 1; i <= looplength; i++)
                {
                    _tableLayoutPanel.Controls.RemoveAt(startIndex);
                }
            }
        }

        /// <summary>
        /// Gets the smart part list.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-05
        /// </remarks>
        private ReadOnlyCollection<SmartPartBase> GetSmartPartList()
        {
            IList<SmartPartBase> smartPartCollection = new List<SmartPartBase>();
            foreach (SmartPartBase smartPartBase in _tableLayoutPanel.Controls)
            {
                smartPartCollection.Add(smartPartBase);
            }

            return new ReadOnlyCollection<SmartPartBase>(smartPartCollection);
        }

        /// <summary>
        /// Raises the WorkspaceGridSizeChanged event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-09-29
        /// </remarks>
        protected virtual void OnWorkspaceGridSizeChanged(EventArgs e)
        {
	        var onWorkspaceGridSizeChanged = WorkspaceGridSizeChanged;
	        if (onWorkspaceGridSizeChanged != null)
                onWorkspaceGridSizeChanged(this, e);
        }
    }
}
