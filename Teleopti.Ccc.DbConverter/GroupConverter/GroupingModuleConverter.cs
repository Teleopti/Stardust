using System;
using System.Collections.Generic;
using Infrastructure;
using log4net;
using Teleopti.Ccc.DatabaseConverter.CollectionConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DBConverter.GroupConverter
{
    /// <summary>
    /// Converts module Grouping
    /// </summary>
    /// <remarks>
    /// Created by: Sachintha Weerasekara
    /// Created date: 7/6/2008
    /// </remarks>
    class GroupingModuleConverter : ModuleConverter
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(GroupingModuleConverter));

        public GroupingModuleConverter(MappedObjectPair mappedObjectPair, DateTimePeriod period, ICccTimeZoneInfo timeZoneInfo)
            : base(mappedObjectPair, period, timeZoneInfo)
        {
        }

        /// <summary>
        /// Gets the name of the module.
        /// </summary>
        /// <value>The name of the module.</value>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 7/6/2008
        /// </remarks>
        protected override string ModuleName
        {
            get { return "GroupPage"; }
        }

        /// <summary>
        /// Gets the depended on.
        /// </summary>
        /// <value>The depended on.</value>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 7/6/2008
        /// </remarks>
        protected override IEnumerable<Type> DependedOn
        {
            get
            {
                return new[] { typeof(AgentModuleConverter) };
            }
        }

        /// <summary>
        /// Groups the convert.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 7/6/2008
        /// </remarks>
        protected override void GroupConvert()
        {
            ConvertGroupings();
        }

        /// <summary>
        /// Converts the groupings.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 7/6/2008
        /// </remarks>
        private void ConvertGroupings()
        {
            Logger.Info("Converting Groupings...");

            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                GroupingMapper groupingMapper = new GroupingMapper(MappedObjectPair, new CccTimeZoneInfo(System.TimeZoneInfo.Utc));
                GroupingConverter groupingConverter = new GroupingConverter(uow, groupingMapper);
                groupingConverter.ConvertAndPersist(new GroupingReader().LoadAllFromDBForConversionToRaptor().Values);
            }
        }
    }
}