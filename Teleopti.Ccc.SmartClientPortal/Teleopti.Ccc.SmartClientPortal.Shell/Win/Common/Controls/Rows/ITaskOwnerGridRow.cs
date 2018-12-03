using System;
using System.Collections.Generic;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Rows
{
    public class ITaskOwnerGridRow : GridRow
    {
        private readonly PropertyReflector _propertyReflector = new PropertyReflector();
        private readonly RowManager<ITaskOwnerGridRow, ITaskOwner> _rowManager;
        private readonly int _maxLength;
        private readonly IList<DateOnly> _dateTimes;
        private readonly TemplateTarget _templateTarget = TemplateTarget.Workload;
        private readonly string _parentRowHeaderText;

        public event EventHandler<FromCellEventArgs<ITaskOwner>> QueryCellValue;
        public event EventHandler<FromCellEventArgs<ITaskOwner>> SaveCellValue;
        public event EventHandler<TemplateEventArgs> TemplateSelected;

        public ITaskOwnerGridRow(RowManager<ITaskOwnerGridRow, ITaskOwner> rowManager, string cellType, 
                                 string displayMember, string rowHeaderText, string parentRowHeaderText, IList<DateOnly> dateTimes) 
            : base(cellType, displayMember, rowHeaderText)
        {
            _rowManager = rowManager;
            _dateTimes = dateTimes;
            _parentRowHeaderText = parentRowHeaderText;
        }

        public ITaskOwnerGridRow(RowManager<ITaskOwnerGridRow, ITaskOwner> rowManager, string cellType,
                                 string displayMember, string rowHeaderText, string parentRowHeaderText, IList<DateOnly> dateTimes, TemplateTarget templateTarget)
            : this(rowManager, cellType, displayMember, rowHeaderText, parentRowHeaderText, dateTimes)
        {
            _templateTarget = templateTarget;
        }

        public ITaskOwnerGridRow(RowManager<ITaskOwnerGridRow, ITaskOwner> rowManager, string cellType,
                                 string displayMember, string rowHeaderText, string parentRowHeaderText, IList<DateOnly> dateTimes, int maxLength)
            : this(rowManager, cellType, displayMember, rowHeaderText, parentRowHeaderText, dateTimes)
        {
            _maxLength = maxLength;
        }

        public override void QueryCellInfo(CellInfo cellInfo)
        {
            if (cellInfo.ColIndex == 0)
            {
                VerticalRowHeaderSettings(cellInfo, _parentRowHeaderText);
            }
            else if (cellInfo.ColIndex == 1)
            {
                cellInfo.Style.CellValue = RowHeaderText;
            }
            else
            {
                if (cellInfo.Style == null) return;
                int rowHeaders = cellInfo.RowHeaderCount;
                if (_rowManager.DataSource.Count == 0 || _dateTimes.Count==0) return;

                if (Math.Max(rowHeaders, cellInfo.ColIndex) - rowHeaders >= _rowManager.DataSource.Count) return;
                ITaskOwner taskOwner = GetObjectAtPosition(_rowManager, cellInfo.ColIndex, rowHeaders);
                cellInfo.Style.CellType = CellType;
                cellInfo.Style.CellValue = GetValue(taskOwner);
                cellInfo.Style.MaxLength = _maxLength;

            	var handler = QueryCellValue;
                if (handler != null)
                {
                    handler.Invoke(this, new FromCellEventArgs<ITaskOwner> { Item = taskOwner, Style = cellInfo.Style });
                }
            }
            GuiHelper.SetStyle(cellInfo.Style);
        }

        public override void SaveCellInfo(CellInfo cellInfo)
        {
            int rowHeaders = _rowManager.Grid.Cols.HeaderCount+1;
            if (_rowManager.DataSource.Count == 0 || _dateTimes.Count == 0) return;

            ITaskOwner taskOwner = GetObjectAtPosition(_rowManager, cellInfo.ColIndex, rowHeaders);

        	var handler = SaveCellValue;
            if (handler != null)
            {
                handler.Invoke(this, new FromCellEventArgs<ITaskOwner>
                                               {
                                                   Item = taskOwner,
                                                   Style = cellInfo.Style,
                                                   Value = cellInfo.Style.CellValue
                                               });
            }
            else
            {
                var gridModel = cellInfo.Style.GetGridModel();
                if (gridModel[cellInfo.RowIndex, cellInfo.ColIndex].CellModel.ToString().Contains("Header"))
                {
                    if (!gridModel[cellInfo.RowIndex, cellInfo.ColIndex + 1].CellModel.ToString().Contains("ReadOnly"))
                        SetValue(taskOwner, cellInfo.Style.CellValue);
                }
                else
                    SetValue(taskOwner, cellInfo.Style.CellValue);
            }
        }

        public override void OnSelectionChanged(GridSelectionChangedEventArgs e, int rowHeaders)
        {
        	var handler = TemplateSelected;
            if (handler == null) return;
            if (_rowManager.DataSource.Count == 0 || _dateTimes.Count == 0) return;

            ITemplateDay taskOwner = GetObjectAtPosition(_rowManager,e.Range.Left, rowHeaders) as ITemplateDay;
            if (taskOwner==null) return;
            handler.Invoke(this, new TemplateEventArgs { 
                                                                      TemplateName = taskOwner.TemplateReference.TemplateName, 
                                                                      TemplateTarget = _templateTarget });
        }

        private void SetValue(ITaskOwner taskOwner, object value)
        {
            Type t = _propertyReflector.GetType(taskOwner.GetType(), DisplayMember).UnderlyingSystemType;

            IWorkloadDay workloadDay = taskOwner as IWorkloadDay;
            if (taskOwner.OpenForWork.IsOpen ||
                (workloadDay!=null && workloadDay.Workload.Skill.SkillType.ForecastSource!=ForecastSource.InboundTelephony && 
					 workloadDay.Workload.Skill.SkillType.ForecastSource!= ForecastSource.Retail &&
					 workloadDay.Workload.Skill.SkillType.ForecastSource != ForecastSource.Chat))
            {
                if (String.IsNullOrEmpty(value.ToString()))
                {
                    if (t.FullName == "System.Double" || t.FullName == "System.Int32")
                        value = 0;
                    else if (t.FullName == "System.TimeSpan")
                        value = TimeSpan.Zero;
                    else if (t.FullName == typeof(Percent).FullName)
                        value = new Percent(0);
                }
                _propertyReflector.SetValue(taskOwner, DisplayMember, value);
            }
        }

        private object GetValue(ITaskOwner taskOwner)
        {
            return _propertyReflector.GetValue(taskOwner, DisplayMember);
        }
    }
}
