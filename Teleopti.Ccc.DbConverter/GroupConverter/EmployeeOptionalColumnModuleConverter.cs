using System;
using System.Collections.Generic;
using Domain;
using Infrastructure;
using log4net;
using Teleopti.Ccc.DatabaseConverter.CollectionConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Ccc.DatabaseConverter;

namespace Teleopti.Ccc.DBConverter.GroupConverter
{
    /// <summary>
    /// Converts the module EmployeeOptionalColumn 
    /// </summary>
    /// <remarks>
    /// Created by: Sachintha Weerasekara
    /// Created date: 8/13/2008
    /// </remarks>
    internal class EmployeeOptionalColumnModuleConverter : ModuleConverter
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(EmployeeOptionalColumnModuleConverter));
        private readonly string moduleName;
        private readonly IEnumerable<Type> dependedOn;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmployeeOptionalColumnModuleConverter"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="period">The period.</param>
        /// <param name="timeZoneInfo">The time zone info.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 8/13/2008
        /// </remarks>
        public EmployeeOptionalColumnModuleConverter(MappedObjectPair mappedObjectPair, DateTimePeriod period,
                                                     ICccTimeZoneInfo timeZoneInfo)
            : base(mappedObjectPair, period, timeZoneInfo)
        {
            moduleName = "OptionalColumn";
            dependedOn = new[] { typeof(AgentModuleConverter) };
        }

        /// <summary>
        /// Gets the name of the module.
        /// </summary>
        /// <value>The name of the module.</value>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 8/13/2008
        /// </remarks>
        protected override string ModuleName
        {
            get { return moduleName; }
        }

        /// <summary>
        /// Gets the depended ModuleConverter.
        /// </summary>
        /// <value>The depended on.</value>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 8/13/2008
        /// </remarks>
        protected override IEnumerable<Type> DependedOn
        {
            get { return dependedOn; }
        }

        /// <summary>
        /// Converts old entities.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 8/13/2008
        /// </remarks>
        protected override void GroupConvert()
        {
            ConvertEmployeeOptionalColumns();
        }

        /// <summary>
        /// Converts EmployeeOptionalColumns.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 8/13/2008
        /// </remarks>
        private void ConvertEmployeeOptionalColumns()
        {
            Logger.Info("Converting EmployeeOptionalColumns...");

            using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                ICollection<Agent> agents = new AgentReader().GetAll().Values;
                EmployeeOptionalColumnMapper employeeOptionalColumnMapper =
                    new EmployeeOptionalColumnMapper(MappedObjectPair, new CccTimeZoneInfo(System.TimeZoneInfo.Utc), agents);

                EmployeeOptionalColumnConverter employeeOptionalColumnConverter =
                    new EmployeeOptionalColumnConverter(unitOfWork, employeeOptionalColumnMapper);

                employeeOptionalColumnConverter.ConvertAndPersist(new EmployeeOptionalColumnReader().LoadAllFromDB().Values);
            }
        }
    }
}