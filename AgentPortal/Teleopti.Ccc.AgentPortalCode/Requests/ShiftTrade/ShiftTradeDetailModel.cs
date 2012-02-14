using System;
using System.Collections.Generic;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortalCode.Requests.ShiftTrade
{
    public class ShiftTradeDetailModel
    {
        private readonly ShiftTradeSwapDetailDto _shiftTradeSwapDetailDto;

        public ShiftTradeDetailModel(ShiftTradeSwapDetailDto shiftTradeSwapDetailDto, SchedulePartDto schedulePartDto, PersonDto personDto, PersonDto loggedOn)
        {
            _shiftTradeSwapDetailDto = shiftTradeSwapDetailDto;

            Person = personDto.Id == loggedOn.Id ? UserTexts.Resources.Me : personDto.Name;
            TradeDate = _shiftTradeSwapDetailDto.DateFrom.DateTime;
            VisualProjection = CreateVisualProjection(schedulePartDto, personDto);
			VisualProjectionContainsAbsence = CheckForAbsences(schedulePartDto);
        }

        private static VisualProjection CreateVisualProjection(SchedulePartDto schedulePartDto, PersonDto currentPerson)
        {
            string dayOffName = string.Empty;
            bool isDayOff = false;
            var activityVisualLayers = new List<ActivityVisualLayer>();
            if (schedulePartDto != null)
            {
                if (schedulePartDto.PersonDayOff != null)
                {
                    isDayOff = true;
                    dayOffName = schedulePartDto.PersonDayOff.Name;
                }
                foreach (ProjectedLayerDto projectedLayerDto in schedulePartDto.ProjectedLayerCollection)
                {
                    TimeSpan start = projectedLayerDto.Period.LocalStartDateTime.Subtract(schedulePartDto.Date.DateTime);
                    TimeSpan end = projectedLayerDto.Period.LocalEndDateTime.Subtract(schedulePartDto.Date.DateTime);
                    TimePeriod timePeriod = new TimePeriod(start, end);
                    ActivityVisualLayer activityVisualLayer = new ActivityVisualLayer(timePeriod,
                                                                                      ColorHelper.CreateColorFromDto(
                                                                                          projectedLayerDto.DisplayColor),
                                                                                      projectedLayerDto.Description);
                    activityVisualLayers.Add(activityVisualLayer);
                }
            }
            return new VisualProjection(currentPerson, activityVisualLayers, dayOffName, isDayOff);
        }

		private static bool CheckForAbsences(SchedulePartDto schedulePartDto)
		{
			if (schedulePartDto == null) return false;

			foreach (var projectedLayerDto in schedulePartDto.ProjectedLayerCollection)
			{
				if (projectedLayerDto.IsAbsence)
					return true;
			}

			return false;
		}

        public ShiftTradeSwapDetailDto ShiftTradeSwapDetailDto
        {
            get { return _shiftTradeSwapDetailDto; }
        }

        public DateTime TradeDate { get; private set; }

        public string Person { get; private set; }

        public VisualProjection VisualProjection { get; private set; }

		public bool VisualProjectionContainsAbsence { get; private set; }
    }
}