using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using Teleopti.Ccc.AgentPortal.Reports.Grid;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortal.Reports
{
	public class MyReportStateHolder
	{
		private IList<AdherenceInfoDto> _informationDto = new List<AdherenceInfoDto>();
		private IList<AgentQueueStatDetailsDto> _queueDto = new List<AgentQueueStatDetailsDto>();
		private DateTimePeriodDto _selectedDateTimePeriodDto;
		private readonly IList<MyScheduleGridAdapter> _myScheduleGridAdapterCollection = new List<MyScheduleGridAdapter>();
		private decimal? _dayAdherence;

		/// <summary>
		/// Gets the information cellection.
		/// </summary>
		/// <value>The information cellection.</value>
		/// <remarks>
		/// Created by: Madhuranga Pinnagoda
		/// Created date: 2008-10-13
		/// </remarks>
		public IList<AdherenceInfoDto> InformationCollection
		{
			get { return _informationDto; }
		}

		/// <summary>
		/// Gets the queue collection.
		/// </summary>
		/// <value>The queue collection.</value>
		/// <remarks>
		/// Created by: Madhuranga Pinnagoda
		/// Created date: 2008-10-13
		/// </remarks>
		public IList<AgentQueueStatDetailsDto> QueueCollection
		{
			get { return _queueDto; }
		}

		/// <summary>
		/// Gets or sets the selected date time period dto.
		/// </summary>
		/// <value>The selected date time period dto.</value>
		/// <remarks>
		/// Created by: Muhamad Risath
		/// Created date: 10/13/2008
		/// </remarks>
		public DateTimePeriodDto SelectedDateTimePeriodDto
		{
			get { return _selectedDateTimePeriodDto; }
			set { _selectedDateTimePeriodDto = value; }
		}

		/// <summary>
		/// Gets the visualize start time.
		/// </summary>
		/// <value>The visualize start time.</value>
		/// <remarks>
		/// Created by: Muhamad Risath
		/// Created date: 10/16/2008
		/// </remarks>
		public TimeSpan VisualizeStartTime { get; set; }

		/// <summary>
		/// Gets the visualize end time.
		/// </summary>
		/// <value>The visualize end time.</value>
		/// <remarks>
		/// Created by: Muhamad Risath
		/// Created date: 10/16/2008
		/// </remarks>
		public TimeSpan VisualizeEndTime { get; set; }

		/// <summary>
		/// Gets my schedule grid adapter collection.
		/// </summary>
		/// <value>My schedule grid adapter collection.</value>
		/// <remarks>
		/// Created by: Muhamad Risath
		/// Created date: 10/15/2008
		/// </remarks>
		public IList<MyScheduleGridAdapter> MyScheduleGridAdapterCollection
		{
			get { return _myScheduleGridAdapterCollection; }
		}

		/// <summary>
		/// Loads my report data.
		/// </summary>
		/// <remarks>
		/// Created by: Muhamad Risath
		/// Created date: 11/27/2008
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public void LoadMyReportData()
		{
			if (SdkServiceHelper.LogOnServiceClient == null) return;

			var startDate = DateTime.SpecifyKind(_selectedDateTimePeriodDto.LocalStartDateTime.Date, DateTimeKind.Unspecified);
			var endDate = DateTime.SpecifyKind(_selectedDateTimePeriodDto.LocalEndDateTime.Date, DateTimeKind.Unspecified);
			var startDateOnly = new DateOnlyDto { DateTime = startDate.AddDays(0), DateTimeSpecified = true };
			var endDateOnly = new DateOnlyDto { DateTime = endDate, DateTimeSpecified = true };
			PersonDto person = StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson;
			var query = new GetSchedulesByPersonQueryDto { StartDate = startDateOnly,EndDate = endDateOnly,PersonId = person.Id,TimeZoneId = person.TimeZoneId};
			IList<SchedulePartDto> schedulePartDtos = SdkServiceHelper.SchedulingService.GetSchedulesByQuery(query);

			_myScheduleGridAdapterCollection.Clear();
			//var allProjectedLayers = new List<ProjectedLayerDto>();
			//foreach (SchedulePartDto mySchedulePart in schedulePartDtos)
			//{
			//    allProjectedLayers.AddRange(mySchedulePart.ProjectedLayerCollection);
			//}
			//allProjectedLayers.Sort(delegate(ProjectedLayerDto l1, ProjectedLayerDto l2)
			//                            {
			//                                int result =
			//                                    l1.Period.LocalStartDateTime.CompareTo(
			//                                        l2.Period.LocalStartDateTime);
			//                                if (result != 0) return result;
			//                                return l1.Period.LocalEndDateTime.CompareTo(
			//                                    l2.Period.LocalEndDateTime);
			//                            });

			foreach (SchedulePartDto mySchedulePart in schedulePartDtos)
			{
				IList<ActivityVisualLayer> activityVisualLayers = new List<ActivityVisualLayer>();
				foreach (ProjectedLayerDto projectedLayerDto in mySchedulePart.ProjectedLayerCollection)
				{
					DateTime layerStart = projectedLayerDto.Period.LocalStartDateTime;
					DateTime layerEnd = projectedLayerDto.Period.LocalEndDateTime;

					int startHour = layerStart.Hour;
					int startMinute = layerStart.Minute;
					var endDay = 0;
					int endHour = layerEnd.Hour;
					int endMinute = layerEnd.Minute;
					
					if (endHour < startHour) 
						endDay = 1;
					
					var start = new TimeSpan(startHour, startMinute, 0);
					var end = new TimeSpan(endDay, endHour, endMinute, 0);
					var timePeriod = new TimePeriod(start, end);

					ActivityVisualLayer activityVisualLayer = GetMidnightActivityVisualLayer(projectedLayerDto, timePeriod);
					activityVisualLayers.Add(activityVisualLayer);
				}

				_dayAdherence = null;
				IList<AdherenceLayer> adherenceLayers = GetAdherenceLayers(person, mySchedulePart.Date.DateTime);
				
				var scheduleAdherence = new ScheduleAdherence(new VisualProjection(person, activityVisualLayers, string.Empty, false), adherenceLayers);
				if (activityVisualLayers.Count == 0) _dayAdherence = null;

				var myScheduleGridAdapter = new MyScheduleGridAdapter
				                            	{
				                            		MyScheduleAdherence = scheduleAdherence,
				                            		Adherence = (double?) _dayAdherence,
				                            		Date = mySchedulePart.Date.DateTime
				                            	};
				_myScheduleGridAdapterCollection.Add(myScheduleGridAdapter);
			}
			_informationDto = GetAgentAdherenceInfoStat(startDate);
			_queueDto = GetAgentQueueStatDetails(startDate);
		}

		public static IList<AgentQueueStatDetailsDto> GetAgentQueueStatDetails(DateTime fromDate)
		{
			PersonDto personDto = StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson;
			IList<AgentQueueStatDetailsDto> agentQueueStatDetailsDtos = SdkServiceHelper.SchedulingService.GetAgentQueueStatDetails(fromDate, true,
				fromDate.Date.AddDays(6), true,
				personDto.TimeZoneId,
				personDto);
			return agentQueueStatDetailsDtos;
		}
		public static IList<AdherenceInfoDto> GetAgentAdherenceInfoStat(DateTime fromDate)
		{
			PersonDto personDto = StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson;
			IList<AdherenceInfoDto> agentQueueStatDetailsDtos = SdkServiceHelper.SchedulingService.GetAdherenceInfo(fromDate, true,
				fromDate.Date.AddDays(6), true,
				personDto.TimeZoneId,
				personDto);
			return agentQueueStatDetailsDtos;
		}
		private IList<AdherenceLayer> GetAdherenceLayers(PersonDto personDto, DateTime dateTime)
		{
			CultureInfo c = GetCultureInfo(personDto);
			AdherenceDto adherenceDto = SdkServiceHelper.SchedulingService.GetAdherenceData(dateTime, true,
																							personDto.TimeZoneId, personDto, personDto, c.LCID, true);
			IList<AdherenceLayer> adherenceLayers = new List<AdherenceLayer>();
			foreach (AdherenceDataDto adherenceDataDto in adherenceDto.AdherenceDataDtos)
			{
				var adherenceLayer =
					new AdherenceLayer(
						new TimePeriod(TimeSpan.FromTicks(adherenceDataDto.LocalStartTime),
						               TimeSpan.FromTicks(adherenceDataDto.LocalEndTime)), (double) adherenceDataDto.ReadyTimeMinutes, 
									   adherenceDataDto.CalendarDate, adherenceDataDto.ShiftBelongsToDate);
				adherenceLayers.Add(adherenceLayer);
				if (adherenceDataDto.AdherenceForDay.HasValue)
					_dayAdherence = adherenceDataDto.AdherenceForDay * 100;
				else
					_dayAdherence = null;
			}
			return adherenceLayers;
		}

		private static CultureInfo GetCultureInfo(PersonDto personDto)
		{
			return personDto.UICultureLanguageId.HasValue
					   ? CultureInfo.GetCultureInfo(personDto.UICultureLanguageId.Value)
					   : CultureInfo.CurrentUICulture;
		}

		private static ActivityVisualLayer GetMidnightActivityVisualLayer(ProjectedLayerDto overMidnightDto, TimePeriod midnigthTimePeriod)
		{
			return new ActivityVisualLayer(midnigthTimePeriod,
										   Color.FromArgb(overMidnightDto.DisplayColor.Alpha,
														  overMidnightDto.DisplayColor.Red,
														  overMidnightDto.DisplayColor.Green,
														  overMidnightDto.DisplayColor.Blue),
										   overMidnightDto.Description);
		}
	}
}
