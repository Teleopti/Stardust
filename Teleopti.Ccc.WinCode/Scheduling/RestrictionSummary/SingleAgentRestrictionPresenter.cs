using System;
using System.Collections.Generic;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary
{
    public class SingleAgentRestrictionPresenter: IDisposable
    {
        private IRestrictionSummaryGrid _grid;
        private SingleAgentRestrictionModel _model;
        private IList<string> _headers;
        private int _numberOfHeaders = 2;
        private bool _isInitialized;
        private ISchedulingOptions _schedulingOptions;
        private IList<IPerson> _persons;
        private ISchedulingResultStateHolder _stateHolder;

        public SingleAgentRestrictionPresenter(IRestrictionSummaryGrid grid, SingleAgentRestrictionModel singleAgentRestrictionModel)
        {
            _schedulingOptions = new RestrictionSchedulingOptions
                                     {
                                         UseAvailability = true,
                                         UsePreferences = true,
                                         UseStudentAvailability = true,
                                         UseRotations = true,
                                         UseScheduling = true 
                                     };
            _model = singleAgentRestrictionModel;
            _grid = grid;
        }

        public ISchedulingResultStateHolder SchedulingResultStateHolder
        {
            get { return _model.StateHolder; }
        }

        public bool IsInitialized
        {
            get { return _isInitialized; }
        }

        public ISchedulingOptions SchedulingOptions
        {
            get { return _schedulingOptions; }
        }

        public void Initialize(IList<IPerson> persons, ISchedulingResultStateHolder stateHolder, ISchedulingOptions schedulingOptions)
        {
            _persons = persons;
            _stateHolder = stateHolder;

            if (schedulingOptions != null)
                _schedulingOptions = schedulingOptions;
            if (!IsInitialized)
            {
                _headers = new List<string>
                               {
                                   Resources.Name,
                                   Resources.Warnings,
                                   Resources.Type,
                                   Resources.From,
                                   Resources.To,
                                   Resources.ContractTargetTime,
                                   Resources.TargetDaysOff,
                                   Resources.ContractTime,
                                   Resources.DaysOff,
                                   Resources.MinTime,
                                   Resources.MaxTime,
                                   Resources.DaysOff,
                                   Resources.Ok
                               };
                _model.Initialize(persons, stateHolder, SchedulingOptions);
                _grid.HeaderCount = 1;
                _grid.RowCount = _model.PersonsAffectedPeriodDates.Count + 1;
                _grid.ColCount = _headers.Count - 1;
                _isInitialized = true;
            }
            else
            {
                _model.Initialize(persons, stateHolder, SchedulingOptions);
            }
            ClearAndSetWarnings(false);
        }

        public void Reload(ICollection<IPerson> restrictionPersonsToReload)
        {
            _model.Reload(restrictionPersonsToReload);
            ClearAndSetWarnings(true);
        }

        private void ClearAndSetWarnings(bool keepSelection)
        {
            //Clear grid from comments
            _grid.RowCount = 0;
            _grid.RowCount = _model.PersonsAffectedPeriodDates.Count + 1;

            WriteWarnings(keepSelection);
        }

        public void WriteWarnings(bool keepSelection)
        {
            for (int i = 2; i <= _grid.RowCount; i++)
            {
                SetWarnings(i);
            }
            if (_grid.RowCount > 1)
            {
                if (!keepSelection)
                    _grid.KeepSelection(false);
                else
                    _grid.KeepSelection(true);
                _grid.Invalidate();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.Int32.ToString")]
        public object OnQueryCellInfo(int rowIndex, int colIndex)
        {
            if (_model.PersonsAffectedPeriodDates.Count == 0 && rowIndex > 0)
                return string.Empty;
            if (rowIndex == 0 && colIndex == 2)
                return Resources.SchedulePeriod;
            if (rowIndex == 0 && colIndex == 7)
                return Resources.Schedule;
            if (rowIndex == 0 && colIndex == 9)
                return Resources.SchedulePlusRestrictions;
            if (rowIndex == 1 && colIndex >= 0)
                return _headers[colIndex];
            if (rowIndex > 1 && colIndex >= 0)
            {
                if (colIndex == 0)
                    return _model.GetRowData(rowIndex - _numberOfHeaders).Person.Name.ToString();
                if (colIndex == 1)
                    return _model.GetRowData(rowIndex - _numberOfHeaders).NumberOfWarnings;
                if (colIndex == 2)
                    return LanguageResourceHelper.TranslateEnumValue(_model.GetRowData(rowIndex - _numberOfHeaders).SchedulePeriod.PeriodType);
                if (colIndex == 3)
                    return _model.GetRowData(rowIndex - _numberOfHeaders).Period.Value.StartDate.ToShortDateString();
                if (colIndex == 4)
                    return _model.GetRowData(rowIndex - _numberOfHeaders).Period.Value.EndDate.ToShortDateString();
                if (colIndex == 5)
                    return _model.GetRowData(rowIndex - _numberOfHeaders).SchedulePeriodTargetTime;
                if (colIndex == 6)
                    return _model.GetRowData(rowIndex - _numberOfHeaders).SchedulePeriodTargetDaysOff;
                if (colIndex == 7)
                    return _model.GetRowData(rowIndex - _numberOfHeaders).CurrentContractTime;
                if (colIndex == 8)
                    return _model.GetRowData(rowIndex - _numberOfHeaders).CurrentDaysOff;
                if (colIndex == 9)
                    return _model.GetRowData(rowIndex - _numberOfHeaders).PossiblePeriodTime.Minimum;
                if (colIndex == 10)
                    return _model.GetRowData(rowIndex - _numberOfHeaders).PossiblePeriodTime.Maximum;
                if (colIndex == 11)
                    return _model.TotalNumberOfDaysOff(rowIndex - _numberOfHeaders);
                if (colIndex == 12)
                {
                    if (_model.GetRowData(rowIndex - _numberOfHeaders).NumberOfWarnings == 0)
                        return Resources.Yes;
                    return Resources.No;
                }
            }
            return string.Empty;
        }

        public void SetSelectedPersonDate(int rowIndex)
        {
            if ((rowIndex - _numberOfHeaders) < 0)
                throw new ArgumentException("Row index - number of headers must atleast equal to zero ");
            _model.SetSelectedPersonDate(rowIndex - _numberOfHeaders);
        }
        public static string OnQueryCellType(int colIndex)
        {
            string cellType;
            switch (colIndex)
            {
                case 0:
                    cellType = "Header";
                    break;
                case 1:
                case 6:
                case 8:
                case 11:
                    cellType = "NumericCell";
                    break;
                case 5:
                case 7:
                case 9:
                case 10:
                    cellType = "TimeSpan";
                    break;
                default:
                    cellType = "Static";
                    break;
            }
            return cellType;
        }

        public void SetWarnings(int rowIndex)
        {
            int numberOfWarnings = 0;
            AgentInfoHelper agentInfoHelper = _model.GetRowData(rowIndex - _numberOfHeaders);
            if (!agentInfoHelper.SchedulePeriodTargetMinMax.ContainsPart(agentInfoHelper.CurrentContractTime))
            {
                _grid.TipText(rowIndex, 7, Resources.ContractTimeDoesNotMeetTheTargetTime);
                numberOfWarnings++;
            }
            if (agentInfoHelper.SchedulePeriodTargetDaysOff != agentInfoHelper.NumberOfDatesWithPreferenceOrScheduledDaysOff)
            {
                _grid.TipText(rowIndex, 11, Resources.WrongNumberOfDaysOff);
                numberOfWarnings++;
            }
            if (agentInfoHelper.MinPossiblePeriodTime > agentInfoHelper.SchedulePeriodTargetMinMax.EndTime)
            {
                _grid.TipText(rowIndex, 9, Resources.LowestPossibleWorkTimeIsTooHigh);
                numberOfWarnings++;
            }
            if (agentInfoHelper.MaxPossiblePeriodTime < agentInfoHelper.SchedulePeriodTargetMinMax.StartTime)
            {
                _grid.TipText(rowIndex, 10, Resources.HighestPossibleWorkTimeIsTooLow);
                numberOfWarnings++;
            }
            agentInfoHelper.NumberOfWarnings = numberOfWarnings;
        }

        public AgentInfoHelper SelectedAgentInfo()
        {
            int rowIndex = _grid.CurrentCellRowIndex;
            if (rowIndex > 1)
                return _model.GetRowData(rowIndex - _numberOfHeaders);
            return null;
        }

        public void SetSchedulingOptions(ISchedulingOptions schedulingOptions, bool keepSelection)
        {
            if (schedulingOptions != null)
                _schedulingOptions = schedulingOptions;
            _model.Initialize(_persons, _stateHolder, SchedulingOptions);
           
            ClearAndSetWarnings(keepSelection);
        }

        public void Sort(int colIndex, bool ascending)
        {
            _model.SortOnColumn(colIndex, ascending);
            int rowIndex = _model.IndexOf() + _grid.HeaderCount+1;
            _grid.Invalidate();
            _grid.SetSelections(rowIndex, false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _grid = null;
                _schedulingOptions = null;
                _stateHolder = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

				public void SetSelection(IScheduleDay schedulePart)
        {
            int rowIndex = _numberOfHeaders;

            for (int i = 0; i < _model.PersonsAffectedPeriodDates.Count; i++)
            {
                if (_model.PersonsAffectedPeriodDates[i].Key == schedulePart.Person) //&& _model.PersonsAffectedPeriodDates[i].Value.Date == kvp.Value)
                {
                    if(_model.GetRowData(i).Period.Value.Contains(schedulePart.Period.ToDateOnlyPeriod(schedulePart.TimeZone)))
                    {
                        rowIndex += i;
                        break;
                    }          
                }
            }
            _grid.SetSelections(rowIndex, true);
        }
    }
}