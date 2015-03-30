using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using System.Globalization;
using System.Collections.ObjectModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DBConverter
{
    //TODO: Move to DatabaseConversionTools and create testable code (when the weather is boring... :)).
    internal static class ForecastingTemplateHelper
    {
        public static DataSet LoadSkillTemplates(SqlConnection connection, global::Domain.Skill skill)
        {
            SqlCommand command = connection.CreateCommand();
            command.CommandText = "p_fc5_load_skill_standard_days";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@skill_id", skill.Id);

            DataSet skillDefaultTemplates = new DataSet();
            skillDefaultTemplates.Locale = CultureInfo.CurrentCulture;
            using (SqlDataAdapter da = new SqlDataAdapter(command))
            {
                da.Fill(skillDefaultTemplates);
            }
            return skillDefaultTemplates;
        }

        public static DataSet LoadWorkloadTemplates(SqlConnection connection, global::Domain.Forecast forecast)
        {
            SqlCommand command = connection.CreateCommand();
            command.CommandText = "p_fc5_load_forecast_standard_days";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@forecast_id", forecast.Id);

            DataSet workloadDefaultTemplates = new DataSet();
            workloadDefaultTemplates.Locale = CultureInfo.CurrentCulture;
            using (SqlDataAdapter da = new SqlDataAdapter(command))
            {
                da.Fill(workloadDefaultTemplates);
            }
            return workloadDefaultTemplates;
        }

        public static void FillSkillDataPeriodWithValues(DataTable skillDefaultTemplateTable,
            IDictionary<DayOfWeek, ISkillDayTemplate> templateList,
            int minPos, int maxPos, ForecastingDataPeriodValueType type)
        {
            string selectionFilter = "pos = {0}";

            for (int i = minPos; i <= maxPos; i++)
            {
                SkillPersonData skillPersonData;
                ServiceAgreement serviceAgreement;

                DataRow[] skillDefaultRows = skillDefaultTemplateTable.Select(
                    string.Format(CultureInfo.CurrentCulture, selectionFilter, i), "interval");
                DayOfWeek dayOfWeek = (DayOfWeek)((i == maxPos) ? 0 : (i - minPos) + 1);
                ReadOnlyCollection<ITemplateSkillDataPeriod> skillDataPeriodList = templateList[dayOfWeek].TemplateSkillDataPeriodCollection;
                foreach (DataRow item in skillDefaultRows)
                {
                    double value = Convert.ToDouble(item["value"], CultureInfo.CurrentCulture);
                    int interval = Convert.ToInt32(item["interval"], CultureInfo.CurrentCulture);

                    // To avoid crach on a Folksam database as intervals beyond 95 can occure
                    if (interval < skillDataPeriodList.Count)
                    {
                        ISkillData currentSkillDataPeriod = skillDataPeriodList[interval];
                        switch (type)
                        {
                            case ForecastingDataPeriodValueType.ServiceLevelPercent:
                                if (value == 0)
                                {
                                    currentSkillDataPeriod.ServiceLevelPercent =
                                        ServiceAgreement.DefaultValues().ServiceLevel.Percent;
                                }
                                else
                                {
                                    currentSkillDataPeriod.ServiceLevelPercent = new Percent(value / 100d);
                                }
                                break;
                            case ForecastingDataPeriodValueType.ServiceLevelSeconds:
                                if (value == 0)
                                {
                                    currentSkillDataPeriod.ServiceLevelSeconds =
                                        ServiceAgreement.DefaultValues().ServiceLevel.Seconds;
                                }
                                else
                                {
                                    currentSkillDataPeriod.ServiceLevelSeconds = value;   
                                }
                                break;
                            case ForecastingDataPeriodValueType.MinPersons:
                                skillPersonData = new SkillPersonData(
                                        (int)Math.Round(value, 0),
                                        0);
                                currentSkillDataPeriod.SkillPersonData = skillPersonData;
                                break;
                            case ForecastingDataPeriodValueType.MaxPersons:
                                if (value > currentSkillDataPeriod.SkillPersonData.MinimumPersons)
                                {
                                    skillPersonData = new SkillPersonData(
                                            currentSkillDataPeriod.SkillPersonData.MinimumPersons,
                                            (int)Math.Round(value, 0));
                                    currentSkillDataPeriod.SkillPersonData = skillPersonData;
                                }
                                break;
                            case ForecastingDataPeriodValueType.MinOccupancy:
                                serviceAgreement = new ServiceAgreement(
                                        currentSkillDataPeriod.ServiceAgreement.ServiceLevel,
                                            new Percent(value / 100d),
                                                currentSkillDataPeriod.ServiceAgreement.MaxOccupancy);

                                currentSkillDataPeriod.ServiceAgreement = serviceAgreement;
                                break;
                            case ForecastingDataPeriodValueType.MaxOccupancy:
                                serviceAgreement = new ServiceAgreement(
                                        currentSkillDataPeriod.ServiceAgreement.ServiceLevel,
                                            currentSkillDataPeriod.ServiceAgreement.MinOccupancy,
                                                new Percent(value / 100d));

                                currentSkillDataPeriod.ServiceAgreement = serviceAgreement;
                                break;
                        }
                    }
                }
            }
        }

        public static void FillTemplateTaskPeriodWithValues(DataTable workloadDefaultTemplateTable, IDictionary<DayOfWeek, IWorkloadDayTemplate> templateList, int minPos, int maxPos, ForecastingDataPeriodValueType type)
        {
            string selectionFilter = "pos = {0} and interval = {1}";

            for (int i = minPos; i <= maxPos; i++)
            {
                DayOfWeek dayOfWeek = (DayOfWeek)((i == maxPos) ? 0 : (i - minPos) + 1);
                IWorkloadDayTemplate dayTemplate = templateList[dayOfWeek];
                foreach (TemplateTaskPeriod taskPeriod in dayTemplate.TaskPeriodList)
                {
                    int interval = (int)(taskPeriod.Period.StartDateTime.Subtract(dayTemplate.CurrentDate.Date).TotalMinutes / dayTemplate.Workload.Skill.DefaultResolution);
                    DataRow[] workloadDefaultRow = workloadDefaultTemplateTable.Select(
                                    string.Format(CultureInfo.CurrentCulture, selectionFilter, i, interval));

                    foreach (DataRow item in workloadDefaultRow)
                    {
                        double value = Convert.ToDouble(item["value"], CultureInfo.CurrentCulture);
                        switch (type)
                        {
                            case ForecastingDataPeriodValueType.Calls:
                            taskPeriod.SetTasks(value);
                            break;
                        case ForecastingDataPeriodValueType.TalkTime:
                            if (value < 1) value = 1d;
                            taskPeriod.AverageTaskTime = TimeSpan.FromSeconds(value);
                            break;
                        case ForecastingDataPeriodValueType.AfterTalkTime:
                            if (value < 1) value = 1d;
                            taskPeriod.AverageAfterTaskTime = TimeSpan.FromSeconds(value);
                            break;
                    }
                }	 
	}
            }
        }

        public static IDictionary<DayOfWeek, ISkillDayTemplate> CreateDayOfWeekTemplates(ISkill owningSkill, int intervalLength)
        {
            IDictionary<DayOfWeek, ISkillDayTemplate> templateList = new Dictionary<DayOfWeek, ISkillDayTemplate>();

            foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof(DayOfWeek)))
            {
                ISkillDayTemplate template = (ISkillDayTemplate) owningSkill.GetTemplate(TemplateTarget.Skill, dayOfWeek);
                template.SetSkillDataPeriodCollection(CreateSkillDataPeriodList(owningSkill,intervalLength));
                templateList.Add(dayOfWeek, template);
            }

            return templateList;
        }

        public static IDictionary<DayOfWeek, IWorkloadDayTemplate> CreateDayOfWeekTemplates(IWorkload owningWorkload, IDictionary<DayOfWeek, IList<TimePeriod>> openHours)
        {
            IDictionary<DayOfWeek, IWorkloadDayTemplate> templateList = new Dictionary<DayOfWeek, IWorkloadDayTemplate>();

            foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof(DayOfWeek)))
            {
                WorkloadDayTemplate template = (WorkloadDayTemplate) owningWorkload.GetTemplate(TemplateTarget.Workload, dayOfWeek);
                template.ChangeOpenHours(openHours[dayOfWeek]);
                templateList.Add(dayOfWeek, template);
            }

            return templateList;
        }

        public static IList<ITemplateSkillDataPeriod> CreateSkillDataPeriodList(ISkill owningSkill, int intervalLength)
        {
            IList<ITemplateSkillDataPeriod> skillDataPeriodList = new List<ITemplateSkillDataPeriod>();
            DateTime baseDate = TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date, owningSkill.TimeZone);
            DateTime endDate = baseDate.AddDays(1);
            TimeSpan step = TimeSpan.FromMinutes(intervalLength);
            for (DateTime currentDateTime = baseDate; currentDateTime < endDate; currentDateTime = currentDateTime.Add(step))
            {
                ITemplateSkillDataPeriod skillDataPeriod =
                    new TemplateSkillDataPeriod(
                        ServiceAgreement.DefaultValues(),
                        new SkillPersonData(),
                        new DateTimePeriod(currentDateTime, currentDateTime.Add(step)));
                skillDataPeriodList.Add(skillDataPeriod);
            }

            return skillDataPeriodList;
        }

        public static IDictionary<int, IList<TimePeriod>> LoadOpenHours(SqlConnection connection, int resolution)
        {
            SqlCommand command = connection.CreateCommand();
            command.CommandText = "p_fc5_load_open_hours_improved";
            command.CommandType = CommandType.StoredProcedure;

            DataSet openHourData = new DataSet();
            openHourData.Locale = CultureInfo.CurrentCulture;
            using (SqlDataAdapter da = new SqlDataAdapter(command))
            {
                da.Fill(openHourData);
            }

            IList<TimePeriod> openHourList = new List<TimePeriod>();
            IDictionary<int, IList<TimePeriod>> openHourReturnList = new Dictionary<int, IList<TimePeriod>>();
            openHourReturnList.Add(-1, new List<TimePeriod>());

            int currentOpenHourId = -1;
            int lastOpenInterval = -1;
            int lastInterval = -1;

            DataTable openHourDataTable = openHourData.Tables[0];
            foreach (DataRow dr in openHourDataTable.Rows)
            {
                int ohId = (int)dr["id"];
                int interval = (int)dr["interval"];
                bool open = (bool)dr["open"];

                if (currentOpenHourId != ohId)
                {
                    if (lastOpenInterval > -1)
                    {
                        openHourList.Add(new TimePeriod(TimeSpan.FromMinutes(lastOpenInterval * resolution),
                                                         TimeSpan.FromMinutes((lastInterval + 1) * resolution)));
                    }

                    if (currentOpenHourId > 0)
                    {
                        openHourReturnList.Add(currentOpenHourId, openHourList);
                    }
                    currentOpenHourId = ohId;
                    lastOpenInterval = -1;
                    openHourList = new List<TimePeriod>();
                }

                if (lastOpenInterval == -1 && open)
                {
                    lastOpenInterval = interval;
                }

                if (lastOpenInterval > -1 && open == false)
                {
                    openHourList.Add(new TimePeriod(TimeSpan.FromMinutes(lastOpenInterval * resolution),
                                                         TimeSpan.FromMinutes((lastInterval + 1) * resolution)));
                    lastOpenInterval = -1;
                }

                lastInterval = interval;
            }
            if (lastOpenInterval > -1)
            {
                openHourList.Add(new TimePeriod(TimeSpan.FromMinutes(lastOpenInterval * resolution),
                                                         TimeSpan.FromMinutes((lastInterval + 1) * resolution)));
            }
            if (currentOpenHourId > 0)
            {
                openHourReturnList.Add(currentOpenHourId, openHourList);
            }

            return openHourReturnList;
        }

        public static IDictionary<DayOfWeek, IList<TimePeriod>> LoadDayOfWeekOpenHours(SqlConnection connection, global::Domain.Forecast forecast, IDictionary<int, IList<TimePeriod>> openHourLookup)
        {
            SqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT std_ohrs_mo, std_ohrs_tu, std_ohrs_we, std_ohrs_th, std_ohrs_fr, std_ohrs_sa, std_ohrs_su FROM t_fc5_forecasts WHERE (id=@forecast_id)";
            command.CommandType = CommandType.Text;
            command.Parameters.AddWithValue("@forecast_id", forecast.Id);

            IDictionary<DayOfWeek, IList<TimePeriod>> returnList = new Dictionary<DayOfWeek, IList<TimePeriod>>();
            using (SqlDataReader dataReader = command.ExecuteReader())
            {
                dataReader.Read();
                foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof(DayOfWeek)))
                {
                    int ordinal = dataReader.GetOrdinal("std_ohrs_" + dayOfWeek.ToString().Substring(0, 2));
                    if (dataReader.IsDBNull(ordinal))
                        returnList.Add(dayOfWeek, openHourLookup[-1]);
                    else
                        returnList.Add(dayOfWeek, openHourLookup[dataReader.GetInt32(ordinal)]);
                }
            }
            return returnList;
        }
    }

    public enum ForecastingDataPeriodValueType
    {
        ServiceLevelPercent,
        ServiceLevelSeconds,
        MinPersons,
        MaxPersons,
        MinOccupancy,
        MaxOccupancy,
        Calls,
        TalkTime,
        AfterTalkTime,
        OpenHours
    }
}
