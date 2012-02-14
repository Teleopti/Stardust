#region Imports

using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

#endregion

namespace Teleopti.Ccc.Sdk.WcfService.Factory
{
    /// <summary>
    /// Spllit the given period in to multiple based on the Agent's Team Schedule publishing date and 
    /// Preference date 
    /// </summary>
    /// <remarks>
    /// Created by: Sumedah
    /// Created date: 2009-01-25
    /// </remarks>
    internal static class SchedulePeriodFactory
    {
        /// <summary>
        /// create a list of period based on Agent's Team Schedule publishing date and
        /// Preference date
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="authorizationService">The authorization service.</param>
        /// <returns>list of periods</returns>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2009-01-25
        /// </remarks>
        internal static IList<DateTimePeriod> Create(DateTimePeriod period, IAuthorizationService authorizationService)
        {
            IList<DateTimePeriod> periods = new List<DateTimePeriod>();
            // can view all schedules
            if (authorizationService.IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules))
            {
                periods.Add(period);
            }
            else
            {
                IPerson loggedPerson = authorizationService.LoadedPerson;
                IPersonPeriod personPeriodForGivenDate = loggedPerson.Period(new DateOnly(DateTime.Now));

                if (personPeriodForGivenDate!=null && personPeriodForGivenDate.Team.SchedulePublishedToDate.HasValue)
                {
                    ITeam team;
                    using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                    {
                        TeamRepository teamRepository = new TeamRepository(uow);
                        team = teamRepository.Load(personPeriodForGivenDate.Team.Id.Value);
                    }
                    DateTime publishedDate = TimeZoneHelper.ConvertToUtc(team.SchedulePublishedToDate.Value);

                    DateTimePeriod newPeriod;
                    if (authorizationService.IsPermitted
                       (DefinedRaptorApplicationFunctionPaths.ModifyShiftCategoryPreferences))
                    {
                        if (personPeriodForGivenDate.Team.SchedulePreferenceDate.HasValue)
                        {
                            DateTime preferenceDate = TimeZoneHelper.ConvertToUtc(personPeriodForGivenDate.Team.SchedulePreferenceDate.Value);
                            DateTimePeriod preferncePeriod = new DateTimePeriod(
                                preferenceDate > publishedDate ? publishedDate : preferenceDate,
                                preferenceDate > publishedDate ? preferenceDate : publishedDate);

                            //requested period is end before the schedule publishing date/Preference Date
                            if (period.EndDateTime <= preferncePeriod.StartDateTime)
                            {
                                periods.Add(period);
                            }
                            //requested period is start after the schedule publishing date/Preference Date
                            else if (period.StartDateTime >= preferncePeriod.EndDateTime)
                            {
                                periods.Add(period);
                            }
                            // start date of requested period is faling between schedule publishing date 
                            // and Preference Date
                            else if (preferncePeriod.ContainsPart(period.StartDateTime))
                            {
                                if (period.EndDateTime > preferncePeriod.EndDateTime)
                                {
                                    newPeriod = new DateTimePeriod(preferncePeriod.EndDateTime,
                                        period.EndDateTime);
                                    periods.Add(newPeriod);
                                }
                            }
                            // end date of requested period is faling between schedule publishing date 
                            // and Preference Date
                            else if (preferncePeriod.ContainsPart(period.EndDateTime))
                            {
                                if (period.StartDateTime < preferncePeriod.StartDateTime)
                                {
                                    newPeriod = new DateTimePeriod(period.StartDateTime,
                                        preferncePeriod.StartDateTime);
                                    periods.Add(newPeriod);
                                }
                            }
                            // requested period is faling between schedule publishing date and Preference Date
                            else if ((period.StartDateTime < preferncePeriod.StartDateTime) &&
                                      (period.EndDateTime > preferncePeriod.EndDateTime))
                            {
                                newPeriod = new DateTimePeriod(period.StartDateTime,
                                        preferncePeriod.StartDateTime);
                                periods.Add(newPeriod);
                                newPeriod = new DateTimePeriod(preferncePeriod.EndDateTime,
                                        period.EndDateTime);
                                periods.Add(newPeriod);
                            }
                        }
                        else
                        {
                            if (period.EndDateTime <= publishedDate)
                            {
                                periods.Add(period);
                            }
                            else if (period.ContainsPart(publishedDate))
                            {
                                newPeriod = new DateTimePeriod(period.StartDateTime, publishedDate);
                                periods.Add(newPeriod);
                            }
                        }

                    }
                    else
                    {
                        if (period.EndDateTime <= publishedDate)
                        {
                            periods.Add(period);
                        }
                        else if (period.ContainsPart(publishedDate))
                        {
                            newPeriod = new DateTimePeriod(period.StartDateTime, publishedDate);
                            periods.Add(newPeriod);
                        }
                    }
                }
                else
                {
                    periods.Add(period);
                }
            }

            return periods;
        }
    }
}
