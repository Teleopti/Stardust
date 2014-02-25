using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Domain;
using Infrastructure;
using log4net;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.CollectionConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Skill=Domain.Skill;
using SkillDay=Domain.SkillDay;
using SkillType=Domain.SkillType;
using TimePeriod=Teleopti.Interfaces.Domain.TimePeriod;
using System.Linq;
using Queue=Domain.Queue;

namespace Teleopti.Ccc.DBConverter.GroupConverter
{
    internal class ForecastModuleConverter : ModuleConverter
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ForecastModuleConverter));
        private readonly int _intervalLength;
        private readonly string _connectionString;
        private int _newDefaultResolution;

        public ForecastModuleConverter(MappedObjectPair mappedObjectPair, DateTimePeriod period, TimeZoneInfo timeZoneInfo, string connectionString, int newDefaultResolution) : base(mappedObjectPair, period, timeZoneInfo)
        {
            _newDefaultResolution = newDefaultResolution;
            _intervalLength = new SystemSettingReader().GetSystemSetting.IntervalLength();
            if (_newDefaultResolution != 0)
            {
                checkIfNewDefaultResolutionIsValid(_newDefaultResolution, _intervalLength);
            }
            _connectionString = connectionString;
        }

        /// <summary>
        /// Checks if new default resolution is valid.
        /// </summary>
        /// <param name="newDefaultResolution">The new default resolution.</param>
        /// <param name="oldDefaultResolution">The old default resolution.</param>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2009-01-16
        /// </remarks>
        private static void checkIfNewDefaultResolutionIsValid(int newDefaultResolution, int oldDefaultResolution)
        {
            if (newDefaultResolution <= oldDefaultResolution)
            {
                throw new ArgumentException(
                    "New Default Resolution (" + 
                    newDefaultResolution.ToString(CultureInfo.CurrentCulture) + 
                    ") cannot be less than or equal as the old Default Resolution(" + 
                    oldDefaultResolution.ToString(CultureInfo.CurrentCulture) + ")"
                );
            }

            if (newDefaultResolution > (24 * 60))
            {
                throw new ArgumentException(
                    "New Default Resolution (" +
                    newDefaultResolution.ToString(CultureInfo.CurrentCulture) +
                    ") cannot be larger than 1200 minutes."
                );
            }

            int remainder;
            Math.DivRem(newDefaultResolution, oldDefaultResolution, out remainder);
            if (remainder != 0)
            {
                throw new ArgumentException(
                    "The new Default Resolution (" +
                    newDefaultResolution.ToString(CultureInfo.CurrentCulture) +
                    ") is not evenly divideable with the old Default Resolution (" +
                    oldDefaultResolution.ToString(CultureInfo.CurrentCulture) + ")"
                );
            }


        }

        protected override string ModuleName
        {
            get { return "Forecast"; }
        }

        protected override void GroupConvert()
        {
            ConvertQueueSources();
            ConvertQueueSourceNames();
            ConvertSkillTypes();
            ConvertSkills();
            ConvertSkillTemplates();
            ConvertWorkloads();
            ConvertWorkloadTemplates();
            ConvertSkillDays();
            MergeToNewDefaultResolution();
        }

        /// <summary>
        /// Merges to new default resolution.
        /// - Changes Skills Default Resolution.
        /// - Merges Skills templates.
        /// - Aligns WorkloadDayTemplates Open Hour list.
        /// </summary>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2009-01-23
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void MergeToNewDefaultResolution()
        {
            if (_newDefaultResolution != 0)
            {
                int step = _newDefaultResolution/_intervalLength;
                Logger.InfoFormat("Merging to new Default Resolution ({0})...", _newDefaultResolution);
                IList<IScenario> scenarios;
                IList<ISkill> skills;
                using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    IScenarioRepository scenarioRepository = new ScenarioRepository(uow);
                    scenarios = scenarioRepository.FindAllSorted();

                    ISkillRepository skillRepository = new SkillRepository(uow);
                    skills = (IList<ISkill>) skillRepository.FindAllWithWorkloadAndQueues();
                }


                Logger.Info(".Merging Skill- and WorkloadDays...");
                IEnumerable<DateTimePeriod> splittedDateTimePeriods = DateHelper.SplitDateTimePeriod(Period,
                                                                                                     TimeSpan.FromDays(
                                                                                                         90));
                foreach (IScenario scenario in scenarios)
                {
                    Logger.Info("..Merging scenario: " + scenario.Description);
                    foreach (ISkill skill in skills)
                    {
                        //if (skill.Name != "Veibeskrivelse") { continue; }
                        foreach (DateTimePeriod partOfDateTimePeriod in splittedDateTimePeriods)
                        {
                            Logger.InfoFormat("...Merging skilldays for {0} in period {1}", skill.Name,
                                              partOfDateTimePeriod.ToLocalString());
                            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                            {
                                uow.Reassociate(skill);
                                uow.Reassociate(skill.WorkloadCollection);
                                ISkillDayRepository skillDayRepository = new SkillDayRepository(uow);
                                ICollection<ISkillDay> skillDays = skillDayRepository.FindRange(
                                    partOfDateTimePeriod.ToDateOnlyPeriod(skill.TimeZone), skill, scenario);

                                foreach (ISkillDay skillDay in skillDays)
                                {
                                    foreach (IWorkloadDay workloadDay in skillDay.WorkloadDayCollection)
                                    {
                                        workloadDay.ChangeOpenHours(alignOpenHourList(workloadDay.OpenHourList));
                                        workloadDay.SetTaskPeriodCollection(mergeTemplateTaskPeriods(workloadDay, step));
                                    }
                                    IList<ISkillDataPeriod> newSkillDataPeriods = mergeSkillDataPeriods(skillDay, step);
                                    skillDay.SetNewSkillDataPeriodCollection(newSkillDataPeriods);

                                    uow.Merge(skillDay);
                                }
                                Logger.Info("...Persisiting Period: " + partOfDateTimePeriod.ToLocalString());
                                uow.PersistAll();
                            }
                        }
                    }
                }

                // Iterate skills graph.
                using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    Logger.Info(".Merging Skill- and WorkloadDayTemplates...");
                    ISkillRepository skillRepository = new SkillRepository(uow);
                    skills = (IList<ISkill>) skillRepository.FindAllWithWorkloadAndQueues();
                    foreach (ISkill skill in skills)
                    {
                        foreach (IWorkload workload in skill.WorkloadCollection)
                        {
                            foreach (
                                KeyValuePair<int, IWorkloadDayTemplate> workloadDayValuePair in workload.TemplateWeekCollection)
                            {
                                IWorkloadDayTemplate workloadDayTemplate = workloadDayValuePair.Value;
                                workloadDayTemplate.ChangeOpenHours(alignOpenHourList(workloadDayTemplate.OpenHourList));
                                workloadDayTemplate.SetTaskPeriodCollection(mergeTemplateTaskPeriods(
                                                                                workloadDayTemplate, step));
                            }
                        }

                        foreach (KeyValuePair<int, ISkillDayTemplate> skillDayValuePair in skill.TemplateWeekCollection)
                        {
                            ISkillDayTemplate skillDayTemplate = skillDayValuePair.Value;
                            IList<ITemplateSkillDataPeriod> newTemplateSkillDataPeriods =
                                mergeTemplateSkillDataPeriods(skillDayTemplate, step);
                            skillDayTemplate.SetSkillDataPeriodCollection(newTemplateSkillDataPeriods);
                        }

                        skill.DefaultResolution = _newDefaultResolution;
                        uow.PersistAll();
                    }
                }
            }
        }

        /// <summary>
        /// Merges the template task periods.
        /// </summary>
        /// <param name="workloadDayBase">The workload day base.</param>
        /// <param name="step">The step.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2009-01-25
        /// </remarks>
        private static IList<ITemplateTaskPeriod> mergeTemplateTaskPeriods(IWorkloadDayBase workloadDayBase, int step)
        {
            ReadOnlyCollection<ITemplateTaskPeriod> sortedTaskPeriodList = workloadDayBase.SortedTaskPeriodList;
            IList<ITemplateTaskPeriod> newTemplateTaskPeriods = new List<ITemplateTaskPeriod>();
            for (int i = 0; i < sortedTaskPeriodList.Count; i += step)
            {
                IList<ITemplateTaskPeriod> templateTaskPeriodsToBeMerged = new List<ITemplateTaskPeriod>();
                for (int j = i; j < (i + step); j++)
                {
                    templateTaskPeriodsToBeMerged.Add(sortedTaskPeriodList[j]);
                }
                ITemplateTaskPeriod newTemplateTaskPeriod =
                    TemplateTaskPeriod.Merge(templateTaskPeriodsToBeMerged);
                newTemplateTaskPeriods.Add(newTemplateTaskPeriod);
            }
            return newTemplateTaskPeriods;
        }

        /// <summary>
        /// Merges the template skill data periods.
        /// </summary>
        /// <param name="skillDayTemplate">The skill day template.</param>
        /// <param name="step">The step.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2009-01-25
        /// </remarks>
        private static IList<ITemplateSkillDataPeriod> mergeTemplateSkillDataPeriods(ISkillDayTemplate skillDayTemplate, int step)
        {
            IList<ITemplateSkillDataPeriod> sortedTemplateSkillDataPeriods = skillDayTemplate.TemplateSkillDataPeriodCollection.OrderBy(s => s.Period.StartDateTime).ToList();
            IList<ITemplateSkillDataPeriod> newTemplateSkillDataPeriods = createNewTemplateSkillDataPeriodsWithMerge(sortedTemplateSkillDataPeriods, step, skillDayTemplate);
            return newTemplateSkillDataPeriods;
        }

        private static IList<ISkillDataPeriod> mergeSkillDataPeriods(ISkillDay skillDay, int step)
        {
            IList<ISkillDataPeriod> sortedSkillDataPeriods = skillDay.SkillDataPeriodCollection.OrderBy(s => s.Period.StartDateTime).ToList();
            IList<ISkillDataPeriod> newTemplateSkillDataPeriods = createNewTemplateSkillDataPeriodsWithMerge(sortedSkillDataPeriods, step, skillDay);
            return newTemplateSkillDataPeriods;
        }

        /// <summary>
        /// Creates the new template skill data periods with merge.
        /// </summary>
        /// <param name="sortedSkillDataPeriods">The sorted template skill data periods.</param>
        /// <param name="step">The step.</param>
        /// <param name="parent">The parent.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2009-01-25
        /// </remarks>
        private static IList<ISkillDataPeriod> createNewTemplateSkillDataPeriodsWithMerge(IList<ISkillDataPeriod> sortedSkillDataPeriods, int step, IEntity parent)
        {
            IList<ISkillDataPeriod> newSkillDataPeriods = new List<ISkillDataPeriod>();
            for (int i = 0; i < sortedSkillDataPeriods.Count; i += step)
            {
                IList<ISkillDataPeriod> toBeMergedTemplateSkillDataPeriods  = new List<ISkillDataPeriod>();
                for (int j = i; j < (i + step); j++)
                {
                    toBeMergedTemplateSkillDataPeriods.Add(sortedSkillDataPeriods[j]);
                }
                ISkillDataPeriod newTemplateSkillDataPeriod = SkillDataPeriod.Merge(toBeMergedTemplateSkillDataPeriods, parent);
                newSkillDataPeriods.Add(newTemplateSkillDataPeriod);
            }
            return newSkillDataPeriods;
        }

        private static IList<ITemplateSkillDataPeriod> createNewTemplateSkillDataPeriodsWithMerge(IList<ITemplateSkillDataPeriod> sortedSkillDataPeriods, int step, IEntity parent)
        {
            IList<ITemplateSkillDataPeriod> newSkillDataPeriods = new List<ITemplateSkillDataPeriod>();
            for (int i = 0; i < sortedSkillDataPeriods.Count; i += step)
            {
                IList<ITemplateSkillDataPeriod> toBeMergedTemplateSkillDataPeriods = new List<ITemplateSkillDataPeriod>();
                for (int j = i; j < (i + step); j++)
                {
                    toBeMergedTemplateSkillDataPeriods.Add(sortedSkillDataPeriods[j]);
                }
                ITemplateSkillDataPeriod newTemplateSkillDataPeriod = TemplateSkillDataPeriod.Merge(toBeMergedTemplateSkillDataPeriods, parent);
                newSkillDataPeriods.Add(newTemplateSkillDataPeriod);
            }
            return newSkillDataPeriods;
        }


        /// <summary>
        /// Aligns the open hour list.
        /// </summary>
        /// <param name="timePeriods">The time periods.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2009-01-23
        /// </remarks>
        private IList<TimePeriod> alignOpenHourList(IList<TimePeriod> timePeriods)
        {
            IList<TimePeriod> newTimePeriods = new List<TimePeriod>();
            foreach (TimePeriod timePeriod in timePeriods)
            {
                TimePeriod newTimePeriod = TimePeriod.AlignToMinutes(timePeriod, _newDefaultResolution);
                newTimePeriods.Add(newTimePeriod);
            }
            return newTimePeriods;
            
        }


        protected override IEnumerable<Type> DependedOn
        {
            get
            {
                return new List<Type>();
            }
        }

        private void ConvertQueueSources()
        {
            Logger.Info("Converting QueueSources...");
            
            ICollection<int> intList = new HashSet<int>();

            foreach (IntegerPair pair in new ForecastQueueMapReader().GetAll().Values)
            {
                intList.Add(pair.IntegerValue2);
            }

            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                QueueSourceConverter queueSourceConverter = new QueueSourceConverter(uow, new QueueSourceMapper(MappedObjectPair, TimeZoneInfo));
                List<int> allIds = queueSourceConverter.Repository.LoadAll().Select(q => q.QueueAggId).ToList();

                //Remove all "new" ids that are allready in the db
                foreach (int item in allIds)
                {
                    intList.Remove(item);
                }
                queueSourceConverter.ConvertAndPersist(intList);
                updateQueueSourceMap(queueSourceConverter);
            }
        }

        //This is a superspecial case when queues are spread between different BU's
        //So we need to fetch the queues again from the db, there can be queues 
        //created from earlier converted BU's
        private void updateQueueSourceMap(QueueSourceConverter queueSourceConverter)
        {
            IList<IQueueSource> newQueueSourceMap = queueSourceConverter.Repository.LoadAll();
            var newObjectPairList = new ObjectPairCollection<int, IQueueSource>();
            foreach (IQueueSource queueSource in newQueueSourceMap)
            {
                newObjectPairList.Add(queueSource.QueueAggId,queueSource);
            }
            MappedObjectPair.QueueSource = newObjectPairList;
        }

        /// <summary>
        /// Converts the queue source names.
        /// </summary>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2009-01-14
        /// </remarks>
        public static void ConvertQueueSourceNames()
        {
            try
            {
                Logger.Info("Converting QueueSource Names...");
                using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    QueueSourceRepository queueSourceRepository = new QueueSourceRepository(uow);
                    IList<IQueueSource> queueSources = queueSourceRepository.LoadAllQueues();
                    ForecastQueueMapReader forecastQueueMapReader = new ForecastQueueMapReader();
                    IDictionary<int, Queue> queues = forecastQueueMapReader.LoadQueues();
                    Queue queue;
                    foreach (IQueueSource queueSource in queueSources)
                    {
                        if (queues.TryGetValue(queueSource.QueueAggId, out queue))
                        {
                            queueSource.Description = queue.Description;
                        }
                    }
                    uow.PersistAll();
                }
            }
            catch (SqlException se)
            {
                if (se.Number == 208)
                {
                    Logger.Warn("Could not convert QueueSource Names, most probably because p_fc5_load_queues has invalid Agg DB qualifier.",se);
                }
                else
                {
                    throw;
                }
            }
        }

        private void ConvertWorkloads()
        {
            foreach (Skill skill in MappedObjectPair.Skill.Obj1Collection())
            {
                Logger.InfoFormat("Converting Workloads in for skill: {0}...", skill.Name);
                using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    WorkloadConverter workloadConverter = new WorkloadConverter(uow, 
                                                                                new WorkloadMapper(MappedObjectPair, 
                                                                                                   MappedObjectPair.Skill.GetPaired(skill), 
                                                                                                   new ForecastQueueMapReader().GetAll()));
                    workloadConverter.ConvertAndPersist(skill.ForecastCollection);
                }
            }
        }

        private void ConvertSkillDays()
        {
            foreach (Scenario scenario in new ScenarioReader().GetAll().Values)
            {
                SkillDayReader skillDayReader = new SkillDayReader(scenario);
                foreach (Skill skill in MappedObjectPair.Skill.Obj1Collection())
                {
                    //if (skill.Name != "Veibeskrivelse") { continue; }
                    if (!skill.Deleted)
                    {
                        convertSkillDaysForSkill(scenario, skill, skillDayReader);
                    }
                }
            }
        }

        private void convertSkillDaysForSkill(Scenario scenario, Skill skill, ISkillDayReader skillDayReader)
        {
            IDictionary<IntegerDateKey, SkillDay> skillDayDict;
						DateTime currentDateTime = Period.LocalStartDateTime(TimeZoneInfo);
            const int daysToAdd = 10;
						while (currentDateTime < Period.LocalEndDateTime(TimeZoneInfo))
            {
                DatePeriod currentPeriod = new DatePeriod(currentDateTime, calcEndTime(currentDateTime, daysToAdd));
                Logger.InfoFormat("Converting skill days in {0} for skill {1} {2}", scenario.Name, skill.Name, currentPeriod);
                using (PerformanceOutput.ForOperation("Loading ccc-skilldays"))
                {
                    skillDayDict = skillDayReader.LoadSkillDays(skill, currentPeriod);
                }
                foreach (IntegerDateKey key in skillDayDict.Keys)
                {
                    using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                    {
                        SkillDayConverter skillDayConverter = new SkillDayConverter(uow, new SkillDayMapper(MappedObjectPair, TimeZoneInfo, _intervalLength));
                        skillDayConverter.ConvertAndPersist(new List<SkillDay> { skillDayDict[key] });
                    }
                }
                currentDateTime = currentDateTime.AddDays(daysToAdd);
            }
        }


        private void ConvertSkillTypes()
        {
            Logger.Info("Converting SkillTypes...");
            //Skilltypes
            //These skilltypes are system set in CCC 6.5 they are mapped
            //databse <-> Enum global::Domain.SkillType
            long numberOfSkillTypes;
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                SkillTypeRepository skillTypeRep = new SkillTypeRepository(uow);
                numberOfSkillTypes = skillTypeRep.CountAllEntities();
            }

            ObjectPairCollection<SkillType, ISkillType> skillTypeDic = new ObjectPairCollection<SkillType, ISkillType>();
            
            //If there is zero skillTypes in the db we're running the converter in old style(not with dbmanager)
            //When using the dbmanager the Skilltypes will allready be in the database as default data
            if (numberOfSkillTypes == 0)
            {
                ISkillType skillType1 = new SkillTypePhone(new Description("SkillTypeInboundTelephony"), ForecastSource.InboundTelephony);
                ISkillType skillType2 = new SkillTypeEmail(new Description("SkillTypeEmail"), ForecastSource.Email);
                ISkillType skillType3 = new SkillTypeEmail(new Description("SkillTypeFax"), ForecastSource.Facsimile);
                ISkillType skillType4 = new SkillTypeEmail(new Description("SkillTypeBackoffice"), ForecastSource.Backoffice);
                ISkillType skillType5 = new SkillTypeEmail(new Description("SkillTypeProject"), ForecastSource.Time);
                ISkillType skillType6 = new SkillTypeEmail(new Description("SkillTypeTime"), ForecastSource.Time);

                skillTypeDic.Add(SkillType.InboundTelephony, skillType1);
                skillTypeDic.Add(SkillType.email, skillType2);
                skillTypeDic.Add(SkillType.fax, skillType3);
                skillTypeDic.Add(SkillType.backoffice, skillType4);
                skillTypeDic.Add(SkillType.project, skillType5);
                skillTypeDic.Add(SkillType.time, skillType6);

                using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    IRepository<ISkillType> skillTypeRep = new SkillTypeRepository(uow);
                    foreach (ObjectPair<SkillType, ISkillType> pair in skillTypeDic)
                    {
                        skillTypeRep.Add(pair.Obj2);
                    }
                    uow.PersistAll();
                }
            }
            else
            {
                //when using dbmanager we need to fetch the allready created Skilltypes from the db
                using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    SkillTypeRepository skillTypeRep = new SkillTypeRepository(uow);
                    ICollection<ISkillType> skillTypes = skillTypeRep.FindAll();
                    foreach (ISkillType skillType in skillTypes)
                    {
                        switch (skillType.Description.Name)
                        {
                            case "SkillTypeInboundTelephony":
                                skillTypeDic.Add(SkillType.InboundTelephony, skillType);
                                break;
                            case "SkillTypeEmail":
                                skillTypeDic.Add(SkillType.email, skillType);
                                break;
                            case "SkillTypeFax":
                                skillTypeDic.Add(SkillType.fax, skillType);
                                break;
                            case "SkillTypeBackoffice":
                                skillTypeDic.Add(SkillType.backoffice, skillType);
                                break;
                            case "SkillTypeProject":
                                skillTypeDic.Add(SkillType.project, skillType);
                                break;
                            case "SkillTypeTime":
                                skillTypeDic.Add(SkillType.time, skillType);
                                break;
                        }
                    }
                }
            }
            MappedObjectPair.SkillType = skillTypeDic;
        }

        /// <summary>
        /// Converts the Skills
        /// </summary>
        private void ConvertSkills()
        {
            Logger.Info("Converting Skills...");
            ICollection<Skill> oldSkills = new SkillReader().GetAll().Values;
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                SkillConverter skillConverter = new SkillConverter(uow, new SkillMapper(MappedObjectPair, TimeZoneInfo, _intervalLength));
                skillConverter.ConvertAndPersist(oldSkills);
            }
        }

        private void ConvertSkillTemplates()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                foreach (ObjectPair<Skill, ISkill> skillPair in MappedObjectPair.Skill)
                {
                    IDeleteTag dTag = skillPair.Obj2 as IDeleteTag;
                    if (dTag==null || !dTag.IsDeleted)
                    {
                        Logger.InfoFormat("Converting templates for skill {0}", skillPair.Obj1.Name);

                        DataSet skillDefaultTemplates = ForecastingTemplateHelper.LoadSkillTemplates(connection, skillPair.Obj1);

                        IDictionary<DayOfWeek, ISkillDayTemplate> templateList = ForecastingTemplateHelper.CreateDayOfWeekTemplates(skillPair.Obj2, _intervalLength);
                        DataTable skillDefaultTemplateTable = skillDefaultTemplates.Tables[0];

                        //Goal percent 1 - 7
                        ForecastingTemplateHelper.FillSkillDataPeriodWithValues(skillDefaultTemplateTable, templateList, 1, 7, ForecastingDataPeriodValueType.ServiceLevelPercent);

                        //Goal sec 8 - 14
                        ForecastingTemplateHelper.FillSkillDataPeriodWithValues(skillDefaultTemplateTable, templateList, 8, 14, ForecastingDataPeriodValueType.ServiceLevelSeconds);

                        //Min log 15 - 21
                        ForecastingTemplateHelper.FillSkillDataPeriodWithValues(skillDefaultTemplateTable, templateList, 15, 21, ForecastingDataPeriodValueType.MinPersons);

                        //Max log 22 - 28
                        ForecastingTemplateHelper.FillSkillDataPeriodWithValues(skillDefaultTemplateTable, templateList, 22, 28, ForecastingDataPeriodValueType.MaxPersons);

                        //Max occ 50 - 56
                        ForecastingTemplateHelper.FillSkillDataPeriodWithValues(skillDefaultTemplateTable, templateList, 50, 56, ForecastingDataPeriodValueType.MaxOccupancy);

                        //Min occ 43 - 49
                        ForecastingTemplateHelper.FillSkillDataPeriodWithValues(skillDefaultTemplateTable, templateList, 43, 49, ForecastingDataPeriodValueType.MinOccupancy);

                        using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                        {
                            SkillRepository skillRep = new SkillRepository(uow);
                            skillRep.Add(skillPair.Obj2);
                            uow.PersistAll();
                        }
                    }
                    
                }
            }
        }

        private void ConvertWorkloadTemplates()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                IDictionary<int, IList<TimePeriod>> openHourLookup = ForecastingTemplateHelper.LoadOpenHours(connection, _intervalLength);
                foreach (ObjectPair<Forecast, IWorkload> workloadPair in MappedObjectPair.Workload)
                {
                    Logger.InfoFormat("Converting templates for workload {0}", workloadPair.Obj1.Name);

                    IDictionary<DayOfWeek, IList<TimePeriod>> openHoursDayOfWeek = ForecastingTemplateHelper.LoadDayOfWeekOpenHours(connection, workloadPair.Obj1, openHourLookup);
                    DataSet workloadDefaultTemplates = ForecastingTemplateHelper.LoadWorkloadTemplates(connection, workloadPair.Obj1);

                    IDictionary<DayOfWeek, IWorkloadDayTemplate> templateList = ForecastingTemplateHelper.CreateDayOfWeekTemplates(workloadPair.Obj2, openHoursDayOfWeek);
                    DataTable workloadDefaultTemplateTable = workloadDefaultTemplates.Tables[0];

                    //Tasks 1 - 7
                    ForecastingTemplateHelper.FillTemplateTaskPeriodWithValues(workloadDefaultTemplateTable, templateList, 1, 7, ForecastingDataPeriodValueType.Calls);

                    //Talk time 8 - 14
                    ForecastingTemplateHelper.FillTemplateTaskPeriodWithValues(workloadDefaultTemplateTable, templateList, 8, 14, ForecastingDataPeriodValueType.TalkTime);

                    //After talk time 15 - 21
                    ForecastingTemplateHelper.FillTemplateTaskPeriodWithValues(workloadDefaultTemplateTable, templateList, 15, 21, ForecastingDataPeriodValueType.AfterTalkTime);

                    using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                    {
                        WorkloadRepository workloadRep = new WorkloadRepository(uow);
                        workloadRep.Add(workloadPair.Obj2);
                        uow.PersistAll();
                    }
                }
            }
        }


        private DateTime calcEndTime(DateTime currentDateTime, int daysToAdd)
        {
            if (currentDateTime.AddDays(daysToAdd - 1) > Period.EndDateTime)
                return Period.EndDateTime;

            return currentDateTime.AddDays(daysToAdd - 1);
        }
    }
}
