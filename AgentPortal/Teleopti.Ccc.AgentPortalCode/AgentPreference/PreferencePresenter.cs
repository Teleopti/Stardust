using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.AgentPortal.AgentPreferenceView;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Common.Clipboard;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using DayOff = Teleopti.Ccc.AgentPortalCode.Common.DayOff;

namespace Teleopti.Ccc.AgentPortalCode.AgentPreference
{
    public interface IPreferencePresenter
    {
        bool HasOpenDays();
    }

    public class PreferencePresenter : IPreferencePresenter
    {
        private readonly IPreferenceModel _model;
        private readonly IPreferenceView _view;
        private readonly ClipHandler<IPreferenceCellData> _cellDataClipHandler;
        private readonly IToggleButtonState _parent;
        private static readonly TimeLengthValidator TimeLengthValidator = new TimeLengthValidator();
        private static readonly TimeOfDayValidator TimeOfDayValidatorStartTime = new TimeOfDayValidator(false);
        private static readonly TimeOfDayValidator TimeOfDayValidatorEndTime = new TimeOfDayValidator(true);
        private bool _template = true;
        private readonly IAgentScheduleStateHolder _scheduleStateHolder;

        public PreferencePresenter(IPreferenceModel model, IPreferenceView view, ClipHandler<IPreferenceCellData> cellDataClipHandler, IToggleButtonState parent, IAgentScheduleStateHolder scheduleStateHolder)
        {
            _model = model;
            _view = view;
            _cellDataClipHandler = cellDataClipHandler;
            _parent = parent;
            _scheduleStateHolder = scheduleStateHolder;
        }

