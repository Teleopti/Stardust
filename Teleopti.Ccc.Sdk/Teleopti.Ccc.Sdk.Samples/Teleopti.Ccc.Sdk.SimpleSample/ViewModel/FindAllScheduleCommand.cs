using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.SimpleSample.Repositories;

namespace Teleopti.Ccc.Sdk.SimpleSample.ViewModel
{
    public class FindAllScheduleCommand : ICommand
    {
        private readonly ScheduleViewModel _scheduleViewModel;
        private readonly PersonRepository _personRepository;
        private readonly ScheduleRepository _scheduleRepository;
        private readonly ActivityRepository _activityRepository;
        private readonly AbsenceRepository _absenceRepository;
        private readonly OvertimeDefinitionSetRepository _overtimeDefinitionSetRepository;

        public FindAllScheduleCommand(ScheduleViewModel scheduleViewModel, PersonRepository personRepository, ScheduleRepository scheduleRepository, ActivityRepository activityRepository, AbsenceRepository absenceRepository, OvertimeDefinitionSetRepository overtimeDefinitionSetRepository)
        {
            _scheduleViewModel = scheduleViewModel;
            _personRepository = personRepository;
            _scheduleRepository = scheduleRepository;
            _activityRepository = activityRepository;
            _absenceRepository = absenceRepository;
            _overtimeDefinitionSetRepository = overtimeDefinitionSetRepository;
            _scheduleViewModel.PropertyChanged += _scheduleViewModel_PropertyChanged;
        }

        private void _scheduleViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        	if (e.PropertyName != "SelectedGroupPageGroup") return;

        	var handler = CanExecuteChanged;
        	if (handler != null)
        	{
        		handler.Invoke(this, EventArgs.Empty);
        	}

			if (CanExecute(null))
			{
				Execute(null);
			}
        }

    	public void Execute(object parameter)
        {
            _personRepository.Initialize();
            _absenceRepository.Initialize();
            _activityRepository.Initialize();
            _overtimeDefinitionSetRepository.Initialize();

        	var result =
        		_scheduleRepository.GetScheduleModels(
        			new PeriodScheduleLoadOptions(new DateOnlyDto {DateTime = _scheduleViewModel.StartDate.Date}, _scheduleViewModel.SelectedGroupPageGroup.Id.GetValueOrDefault(),
        			                              TimeZoneInfo.Local));

            _scheduleViewModel.FoundSchedules.Clear();
            foreach (var scheduleModel in result)
            {
                scheduleModel.PersonName = _personRepository.GetById(scheduleModel.PersonId).Name;

                foreach (var scheduleDayDetailModel in scheduleModel.ScheduleDetails)
                {
                    var activity = _activityRepository.GetById(scheduleDayDetailModel.PayloadId);
                    if (activity!=null)
                    {
                        scheduleDayDetailModel.PayloadName = activity.Description;
                        scheduleDayDetailModel.PayloadPayrollCode = activity.PayrollCode;
                        scheduleDayDetailModel.InContractTime = activity.InContractTime;
                        scheduleDayDetailModel.InPaidTime = activity.InPaidTime;
                        scheduleDayDetailModel.InWorkTime = activity.InWorkTime;
                    }

                    var absence = _absenceRepository.GetById(scheduleDayDetailModel.PayloadId);
                    if (absence != null)
                    {
                        scheduleDayDetailModel.PayloadName = absence.Name;
                        scheduleDayDetailModel.PayloadPayrollCode = absence.PayrollCode;
                        scheduleDayDetailModel.InContractTime = absence.InContractTime;
                        scheduleDayDetailModel.InPaidTime = absence.InPaidTime;
                        scheduleDayDetailModel.InWorkTime = absence.InWorkTime;
                    }

                    if (scheduleDayDetailModel.OvertimeDefinitionSetId.HasValue)
                    {
                        var overtimeSet =
                            _overtimeDefinitionSetRepository.GetById(
                                scheduleDayDetailModel.OvertimeDefinitionSetId.Value);
                        if (overtimeSet != null)
                        {
                            scheduleDayDetailModel.OvertimeName = overtimeSet.Description;
                        }
                    }
                }
                
                _scheduleViewModel.FoundSchedules.Add(scheduleModel);
            }

            _scheduleViewModel.ResultCountVisible = Visibility.Visible;
        }

        public bool CanExecute(object parameter)
        {
            return _scheduleViewModel.SelectedGroupPageGroup!=null;
        }

        public event EventHandler CanExecuteChanged;
    }
}