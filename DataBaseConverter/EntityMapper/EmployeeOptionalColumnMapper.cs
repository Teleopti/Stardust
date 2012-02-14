#region Imports
using System.Collections.Generic;
using Domain;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
#endregion

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Maps old entity EmployeeOptionalColumn to new entity OptionalColumn
    /// </summary>
    /// <remarks>
    /// Created by: Sachintha Weerasekara
    /// Created date: 8/12/2008
    /// </remarks>
    public class EmployeeOptionalColumnMapper : Mapper<IOptionalColumn, EmployeeOptionalColumn>
    {
        // length of the Name column
        private const int MaxLengthName = 255;
        private const string PersonTable = "Person";
        private readonly ICollection<Agent> agentCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmployeeOptionalColumnMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <param name="agents">The agents.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 8/14/2008
        /// </remarks>
        public EmployeeOptionalColumnMapper(MappedObjectPair mappedObjectPair, 
                                    ICccTimeZoneInfo timeZone, ICollection<Agent> agents) : base(mappedObjectPair, timeZone)
        {
            agentCollection = agents;
        }

        /// <summary>
        /// Gets the agent collection.
        /// </summary>
        /// <value>The agent collection.</value>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 8/13/2008
        /// </remarks>
        public ICollection<Agent> AgentCollection
        {
            get { return agentCollection; }
        }

        /// <summary>
        /// Maps the specified old entity.
        /// </summary>
        /// <param name="oldEntity">The old entity.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/23/2007
        /// </remarks>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 8/12/2008
        /// </remarks>
        public override IOptionalColumn Map(EmployeeOptionalColumn oldEntity)
        {
            string columnName = ConversionHelper.MapString(oldEntity.Name, MaxLengthName);
            OptionalColumn optionalColumn = new OptionalColumn(columnName);
            optionalColumn.TableName = PersonTable;

            ICollection<Agent> agents = AgentCollection;
            foreach (Agent agent in agents)
            {
                foreach (AgentProperty agentProperty in agent.PropertyCollection)
                {
                    EmployeeOptionalColumn agentOptionalColumn = agentProperty.OptionalCol;
                    if (agentOptionalColumn.Id == oldEntity.Id)
                    {
                        OptionalColumnValue columnValue = new OptionalColumnValue(agentProperty.Value);
                        IPerson person = MappedObjectPair.Agent.GetPaired(agent);
                        if (person != null)
                        {
                            columnValue.ReferenceId = person.Id;
                            optionalColumn.AddOptionalColumnValue(columnValue);
                        }                        
                    }
                }               
            }

            return optionalColumn;
        }
    }
}