        public Dictionary<int, IPreferenceCellData> CellDataCollection
        {
            get { return _model.CellDataCollection; }
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

        public string PeriodInfo()
        {
            if (_model.LoggedOnPerson.WorkflowControlSet != null)
            {
                DateOnlyPeriodDto period = _model.LoggedOnPerson.WorkflowControlSet.PreferencePeriod;
                DateOnlyPeriodDto inputPeriod = _model.LoggedOnPerson.WorkflowControlSet.PreferenceInputPeriod;
                CultureInfo info = _model.CurrentCultureInfo();
                string preferencePeriod = string.Format(CultureInfo.CurrentUICulture, "{0}: {1} - {2}", Resources.PreferencePeriod, period.StartDate.DateTime.Date.ToString("d", info), period.EndDate.DateTime.Date.ToString("d", info));
                string preferenceInputPeriod = string.Concat(Resources.IsOpen, ": ",
                                               inputPeriod.StartDate.DateTime.Date.ToString("d", info), " - ",
                                               inputPeriod.EndDate.DateTime.Date.ToString("d", info));
                return string.Concat(preferencePeriod, " | ", preferenceInputPeriod);
            }
            return Resources.YouDontHaveAWorkflowControlSet;
        }

        private void AfterPeriodChange()
        {
            _view.SetShiftCategories(_model.ShiftCategories);
            _view.SetDaysOff(_model.DaysOff);
            _view.SetAbsences(_model.Absences);
            _view.CellDataLoaded();
            LoadContextMenu();
            ValidationInfoText();
            MustHaveInfo();
        }

        public void ReloadPeriod()
        {
            _model.ReloadPeriod();
            _view.CellDataLoaded();
            ValidationInfoText();
            MustHaveInfo();
            _view.RefreshExtendedPreference();
        }
        private void MustHaveInfo()
        {
            _parent.SetMustHaveText("\n" + _model.NumberOfMustHaveCurrentPeriod + "(" + _model.MaxMustHaveCurrentPeriod + ")");
        }

        public DateTime FirstDateOfPeriod
        {
            get { return _model.FirstDateCurrentPeriod; }
        }

        private void ValidationInfoText()
        {
            if (!_model.PeriodIsValid())
            {
                _view.SetValidationPicture(Properties.Resources.ccc_Cancel_32x32);
                _parent.ToggleButtonEnabled("toolStripButtonValidate", false);
                return;
            }

            TimePeriod timePeriod = _model.CurrentPeriodTime();
            var periodPeriod = _model.BalancedTargetTimeWithTolerance();

            string parameter1;
            if (periodPeriod.StartTime == periodPeriod.EndTime)
                parameter1 =
                    TimeHelper.GetLongHourMinuteTimeString(periodPeriod.StartTime, _model.CurrentCultureInfo());
            else
            {
                parameter1 =
                    TimeHelper.GetLongHourMinuteTimeString(periodPeriod.StartTime, _model.CurrentCultureInfo()) + "--" +
                    TimeHelper.GetLongHourMinuteTimeString(periodPeriod.EndTime, _model.CurrentCultureInfo());
            }

            string text = string.Format(_model.CurrentUICultureInfo(), Resources.YouShouldWorkParameterHDotYourPreferencesCanNowResultInParameterHToParameterHDot,
                                         parameter1, TimeHelper.GetLongHourMinuteTimeString(timePeriod.StartTime, _model.CurrentCultureInfo()), TimeHelper.GetLongHourMinuteTimeString(timePeriod.EndTime, _model.CurrentCultureInfo()));
            string dayOffsText = "";
            if (_model.PeriodTargetDayOffs() != 0)
                dayOffsText = string.Format(_model.CurrentUICultureInfo(),
                                            Resources.YouShouldHaveDaysOffParameterDotYouHaveParameterDot,
                                            _model.PeriodTargetDayOffs(), _model.PeriodDayOffs());
            Color color = Color.Green;
            Color colorDayOffs = Color.Green;
            if (!timePeriod.Intersect(periodPeriod))
                color = Color.Red;
            if (_model.PeriodDayOffs() != _model.PeriodTargetDayOffs())
                colorDayOffs = Color.Red;
            _view.SetValidationInfoText(text, color, dayOffsText, colorDayOffs, _model.CalculationInfo);
            _parent.ToggleButtonEnabled("toolStripButtonValidate", false);
        }

        public ClipHandler<IPreferenceCellData> CellDataClipHandler
        {
            get { return _cellDataClipHandler; }
        }

        public bool ExtendedPreferenceTemplate
        {
            get { return _template; }
        }

        public void PasteTemplateNameInCellData(IList<int> cellDataToChange, string name)
        {
            IList<IPreferenceCellData> cellDataToPersist = new List<IPreferenceCellData>();
            //int index = 0;
            foreach (var i in cellDataToChange)
            {
                _model.CellDataCollection[i].Preference.TemplateName = name;
                cellDataToPersist.Add(_model.CellDataCollection[i]);
                //index++;
            }
            PersistMixedCellData(cellDataToPersist);
            _view.CellDataLoaded();
            _view.SetValidationPicture(Properties.Resources.ccc_ForecastValidate);
            _parent.ToggleButtonEnabled("toolStripButtonValidate", true);
        }

        public void PasteClipsInCellData(IList<int> cellDataToChange)
        {
            IList<IPreferenceCellData> cellDataToPersist = new List<IPreferenceCellData>();
            if (_cellDataClipHandler.ClipList.Count == 0)
                return;

            FakePasteRolling(cellDataToChange);

            int index = 0;
            foreach (var i in cellDataToChange)
            {
                if (i >= _model.CellDataCollection.Count || i < 0)
                    return;
                if (index == _cellDataClipHandler.ClipList.Count)
                    index = 0;
                _cellDataClipHandler.ClipList[index].ClipValue.TheDate = _model.CellDataCollection[i].TheDate;
                Preference preference = _cellDataClipHandler.ClipList[index].ClipValue.Preference;
                if (ValidatePreference(preference))
                {
                    _model.CellDataCollection[i].Preference = (Preference) _cellDataClipHandler.ClipList[index].ClipValue.Preference.Clone();
                    cellDataToPersist.Add(_model.CellDataCollection[i]);
                }
                index++;
            }
            PersistMixedCellData(cellDataToPersist);
            _view.CellDataLoaded();
            _view.SetValidationPicture(Properties.Resources.ccc_ForecastValidate);
            _parent.ToggleButtonEnabled("toolStripButtonValidate", true);
        }

        private void FakePasteRolling(IList<int> cellDataToChange)
        {
            if (_cellDataClipHandler.ClipList.Count > 1 && cellDataToChange.Count == 1)
            {
                int startIndex = cellDataToChange[0] + 1;

                for (int i = startIndex; i < startIndex + _cellDataClipHandler.ClipList.Count - 1; i++)
                {
                    cellDataToChange.Add(i);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "headerCount+1")]
        public void OnSelectColumns(int left, int right, int headerCount, int rowCount)
        {
            int top = headerCount + 1;
            int bottom = rowCount;
            _view.SelectColumns(left, right, top, bottom);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "colHeaderCount+1")]
        public void OnSelectRows(int colHeaderCount, int colCount, int top, int bottom)
        {
            int left = colHeaderCount + 1;
            int right = colCount;
            _view.SelectRows(left, right, top, bottom);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "rowHeaderCount+1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "colsHeaderCount+1")]
        public void OnSelectAll(int rowHeaderCount, int colsHeaderCount, int colCount, int rowCount)
        {
            int top = rowHeaderCount + 1;
            int left = colsHeaderCount + 1;
            int right = colCount;
            int bottom = rowCount;
            _view.SelectAll(left, right, top, bottom);
        }

        public void OnTemplateCell(int top, int bottom, int left, int right)
        {
            _template = true;
            if ((top != bottom) || (left != right))
            {
                _template = false;
            }
            else
            {
                for (int row = top; row <= bottom; row++)
                {
                    for (int col = left; col <= right; col++)
                    {
                        int index = ((row - 1)*7) + (col - 1);

                        IPreferenceCellData cellData;
                        if (!CellDataCollection.TryGetValue(index, out cellData)) continue;
                        if (!cellData.Enabled)
                        {
                            _template = false;
                        }
                        else
                        {
                            if ((cellData.Preference != null) && (cellData.Preference.TemplateName != null))
                            {
                                _template = true;
                            }
                        }
                    }
                }
            }
        }

        public void OnSetCellDataClip(int top, int bottom, int left, int right)
        {
            for (int row = top; row <= bottom; row++)
            {
                for (int col = left; col <= right; col++)
                {
                    int index = ((row - 1) * 7) + (col - 1);

                    IPreferenceCellData cellData;
                    if (CellDataCollection.TryGetValue(index, out cellData))
                    {
                        if (cellData.Preference == null)
                            continue;

                        IPreferenceCellData newCellData = CopyCellData(cellData);
                        CellDataClipHandler.AddClip(newCellData, ScheduleAppointmentTypes.PreferenceRestriction, newCellData.TheDate);
                        _parent.ToggleButtonEnabled("clipboardControl", true);
                        _view.ToggleStateContextMenuItemPaste(true);
                    }
                }
            }
        }

        private static IPreferenceCellData CopyCellData(IPreferenceCellData cellData)
        {
            var newCellData = new PreferenceCellData();
            var newPreference = new Preference
                                    {
                                        DayOff = cellData.Preference.DayOff,
                                        ShiftCategory = cellData.Preference.ShiftCategory,
                                        Absence = cellData.Preference.Absence,
                                        TemplateName = cellData.Preference.TemplateName,
                                        StartTimeLimitation = (TimeLimitation) cellData.Preference.StartTimeLimitation.Clone(),
                                        EndTimeLimitation = (TimeLimitation) cellData.Preference.EndTimeLimitation.Clone(),
                                        WorkTimeLimitation = (TimeLimitation) cellData.Preference.WorkTimeLimitation.Clone(),
                                        Activity = cellData.Preference.Activity
                                    };

            if (cellData.Preference.ActivityStartTimeLimitation != null)
            {
                newPreference.ActivityStartTimeLimitation = (TimeLimitation)cellData.Preference.ActivityStartTimeLimitation.Clone();
            }
            if (cellData.Preference.ActivityEndTimeLimitation != null)
            {
                newPreference.ActivityEndTimeLimitation = (TimeLimitation)cellData.Preference.ActivityEndTimeLimitation.Clone();
            }
            if (cellData.Preference.ActivityTimeLimitation != null)
            {
                newPreference.ActivityTimeLimitation = (TimeLimitation)cellData.Preference.ActivityTimeLimitation.Clone();
            }
            newCellData.Preference = newPreference;
            newCellData.TheDate = new DateTime();
            newCellData.Enabled = cellData.Enabled;
            return newCellData;
        }

        public void OnSetCellDataCut(int top, int bottom, int left, int right)
        {
            bool validate = false;
            for (int row = top; row <= bottom; row++)
            {
                for (int col = left; col <= right; col++)
                {
                    int index = ((row - 1) * 7) + (col - 1);

                    IPreferenceCellData cellData;
                    if (CellDataCollection.TryGetValue(index, out cellData))
                    {
                        if (cellData.Preference == null)
                            continue;

                        IPreferenceCellData newCellData = CopyCellData(cellData);
                        CellDataClipHandler.AddClip(newCellData, ScheduleAppointmentTypes.PreferenceRestriction, newCellData.TheDate);
                        CellDataCollection[index].Preference = CreateEmptyPreference();
                        AgentScheduleStateHolder.Instance().DeletePreference(cellData.TheDate);
                        _parent.ToggleButtonEnabled("clipboardControl", true);
                        _view.ToggleStateContextMenuItemPaste(true);
                        _parent.ToggleButtonEnabled("toolStripButtonValidate", true);
                        validate = true;
                    }
                }
            }
            if (validate)
                _view.SetValidationPicture(Properties.Resources.ccc_ForecastValidate);
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

        public void OnSaveTemplateCellData(int top, int bottom, int left, int right, string name)
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
            PasteTemplateNameInCellData(cells, name);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
        public bool OnQueryCellInfo(int colIndex, int rowIndex, out IPreferenceCellData cellData)
        {
            cellData = new PreferenceCellData();
            if (rowIndex < 1 || colIndex < 1)
                return false;
            int currentCell = ((rowIndex - 1) * 7) + colIndex;

            if (CellDataCollection.TryGetValue(currentCell - 1, out cellData))
                return true;

            return false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "colIndex-1")]
        public string OnQueryColumnHeaderText(int colIndex)
        {
            int index = colIndex - 1;
            IPreferenceCellData cellData;
            CellDataCollection.TryGetValue(index, out cellData);
            if (cellData == null)
                return "";

            return _model.CurrentUICultureInfo().DateTimeFormat.GetDayName(_model.CurrentCultureInfo().Calendar.GetDayOfWeek(cellData.TheDate));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "rowIndex-1")]
        public WeekHeaderCellData OnQueryWeekHeader(int rowIndex)
        {
            int stop = ((rowIndex - 1) * 7) + 7;
            var minTime = new TimeSpan();
            var maxTime = new TimeSpan();
            IPreferenceCellData cellData;
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
            //Om någon dag cellData.EffectiveRestriction.Invalid
            if (dayIsInvalid)
                weekHeaderCell = new WeekHeaderCellData(true);
            //Om en dag saknar effectiv restriction
            else if (!dayHasEffectiveRestriction)
                weekHeaderCell = new WeekHeaderCellData(false);
            //Alla dagar har effective restriction och ingen är invalid
            else
                weekHeaderCell = new WeekHeaderCellData(minTime, maxTime, !weekIsLegal, weekNumber);//överskrider veckomax
            //Ledig dag är validated

            return weekHeaderCell;
        }

        public void OnDelete(int top, int bottom, int left, int right)
        {
            bool validate = false;
            for (int row = top; row <= bottom; row++)
            {
                for (int col = left; col <= right; col++)
                {
                    int index = ((row - 1) * 7) + (col - 1);

                    IPreferenceCellData cellData;
                    if (CellDataCollection.TryGetValue(index, out cellData))
                    {
                        if (cellData.Enabled)
                        {
                            if (cellData.Preference != null && cellData.Preference.MustHave)
                                _model.RemoveMustHave();
                            cellData.Preference = CreateEmptyPreference();
                            AgentScheduleStateHolder.Instance().DeletePreference(cellData.TheDate);
                            validate = true;
                        }
                    }
                }
            }
            if (validate)
            {
                _view.SetValidationPicture(Properties.Resources.ccc_ForecastValidate);
                _view.RefreshExtendedPreference();
                _parent.ToggleButtonEnabled("toolStripButtonValidate", true);
                _parent.ToggleButtonEnabled("toolStripButtonMustHave", false);
                MustHaveInfo();
            }
        }

        public void OnSaveTemplate(string name, int top, int bottom, int left, int right)
        {
            var template = new ExtendedPreferenceTemplateDto {Name = name};
            var colorDto = new ColorDto(Color.FromArgb(0,0,123));
            template.DisplayColor = colorDto;

            for (var row = top; row <= bottom; row++)
            {
                for (var col = left; col <= right; col++)
                {
                    var index = ((row - 1) * 7) + (col - 1);

                    IPreferenceCellData cellData;
                    if (!CellDataCollection.TryGetValue(index, out cellData)) continue;
                    if (cellData.Preference == null)
                    {
                        cellData.Preference = CreateEmptyPreference();
                    }
                    if (cellData.Preference.Activity != null)
                    {
                        var activityRestrictionDto = new ActivityRestrictionDto();
                        var activityDto = new ActivityDto();
                        var cellActivity = cellData.Preference.Activity;
                        activityDto.Id = cellActivity.Id;
                        activityDto.Description = cellActivity.Name;

                        activityRestrictionDto.StartTimeLimitation = cellData.Preference.ActivityStartTimeLimitation.SetValuesToDto();
                        activityRestrictionDto.EndTimeLimitation = cellData.Preference.ActivityEndTimeLimitation.SetValuesToDto();
                        activityRestrictionDto.WorkTimeLimitation = cellData.Preference.ActivityTimeLimitation.SetValuesToDto();

                        activityRestrictionDto.Activity = activityDto;

                        template.ActivityRestrictionCollection.Clear();
                        template.ActivityRestrictionCollection.Add(activityRestrictionDto);
                    }

                    if (cellData.Preference.DayOff != null)
                    {
                        var dayOff = new DayOffInfoDto
                        {
                            Name = cellData.Preference.DayOff.Name,
                            ShortName = cellData.Preference.DayOff.ShortName,
                            Id = cellData.Preference.DayOff.Id
                        };
                        template.DayOff = dayOff;
                    }

                    if (cellData.Preference.ShiftCategory != null)
                    {
                        var shiftCategory = new ShiftCategoryDto
                        {
                            Name = cellData.Preference.ShiftCategory.Name,
                            ShortName = cellData.Preference.ShiftCategory.ShortName,
                            Id = cellData.Preference.ShiftCategory.Id
                        };
                        var color = ColorHelper.CreateColorDto(cellData.Preference.ShiftCategory.DisplayColor);
                        shiftCategory.DisplayColor = color;
                        template.ShiftCategory = shiftCategory;
                    }

                    AddAbsenceToExtendedPreferenceTemplateDto(template, cellData);

                    template.StartTimeLimitation = cellData.Preference.StartTimeLimitation.SetValuesToDto();
                    template.EndTimeLimitation = cellData.Preference.EndTimeLimitation.SetValuesToDto();
                    template.WorkTimeLimitation = cellData.Preference.WorkTimeLimitation.SetValuesToDto();
                }
            }
            SdkServiceHelper.SchedulingService.SaveExtendedPreferenceTemplate(template);
            OnSaveTemplateCellData(top, bottom, left, right, template.Name);
            _parent.ToggleButtonEnabled("refresh", false);
        }

        public void AddAbsenceToExtendedPreferenceTemplateDto(ExtendedPreferenceTemplateDto dto, IPreferenceCellData cellData)
        {
            if(dto == null)
                throw new ArgumentNullException("dto");

            if(cellData == null)
                throw new ArgumentNullException("cellData");

            if (_template == false ||cellData.Preference == null || cellData.Preference.Absence == null) return;
            var color = ColorHelper.CreateColorDto(cellData.Preference.Absence.Color);
            var absenceDto = new AbsenceDto()
                                 {
                                     Name = cellData.Preference.Absence.Name,
                                     ShortName = cellData.Preference.Absence.ShortName,
                                     Id = cellData.Preference.Absence.Id,
                                     DisplayColor = color
                                 };

            dto.Absence = absenceDto;
        }

        public void OnAddPreference(int top, int bottom, int left, int right, Preference preference)
        {
            //Verify that preference is valid for now!
            if (!ValidatePreference(preference))
            {
                return;
            }
            IList<IPreferenceCellData> cellDataToPersist = new List<IPreferenceCellData>();
            for (int row = top; row <= bottom; row++)
            {
                for (int col = left; col <= right; col++)
                {
                    int index = ((row - 1) * 7) + (col - 1);

                    IPreferenceCellData cellData;
                    if (CellDataCollection.TryGetValue(index, out cellData))
                    {
                        if (!cellData.Enabled) continue;

						var previousStateMustHave = cellData.Preference != null && cellData.Preference.MustHave;
						var clonedPreference = (Preference)preference.Clone();
						cellData.Preference = clonedPreference;
						cellData.Preference.MustHave = previousStateMustHave;
                        cellDataToPersist.Add(cellData);
                    }
                }
            }
            PersistMixedCellData(cellDataToPersist);
            _view.SetValidationPicture(Properties.Resources.ccc_ForecastValidate);
            _view.RefreshExtendedPreference();
            _parent.ToggleButtonEnabled("toolStripButtonValidate", true);
            _parent.ToggleButtonEnabled("toolStripButtonMustHave", true);
        }

        private bool ValidatePreference(Preference preference)
        {
            bool isValid = true;
            string itemName = string.Empty;
            if (preference.DayOff != null)
            {
                itemName = preference.DayOff.Name;
                bool dayOffExists = false;
                foreach (DayOff dayOff in _model.DaysOff)
                {
                    if (dayOff.Id == preference.DayOff.Id)
                    {
                        dayOffExists = true;
                        break;
                    }
                }
                isValid = dayOffExists;
            }
            if (preference.ShiftCategory != null)
            {
                itemName = preference.ShiftCategory.Name;
                bool shiftCategoryExists = false;
                foreach (ShiftCategory shiftCategory in _model.ShiftCategories)
                {
                    if (shiftCategory.Id == preference.ShiftCategory.Id)
                    {
                        shiftCategoryExists = true;
                        break;
                    }
                }
                isValid = isValid && shiftCategoryExists;
            }

            if(preference.Absence != null)
            {
                itemName = preference.Absence.Name;
                bool absenceExists = false;
                foreach (Absence absence in _model.Absences)
                {
                    if(absence.Id == preference.Absence.Id)
                    {
                        absenceExists = true;
                        break;
                    }
                }

                isValid = isValid && absenceExists;
            }
            
            if (!isValid)
            {
                _view.ShowErrorItemNoLongerAvailable(itemName);
            }

            return isValid;
        }

        private bool ValidateTemplatePreference(Preference preference)
        {
            bool isValid = true;
            if (preference.DayOff != null)
            {
                bool dayOffExists = false;
                foreach (DayOff dayOff in _model.DaysOff)
                {
                    if (dayOff.Id == preference.DayOff.Id)
                    {
                        dayOffExists = true;
                        break;
                    }
                }
                isValid = dayOffExists;
            }
            if (preference.ShiftCategory != null)
            {
                bool shiftCategoryExists = false;
                foreach (ShiftCategory shiftCategory in _model.ShiftCategories)
                {
                    if (shiftCategory.Id == preference.ShiftCategory.Id)
                    {
                        shiftCategoryExists = true;
                        break;
                    }
                }
                isValid = isValid && shiftCategoryExists;
            }
            if (!isValid)
            {
            }

            return isValid;
        }

        public static Preference CreateEmptyPreference()
        {
            return new Preference
            {
                StartTimeLimitation = new TimeLimitation(TimeOfDayValidatorStartTime),
                EndTimeLimitation = new TimeLimitation(TimeOfDayValidatorEndTime),
                WorkTimeLimitation = new TimeLimitation(TimeLengthValidator)
            };
        }

        private void PersistMixedCellData(IEnumerable<IPreferenceCellData> cellDataToPersist)
        {
            IDictionary<IList<Preference>, IList<DateTime>> prefDic = new Dictionary<IList<Preference>, IList<DateTime>>();

            foreach (var cellData in cellDataToPersist)
            {
                if (cellData.Preference == null)
                    continue;

                IList<DateTime> prefDates = new List<DateTime> { cellData.TheDate };
                IList<Preference> preference = new List<Preference> { cellData.Preference };
                prefDic.Add(preference, prefDates);
            }

            foreach (KeyValuePair<IList<Preference>, IList<DateTime>> keyValuePair in prefDic)
            {
                _scheduleStateHolder.UpdateOrAddPreference(keyValuePair.Value, keyValuePair.Key);
            }
        }

        private void LoadContextMenu()
        {
            _view.ClearContextMenus();
            foreach (DayOff dayOff in _model.DaysOff)
            {
                _view.AddDayOffToContextMenu(dayOff);
            }
            foreach (ShiftCategory shiftCategory in _model.ShiftCategories)
            {
                _view.AddShiftCategoryToContextMenu(shiftCategory);
            }
            foreach (Absence absence in _model.Absences)
            {
                _view.AddAbsenceToContextMenu(absence);
            }
            _view.ToggleStateContextMenuItemSaveAsTemplate(true);
            _view.SetupContextMenu();
        }

        public void OnToggleMustHave(int top, int bottom, int left, int right, bool mustHave)
        {
            IList<IPreferenceCellData> cellDataToPersist = new List<IPreferenceCellData>();
            for (int row = top; row <= bottom; row++)
            {
                for (int col = left; col <= right; col++)
                {
                    int index = ((row - 1) * 7) + (col - 1);

                    IPreferenceCellData cellData;
                    if (CellDataCollection.TryGetValue(index, out cellData))
                    {
                        if (cellData.Enabled && cellData.Preference != null)
                        {
                            if (mustHave && _model.NumberOfMustHaveCurrentPeriod < _model.MaxMustHaveCurrentPeriod)
                            {
                                _model.AddMustHave();
                                cellData.Preference.MustHave = true;
                                cellDataToPersist.Add(cellData);
                                MustHaveChanged(true);
                            }
                            if (!mustHave && _model.NumberOfMustHaveCurrentPeriod > 0 && cellData.Preference.MustHave)
                            {
                                _model.RemoveMustHave();
                                cellData.Preference.MustHave = false;
                                cellDataToPersist.Add(cellData);
                                MustHaveChanged(false);
                            }
                        }
                    }
                }
            }
            PersistMixedCellData(cellDataToPersist);
            MustHaveInfo();
        }
        private void MustHaveChanged(bool mustHave)
        {
            ToggleMustHaveState(mustHave);
        }
        public void ToggleMustHaveState(bool? state)
        {
            _parent.ToggleButtonChecked("toolStripButtonMustHave", state);
        }
        public void ToggleMustHaveEnable(bool enabled)
        {
            _parent.ToggleButtonEnabled("toolStripButtonMustHave", enabled);
        }
        public bool? PreferenceExistsOnNotScheduledDay(int top, int bottom, int left, int right)
        {
            bool? exist = null;
            for (int row = top; row <= bottom; row++)
            {
                for (int col = left; col <= right; col++)
                {
                    int index = ((row - 1) * 7) + (col - 1);

                    IPreferenceCellData cellData;
                    if (CellDataCollection.TryGetValue(index, out cellData))
                    {
                        if (cellData.Enabled && cellData.Preference != null)
                        {
                            if (exist.HasValue && !exist.Value)
                                return null;
                            exist = true;
                        }
                        else
                        {
                            if (exist.HasValue && exist.Value)
                                return null;
                            exist = false;
                        }
                    }
                }
            }
            return exist;
        }

        public void OnSelectionChanged(int top, int bottom, int left, int right)
        {
            bool? preferenceExists = PreferenceExistsOnNotScheduledDay(top, bottom, left, right);
            if (preferenceExists.HasValue)
            {
                if (preferenceExists.Value)
                {
                    ToggleMustHaveEnable(true);
                    bool mustHave = IsMustHave(top, bottom, left, right);
                    ToggleMustHaveState(mustHave);
                }
                else
                    ToggleMustHaveEnable(false);
            }
            else
            {
                ToggleMustHaveEnable(false);
                //ToggleMustHaveState(null);
            }
        }

        public bool IsMustHave(int top, int bottom, int left, int right)
        {
            for (int row = top; row <= bottom; row++)
            {
                for (int col = left; col <= right; col++)
                {
                    int index = ((row - 1) * 7) + (col - 1);

                    IPreferenceCellData cellData;
                    if (CellDataCollection.TryGetValue(index, out cellData))
                    {
                        return cellData.Preference.MustHave;
                    }
                }
            }
            return false;
        }

        public static Preference CreatePreferenceFromTemplate(ExtendedPreferenceTemplateDto dto)
        {
            Preference preference = CreateEmptyPreference();
            preference.TemplateName = dto.Name;
            if ((dto.ActivityRestrictionCollection != null) && (dto.ActivityRestrictionCollection.Count > 0))
            {
                var activityRestriction = dto.ActivityRestrictionCollection.First();
                var activity = activityRestriction.Activity;
                preference.Activity = new Activity(activity.Id.GetValueOrDefault(), activity.Description);
                preference.ActivityStartTimeLimitation = new TimeLimitation(TimeOfDayValidatorStartTime, activityRestriction.StartTimeLimitation);
                preference.ActivityEndTimeLimitation = new TimeLimitation(TimeOfDayValidatorEndTime, activityRestriction.EndTimeLimitation);
                preference.ActivityTimeLimitation = new TimeLimitation(TimeLengthValidator,
                                                                       activityRestriction.WorkTimeLimitation);

            }
            if (dto.DayOff != null)
            {
                preference.DayOff = new DayOff(dto.DayOff.Name, dto.DayOff.ShortName, dto.DayOff.Id.GetValueOrDefault(), Color.Empty);
            }
            if (dto.ShiftCategory != null)
            {
                preference.ShiftCategory = new ShiftCategory(dto.ShiftCategory.Name, dto.ShiftCategory.ShortName,
                                                             dto.ShiftCategory.Id.GetValueOrDefault(),
                                                             ColorHelper.CreateColorFromDto(dto.ShiftCategory.DisplayColor));
            }

            if(dto.Absence != null)
            {
                preference.Absence = new Absence(dto.Absence.Name, dto.Absence.ShortName, dto.Absence.Id.GetValueOrDefault(), ColorHelper.CreateColorFromDto(dto.Absence.DisplayColor));    
            }

            preference.StartTimeLimitation = new TimeLimitation(TimeOfDayValidatorStartTime, dto.StartTimeLimitation);
            preference.EndTimeLimitation = new TimeLimitation(TimeOfDayValidatorEndTime, dto.EndTimeLimitation);
            preference.WorkTimeLimitation = new TimeLimitation(TimeLengthValidator, dto.WorkTimeLimitation);
            return preference;
        }

        public bool OnCheckPermission(Preference preference)
        {
            var valid = true;
            //Verify that preference is valid for now!
            if (!ValidateTemplatePreference(preference))
            {
                valid = false;
            }
            return valid;
        }

        public bool HasOpenDays()
        {
            foreach (var preferenceCellData in CellDataCollection)
            {
                if (preferenceCellData.Value.Enabled)
                    return true;
            }
            return false;
        }
    }
}