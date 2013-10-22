using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Configuration
{
    public class AlarmControlPresenter
    {
        private readonly IAlarmControlView _view;
        private readonly IList<IAlarmType> _alarmTypes;
        private IAlarmType _alarm;
        private readonly LocalizedUpdateInfo _localizer = new LocalizedUpdateInfo();
        
        public AlarmControlPresenter(IList<IAlarmType> alarmTypes, IAlarmControlView view)
        {
            _view = view;
            _alarmTypes = alarmTypes;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        public void QueryRowCount(object sender, GridRowColCountEventArgs e)
        {
            e.Count = _alarmTypes.Count;
            e.Handled = true;
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        public void QueryColCount(object sender, GridRowColCountEventArgs e)
        {
            e.Count =Enum.GetNames( typeof (ColumnHeader) ).Length -1;
            e.Handled = true;
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        public void QueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
        {
            if (e.RowIndex < (int) ColumnHeader.Empty || e.ColIndex < (int) ColumnHeader.Empty)
                return; // Bad index
            if (e.RowIndex == (int) ColumnHeader.Empty && e.ColIndex == 0)
                return;
            if (e.RowIndex == 0)
                QueryHeader(e);
            else
            {
                switch ((ColumnHeader)e.ColIndex)
                {
                    case ColumnHeader.Name:
                        QueryName(e);
                        break;
                    case ColumnHeader.Time:
                        QueryTimespan(e);
                        break;
                    case ColumnHeader.Color:
                        QueryColorPicker(e);
                        break;
                    case ColumnHeader.StaffingEffect:
                        QueryStaffingEffect(e);
                        break;
                    case ColumnHeader.CreatedBy:
                        QueryCreatedBy(e);
                        break;
                    case ColumnHeader.CreatedOn:
                        QueryCreatedOn(e);
                        break;
                    case ColumnHeader.UpdatedOn:
                        QueryUpdatedOn(e);
                        break;
                    case ColumnHeader.UpdatedBy:
                        QueryUpdatedBy(e);
                        break;
                }

            }
            e.Handled = true;
        }

        private void QueryStaffingEffect(GridQueryCellInfoEventArgs e)
        {
            e.Style.CellType = "NumericCell";
            e.Style.CellValue  = _alarmTypes[e.RowIndex - 1].StaffingEffect ;
        }

        private void QueryName(GridQueryCellInfoEventArgs e)
        {
            e.Style.Text = _alarmTypes[e.RowIndex - 1].Description.Name;
        }

        private void QueryColorPicker(GridQueryCellInfoEventArgs e)
        {
            e.Style.CellType = "ColorPickerCell";
            e.Style.CellValueType = typeof(Color);
            e.Style.CellValue = _alarmTypes[e.RowIndex - 1].DisplayColor;
        }

        private void QueryTimespan(GridQueryCellInfoEventArgs e)
        {
            e.Style.CellValue = _alarmTypes[e.RowIndex - 1].ThresholdTime.TotalSeconds ;
            e.Style.CellType = "NumericCell";
        }

        private void QueryCreatedBy(GridQueryCellInfoEventArgs e)
        {
            e.Style.CellType = "Static";
            if (_alarmTypes[e.RowIndex - 1].CreatedBy != null)
                e.Style.CellValue = _alarmTypes[e.RowIndex - 1].CreatedBy.Name;
        }

        private void QueryCreatedOn(GridQueryCellInfoEventArgs e)
        {
            e.Style.CellType = "Static";
            if (_alarmTypes[e.RowIndex - 1].CreatedOn.HasValue)
                e.Style.CellValue = _localizer.CreatedTimeInUserPerspective(_alarmTypes[e.RowIndex - 1]);
        }

        private void QueryUpdatedBy(GridQueryCellInfoEventArgs e)
        {
            e.Style.CellType = "Static";
            if (_alarmTypes[e.RowIndex - 1].UpdatedBy != null)
                e.Style.CellValue = _alarmTypes[e.RowIndex - 1].UpdatedBy.Name;
        }

        private void QueryUpdatedOn(GridQueryCellInfoEventArgs e)
        {
            e.Style.CellType = "Static";
            if (_alarmTypes[e.RowIndex - 1].UpdatedOn.HasValue)
                e.Style.CellValue = _localizer.UpdatedTimeInUserPerspective(_alarmTypes[e.RowIndex - 1]);
        }

        private static void QueryHeader(GridQueryCellInfoEventArgs e)
        {
            switch ((ColumnHeader)e.ColIndex)
            {
                case ColumnHeader.Name:

                    e.Style.Text = Resources.Name;
                    break;

                case ColumnHeader.Time:

                    e.Style.Text = Resources.Time ;
                    break;

                case ColumnHeader.Color:

                    e.Style.Text = Resources.Color;
                    break;
                case ColumnHeader.StaffingEffect:

                    e.Style.Text = Resources.StaffingEffect;
                    break;

                case ColumnHeader.CreatedBy:

                    e.Style.Text = Resources.CreatedBy;
                    break;
                case ColumnHeader.CreatedOn:

                    e.Style.Text = Resources.CreatedOn;
                    break;
                case ColumnHeader.UpdatedBy:

                    e.Style.Text = Resources.UpdatedBy;
                    break;
                case ColumnHeader.UpdatedOn:

                    e.Style.Text = Resources.UpdatedOn;
                    break;
            }
       }

        /// <summary>
        /// only public for testing
        ///add header here
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public enum ColumnHeader
        {
            Empty,

            Name,

            Time,

            StaffingEffect,

            Color,

            CreatedBy,

            CreatedOn,

            UpdatedBy,

            UpdatedOn

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        public void CellClick(object sender, GridCellCancelEventArgs  e)
        {
           _view.ShowThisItem(e.RowIndex -1);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        public void SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
        {
            if (e.RowIndex == 0 || e.ColIndex == 0) return;

            //find alarmtype
            if (_alarmTypes.Count >= 1)
                _alarm = _alarmTypes[e.RowIndex - 1];
            else
                return;

            switch ((ColumnHeader) e.ColIndex)
            {
                case ColumnHeader.Name:
                    NewName(e);
                    break;
                case ColumnHeader.Time:
                    NewTimeSpan(e);
                    break;
                case ColumnHeader.Color:
                    NewColor(e);
                    break;
                case ColumnHeader.StaffingEffect:
                    NewStaffingEffect(e);
                    break;
            }
            e.Handled = true;
        }

        private void NewStaffingEffect(GridSaveCellInfoEventArgs e)
        {
             _alarm.StaffingEffect  = (double)e.Style.CellValue;
        }

        private void NewColor(GridSaveCellInfoEventArgs e)
        {
            Color t = (Color)e.Style.CellValue;
            if(t == Color.Empty ) return;
        	_alarm.DisplayColor = t;
        }

        private void NewTimeSpan(GridSaveCellInfoEventArgs e)
        {
            double d = (double) e.Style.CellValue;
            if(d< 0) return;

            _alarm.ThresholdTime = TimeSpan.FromSeconds(d);
        }

        private void NewName(GridSaveCellInfoEventArgs e)
        {
            string s = (string) e.Style.CellValue;

            if(string.IsNullOrEmpty(s))
            {
                return;
            }

        	IAlarmType alarmType = _alarmTypes.SingleOrDefault(a => a.Description.Name == s);
            if (alarmType != null)
            {
                _view.Warning(Resources.NameAlreadyExists );
                return;
            }

            _alarm.Description = new Description(s);
        }
    }
}
