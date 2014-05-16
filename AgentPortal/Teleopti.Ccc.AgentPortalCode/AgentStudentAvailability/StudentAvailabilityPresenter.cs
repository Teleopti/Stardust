using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using Teleopti.Ccc.AgentPortal.AgentPreferenceView;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Common.Clipboard;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortalCode.AgentStudentAvailability
{
    public class StudentAvailabilityPresenter
    {
        private readonly StudentAvailabilityModel _model;
        private readonly IStudentAvailabilityView _view;
        private readonly ClipHandler<IStudentAvailabilityCellData> _clipHandler;
        private readonly IToggleButtonState _parent;

        public StudentAvailabilityPresenter(StudentAvailabilityModel model, IStudentAvailabilityView studentAvailabilityView, ClipHandler<IStudentAvailabilityCellData> clipHandler, IToggleButtonState parent)
        {
            _model = model;
            _view = studentAvailabilityView;
            _clipHandler = clipHandler;
            _parent = parent;
        }

        public ClipHandler<IStudentAvailabilityCellData> CellDataClipHandler
        {
            get { return _clipHandler; }
        }

        public Dictionary<int, IStudentAvailabilityCellData> CellDataCollection
        {
            get { return _model.CellDataCollection; }
        }

        public string PeriodInfo()
        {
            if (_model.LoggedOnPerson.WorkflowControlSet != null)
            {
                var period = _model.LoggedOnPerson.WorkflowControlSet.StudentAvailabilityPeriod;
                var inputPeriod = _model.LoggedOnPerson.WorkflowControlSet.StudentAvailabilityInputPeriod;
                var info = _model.CurrentCultureInfo();
                var studentAvailabilityPeriod = string.Format(CultureInfo.CurrentUICulture, "{0}: {1} - {2}", Resources.StudentAvailabilityPeriod, period.StartDate.DateTime.Date.ToString("d", info), period.EndDate.DateTime.Date.ToString("d", info));
                var studentAvailabilityInputPeriod = string.Concat(Resources.IsOpen, ": ",
                                                                   inputPeriod.StartDate.DateTime.Date.ToString("d", info), " - ",
                                                                   inputPeriod.EndDate.DateTime.Date.ToString("d", info));
                return string.Concat(studentAvailabilityPeriod, " | ", studentAvailabilityInputPeriod);
            }
            return Resources.YouDontHaveAWorkflowControlSet;
        }

        public void ReloadPeriod()
        {
            _model.ReloadPeriod();
            _view.CellDataLoaded();
            ValidationInfoText();
            _view.RefreshEditStudentAvailabilityView();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxYouShouldWorkAtLeastParameterH"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DotYouAreNowAvailableForBetweenParameterH"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "AndParameterH"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.AgentPortalCode.AgentStudentAvailability.IStudentAvailabilityView.SetValidationInfoText(System.String,System.Drawing.Color)")]
        private void ValidationInfoText()
        {
            if (!_model.PeriodIsValid())
            {
                _view.SetValidationPicture(Properties.Resources.ccc_Cancel_32x32);
                _parent.ToggleButtonEnabled("toolStripButtonSAValidate", false);
                return;
            }

            TimePeriod timePeriod = _model.CurrentPeriodTime();
            TimeSpan periodTarget = _model.BalancedPeriodTargetTime();

            string text = string.Format(_model.CurrentUICultureInfo(), Resources.YouShouldWorkAtLeastParameterHDotYouAreNowAvailableForBetweenParameterHAndParameterHDot,
                                         TimeHelper.GetLongHourMinuteTimeString(periodTarget, _model.CurrentCultureInfo()), TimeHelper.GetLongHourMinuteTimeString(timePeriod.StartTime, _model.CurrentCultureInfo()), TimeHelper.GetLongHourMinuteTimeString(timePeriod.EndTime, _model.CurrentCultureInfo()));

            var color = timePeriod.EndTime >= periodTarget ? Color.Green : Color.Red;
            _view.SetValidationInfoText(text, color);
            _parent.ToggleButtonEnabled("toolStripButtonSAValidate", false);
        }

        public void OnPasteCellDataClip(int top, int bottom, int left, int right)
        {
            IList<int> cells = new List<int>();
            for (int row = top; row <= bottom; row++)
            {
                for (int col = left; col <= right; col++)
                {
                    int index = ((row - 1) * 7) + (col - 1);
                    cells.Add(index);
                }
            }
            PasteClipsInCellData(cells);
        }

        private void PasteClipsInCellData(IList<int> cellDataToChange)
        {
            IList<IStudentAvailabilityCellData> cellDataToPersist = new List<IStudentAvailabilityCellData>();
            if (_clipHandler.ClipList.Count == 0)
                return;

            FakePasteRolling(cellDataToChange);

            int index = 0;
            int disabledCellSelected = 0;
            foreach (var i in cellDataToChange)
            {
                if (i >= _model.CellDataCollection.Count || i < 0)
                    return;
                if (index == _clipHandler.ClipList.Count)
                    index = 0;
                _clipHandler.ClipList[index].ClipValue.TheDate = _model.CellDataCollection[i].TheDate;
                _model.CellDataCollection[i].StudentAvailabilityRestrictions = _clipHandler.ClipList[index].ClipValue.StudentAvailabilityRestrictions;
                cellDataToPersist.Add(_model.CellDataCollection[i]);
                if (!_model.CellDataCollection[i].Enabled)
                    disabledCellSelected++;
                index++;
            }
            if (disabledCellSelected == cellDataToChange.Count) return;
            PersistMixedCellData(cellDataToPersist);
            _view.CellDataLoaded();
            _view.SetValidationPicture(Properties.Resources.ccc_ForecastValidate);
            _parent.ToggleButtonEnabled("toolStripButtonSAValidate", true);
        }

        private void FakePasteRolling(IList<int> cellDataToChange)
        {
            if (_clipHandler.ClipList.Count > 1 && cellDataToChange.Count == 1)
            {
                int startIndex = cellDataToChange[0] + 1;

                for (int i = startIndex; i < startIndex + _clipHandler.ClipList.Count - 1; i++)
                {
                    cellDataToChange.Add(i);
                }
            }
        }

        public void OnSetCellDataCut(int top, int bottom, int left, int right)
        {
            bool validate = false;
            for (int row = top; row <= bottom; row++)
            {
                for (int col = left; col <= right; col++)
                {
                    int index = ((row - 1) * 7) + (col - 1);

                    IStudentAvailabilityCellData cellData;
                    if (CellDataCollection.TryGetValue(index, out cellData))
                    {
                        if (cellData.StudentAvailabilityRestrictions == null)
                            continue;

                        IStudentAvailabilityCellData newCellData = CopyCellData(cellData);
                        CellDataClipHandler.AddClip(newCellData, ScheduleAppointmentTypes.StudentAvailability, newCellData.TheDate);
                        CellDataCollection[index].StudentAvailabilityRestrictions = null;
                        AgentScheduleStateHolder.Instance().DeleteStudentAvailability(cellData.TheDate);
                        _parent.ToggleButtonEnabled("clipboardControlStudentAvailability", true);
                        _view.ToggleStateContextMenuItemPaste(true);
                        _parent.ToggleButtonEnabled("toolStripButtonValidate", true);
                        validate = true;
                    }
                }
            }
            if (validate)
                _view.SetValidationPicture(Properties.Resources.ccc_ForecastValidate);
        }

        private static IStudentAvailabilityCellData CopyCellData(IStudentAvailabilityCellData cellData)
        {
            var newCellData = new StudentAvailabilityCellData();

            var newStudentAvailaibilityRestrictions = new List<StudentAvailabilityRestriction>();
            foreach(var restriction in cellData.StudentAvailabilityRestrictions)
            {
                var newRestriction = new StudentAvailabilityRestriction();
                newRestriction.StartTimeLimitation = restriction.StartTimeLimitation;
                newRestriction.EndTimeLimitation = restriction.EndTimeLimitation;
                newStudentAvailaibilityRestrictions.Add(newRestriction);
            }
            newCellData.StudentAvailabilityRestrictions = newStudentAvailaibilityRestrictions;
            newCellData.TheDate = new DateTime();
            newCellData.Enabled = cellData.Enabled;
            return newCellData;
        }

        public void OnDelete(int top, int bottom, int left, int right)
        {
            bool validate = false;
            for (int row = top; row <= bottom; row++)
            {
                for (int col = left; col <= right; col++)
                {
                    int index = ((row - 1) * 7) + (col - 1);

                    IStudentAvailabilityCellData cellData;
                    if (CellDataCollection.TryGetValue(index, out cellData))
                    {
                        if (cellData.Enabled)
                        {
                            cellData.StudentAvailabilityRestrictions = null;
                            AgentScheduleStateHolder.Instance().DeleteStudentAvailability(cellData.TheDate);
                            validate = true;
                        }
                    }
                }
            }
            if (validate)
            {
                _view.SetValidationPicture(Properties.Resources.ccc_ForecastValidate);
                _view.RefreshEditStudentAvailabilityView();
                _parent.ToggleButtonEnabled("toolStripButtonSAValidate", true);
            }    
        }

        public void OnSetCellDataClip(int top, int bottom, int left, int right)
        {
            IStudentAvailabilityCellData cellData = new StudentAvailabilityCellData();
            for (int row = top; row <= bottom; row++)
            {
                for (int col = left; col <= right; col++)
                {
                    int index = ((row - 1) * 7) + (col - 1);
                    if (CellDataCollection.TryGetValue(index, out cellData))
                    {
                        if (cellData.StudentAvailabilityRestrictions == null)
                            continue;

                        IStudentAvailabilityCellData newCellData = CopyCellData(cellData);
                        CellDataClipHandler.AddClip(newCellData, ScheduleAppointmentTypes.StudentAvailability, newCellData.TheDate);
                        _parent.ToggleButtonEnabled("clipboardControlStudentAvailability", true);
                        _view.ToggleStateContextMenuItemPaste(true);
                    }
                }
            }
            if(cellData != null && cellData.StudentAvailabilityRestrictions == null)
            {
                _parent.ToggleButtonEnabled("clipboardControlStudentAvailability", false);
                _view.ToggleStateContextMenuItemPaste(false);
            }
        }

        public void OnSelectColumns(int left, int right, int headerCount, int rowCount)
        {
            int top = headerCount + 1;
            int bottom = rowCount;
            _view.SelectColumns(left, right, top, bottom);
        }

        public void OnSelectRows(int colHeaderCount, int colCount, int top, int bottom)
        {
            int left = colHeaderCount + 1;
            int right = colCount;
            _view.SelectRows(left, right, top, bottom);
        }

        public void OnSelectAll(int rowHeaderCount, int colsHeaderCount, int colCount, int rowCount)
        {
            int top = rowHeaderCount + 1;
            int left = colsHeaderCount + 1;
            int right = colCount;
            int bottom = rowCount;
            _view.SelectAll(left, right, top, bottom);
        }
        public string OnQueryColumnHeaderText(int colIndex)
        {
            int index = colIndex - 1;
            IStudentAvailabilityCellData cellData;
            CellDataCollection.TryGetValue(index, out cellData);
            if (cellData == null)
                return "";

            return _model.CurrentUICultureInfo().DateTimeFormat.GetDayName(_model.CurrentCultureInfo().Calendar.GetDayOfWeek(cellData.TheDate));
        }

        public WeekHeaderCellData OnQueryWeekHeader(int rowIndex)
        {
            int stop = ((rowIndex - 1) * 7) + 7;
            var minTime = new TimeSpan();
            var maxTime = new TimeSpan();
            IStudentAvailabilityCellData cellData;
            bool dayIsInvalid = false;
            bool dayHasEffectiveRestriction = true;
            WeekHeaderCellData weekHeaderCell;

            PersonDto person = StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson;
            CultureInfo cultureUi = (person.CultureLanguageId.HasValue
                                        ? CultureInfo.GetCultureInfo(person.CultureLanguageId.Value)
                                        : CultureInfo.CurrentCulture).FixPersianCulture();

            int weekNumber = 0;

            TimeSpan weekMax = TimeSpan.Zero;
            for (int index = ((rowIndex - 1) * 7); index < stop; index++)
            {
                if (index >= CellDataCollection.Count)
                    break;
                CellDataCollection.TryGetValue(index, out cellData);

                if (cellData.EffectiveRestriction != null)
                {
                    if (cellData.EffectiveRestriction.WorkTimeLimitation != null)
                    {
                        minTime =
                            minTime.Add(
                                cellData.EffectiveRestriction.WorkTimeLimitation.MinTime.GetValueOrDefault(TimeSpan.Zero));
                        maxTime =
                            maxTime.Add(
                                cellData.EffectiveRestriction.WorkTimeLimitation.MaxTime.GetValueOrDefault(TimeSpan.Zero));
                    }
                    if (cellData.EffectiveRestriction.Invalid)
                        dayIsInvalid = true;
                }
                else
                    dayHasEffectiveRestriction = false;

                weekMax = cellData.WeeklyMax;
                weekNumber = DateHelper.WeekNumber(cellData.TheDate, cultureUi);
            }
            bool weekIsLegal = minTime <= weekMax;
            if (dayIsInvalid)
                weekHeaderCell = new WeekHeaderCellData(true);
            else if (!dayHasEffectiveRestriction)
                weekHeaderCell = new WeekHeaderCellData(false);
            else
                weekHeaderCell = new WeekHeaderCellData(minTime, maxTime, !weekIsLegal, weekNumber);

            return weekHeaderCell;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
        public bool OnQueryCellInfo(int colIndex, int rowIndex, out IStudentAvailabilityCellData cellData)
        {
            cellData = new StudentAvailabilityCellData();
            if (rowIndex < 1 || colIndex < 1)
                return false;
            int currentCell = ((rowIndex - 1) * 7) + colIndex;

            if (CellDataCollection.TryGetValue(currentCell - 1, out cellData))
                return true;

            return false;   
        }

        public void GetNextPeriod()
        {
            _model.GetNextPeriod();
            AfterPeriodChange();
        }

        public void GetPreviousPeriod()
        {
            _model.GetPreviousPeriod();
            AfterPeriodChange();
        }

        private void AfterPeriodChange()
        {
           _view.SetupContextMenu();
           _view.CellDataLoaded();
           ValidationInfoText();

        }

        public void OnAddStudentAvailabilityRestrictions(int top, int bottom, int left, int right, IList<StudentAvailabilityRestriction> studentAvailabilityRestrictions)
        {
            IList<IStudentAvailabilityCellData> cellDataToPersist = new List<IStudentAvailabilityCellData>();
            for (int row = top; row <= bottom; row++)
            {
                for (int col = left; col <= right; col++)
                {
                    int index = ((row - 1) * 7) + (col - 1);

                    IStudentAvailabilityCellData cellData;
                    if (CellDataCollection.TryGetValue(index, out cellData))
                    {
                        var cloneStudentAvailabilityRestrictions = new List<StudentAvailabilityRestriction>();
                        cloneStudentAvailabilityRestrictions.AddRange(studentAvailabilityRestrictions);
                        cellData.StudentAvailabilityRestrictions = cloneStudentAvailabilityRestrictions;
                        cellDataToPersist.Add(cellData);
                    }
                }
            }
            PersistMixedCellData(cellDataToPersist);
            _view.SetValidationPicture(Properties.Resources.ccc_ForecastValidate);
            _view.RefreshEditStudentAvailabilityView();
            _parent.ToggleButtonEnabled("toolStripButtonSAValidate", true);
        }
        
        private static void PersistMixedCellData(IList<IStudentAvailabilityCellData> cellDataToPersist)
        {
            IDictionary<IList<StudentAvailabilityRestriction>, IList<DateTime>> studentAvailabileDict = new Dictionary<IList<StudentAvailabilityRestriction>, IList<DateTime>>();

            foreach (var cellData in cellDataToPersist)
            {
                if (cellData.StudentAvailabilityRestrictions == null)
                    continue;

                IList<DateTime> dates;
                if (!studentAvailabileDict.TryGetValue(cellData.StudentAvailabilityRestrictions, out dates))
                {
                    dates = new List<DateTime> { cellData.TheDate };
                    studentAvailabileDict.Add(cellData.StudentAvailabilityRestrictions, dates);
                }
                else
                {
                    studentAvailabileDict[cellData.StudentAvailabilityRestrictions].Add(cellData.TheDate);
                }
            }

            foreach (KeyValuePair<IList<StudentAvailabilityRestriction>, IList<DateTime>> keyValuePair in studentAvailabileDict)
            {
                AgentScheduleStateHolder.Instance().UpdateOrAddStudentAvailability(keyValuePair.Value, keyValuePair.Key);
            }
        }
    }
}