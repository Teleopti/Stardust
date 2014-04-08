using System;
using System.Collections.Generic;
using System.Linq;
using Domain;
using Infrastructure;
using log4net;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.CollectionConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DBConverter.GroupConverter
{
    class CommonConverter : ModuleConverter
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CommonConverter));
        private readonly DefaultAggregateRoot _defaultAggregateRoot;
        private readonly string _connectionString;

        public CommonConverter(MappedObjectPair mappedObjectPair, 
                               DateTimePeriod period, 
                               TimeZoneInfo timeZoneInfo,
                               DefaultAggregateRoot defaultAggregateRoot, string connectionString)
            : base(mappedObjectPair, period, timeZoneInfo)
        {
            _defaultAggregateRoot = defaultAggregateRoot;
            _connectionString = connectionString;
        }

        protected override string ModuleName
        {
            get { return "General"; }
        }

        protected override IEnumerable<Type> DependedOn
        {
            get
            {
                return new List<Type>();
            }
        }

        protected override void GroupConvert()
        {
            ConvertDayoffs(); // Must be convert before even absences
            ConvertAbsences();  //Must be converted before activities otherwise absenceactivities will not work

            var oldActivities = LoadActivities();
            ConvertOvertimeActivitites(oldActivities); //Must be converted before activities to avoid having overtime activities as normal activities
            ConvertActivities(oldActivities); //Must be converted after absences
            ConvertScenarios();
            ICollection<Unit> unitList = ConvertSites();
            ConvertTeams(unitSubs(unitList));
            ConvertShiftCategories();
            ConvertRuleSetBags();
        }

        private void ConvertOvertimeActivitites(IDictionary<int, Activity> oldActivities)
        {
            Logger.Info("Converting Overtime Activities...");
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                OvertimeActivityConverter actConverter =
                    new OvertimeActivityConverter(uow, new OvertimeActivityMapper(MappedObjectPair, oldActivities));
                actConverter.ConvertAndPersist(new OvertimeReader().GetAll().Values);
            }
        }

        private void ConvertActivities(IDictionary<int, Activity> oldActivities)
        {
            Logger.Info("Converting Activities...");
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                var actConverter = new ActivityConverter(uow, new ActivityMapper(MappedObjectPair));
                actConverter.ConvertAndPersist(oldActivities.Values);
            }
        }

        private static IDictionary<int, Activity> LoadActivities()
        {
            return new ActivityReader().GetAll();
        }

        private void ConvertDayoffs()
        {
            Logger.Info("Converting Dayoffs...");
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                DayOffConverter dayOffConverter =
                    new DayOffConverter(uow, new DayOffMapper(MappedObjectPair));
                dayOffConverter.ConvertAndPersist(new AbsenceReader().GetAll().Values);
            }
        }

        private void ConvertAbsences()
        {
            Logger.Info("Converting Absences...");
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                var helper = new AbsenceConverterHelper(_connectionString);
                AbsenceConverter absConverter =
                    new AbsenceConverter(uow, new AbsenceMapper(MappedObjectPair, helper.ReadConfidentialAbsence()));
                absConverter.ConvertAndPersist(new AbsenceReader().GetAll().Values);
            }
        }

        private void ConvertScenarios()
        {
            Logger.Info("Converting Scenarios...");
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                ScenarioConverter scenConverter = new ScenarioConverter(uow, new ScenarioMapper(MappedObjectPair));
                scenConverter.ConvertAndPersist(new ScenarioReader().GetAll().Values);
            }
        }

        private void ConvertShiftCategories()
        {
            Logger.Info("Converting ShiftCaterories...");
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                ShiftCategoryConverter shiftCatConverter = new ShiftCategoryConverter(uow, new ShiftCategoryMapper(MappedObjectPair));
                shiftCatConverter.ConvertAndPersist(new ShiftCategoryReader().GetAll().Values);
            }
        }

        private void ConvertRuleSetBags()
        {
            Logger.Info("Converting RuleSetBags...");
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                RuleSetBagConverter ruleSetBagConverter = new RuleSetBagConverter(uow, new RuleSetBagMapper(MappedObjectPair, TimeZoneInfo), MappedObjectPair);
                WorkShiftRuleSetConverter workShiftRuleSetConverter = new WorkShiftRuleSetConverter(uow, new WorkShiftRuleSetMapper(MappedObjectPair, TimeZoneInfo));

                workShiftRuleSetConverter.ConvertAndPersist(new ShiftClassReader().GetAll().Values);
                MappedObjectPair.RuleSetBag = new ObjectPairCollection<UnitEmploymentType, IRuleSetBag>();
                ruleSetBagConverter.ConvertAndPersist(ruleSetBagConverter.CreateFakeOldEntities(new ShiftClassReader().GetAll().Values, workShiftRuleSetConverter));
            }
        }

        private ICollection<Unit> ConvertSites()
        {
            Logger.Info("Converting Sites...");

            ICollection<Unit> unitList = new ShiftUnitReader().GetAll().Values;

            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                SiteConverter siteConverter = new SiteConverter(uow, new SiteMapper(MappedObjectPair, TimeZoneInfo));
                siteConverter.ConvertAndPersist(unitList);
            }
            return unitList;
        }

        private void ConvertTeams(IEnumerable<UnitSub> units)
        {
            Logger.Info("Converting Teams...");
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                TeamConverter siteConverter = new TeamConverter(uow, new TeamMapper(MappedObjectPair));
                siteConverter.ConvertAndPersist(units);
            }
        }

       
        private static ICollection<UnitSub> unitSubs(IEnumerable<Unit> units)
        {
            Logger.Info("Getting unit subs...");
            //Done: linq!

            var subUnits = from u in units
                           from us in u.ChildUnits
                           select us;

            HashSet<UnitSub> retList = new HashSet<UnitSub>(subUnits);
            return retList;
        }

        

    }
}