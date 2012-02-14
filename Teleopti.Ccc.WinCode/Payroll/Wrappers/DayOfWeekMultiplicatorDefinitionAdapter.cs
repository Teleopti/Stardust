using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Payroll.Wrappers
{
    public class DayOfWeekMultiplicatorDefinitionAdapter:BaseMultiplicatorDefinitionAdapter
    {
        
        #region Fields - Instance Members

        #region Fields - Instance Members - Private Fields

        /// <summary>
        /// Holds the contained entity
        /// </summary>
        private DayOfWeekMultiplicatorDefinition _containedEntity;

        #endregion

        #region Fields - Instance Members - Constants

        #endregion

        #endregion

        #region Properties - Instance Members

        /// <summary>
        /// Gets the contained entitity.
        /// </summary>
        /// <value>The contained entitity.</value>
        public DayOfWeekMultiplicatorDefinition ContainedEntity
        {
            get
            {
                return _containedEntity;
            }
        }

        /// <summary>
        /// Gets the end time.
        /// </summary>
        /// <value>The start time.</value>
        public DayOfWeek SelectedDayOfWeek
        {
            get
            {
                return _containedEntity.DayOfWeek;
            }
            set
            {
                _containedEntity.DayOfWeek = value;
            }
        }

        /// <summary>
        /// Gets the end time.
        /// </summary>
        /// <value>The start time.</value>
        public string StartTime
        {
            get
            {
                return TimeHelper.TimeOfDayFromTimeSpan(_containedEntity.Period.StartTime);
            }
            set
            {
                TimeSpan newStartTime;
                if (TimeHelper.TryParse(value, out newStartTime))
                {
                    TimePeriod perid = new TimePeriod(newStartTime, _containedEntity.Period.EndTime);
                    _containedEntity.Period = perid;
                }
            }
        }

        /// <summary>
        /// Gets the end time.
        /// </summary>
        /// <value>The start time.</value>
        public string EndTime
        {
            get
            {
                return TimeHelper.TimeOfDayFromTimeSpan(_containedEntity.Period.EndTime) ;
            }
            set
            {
                TimeSpan newEndTime;
                if(TimeHelper.TryParse(value , out newEndTime))
                {
                    TimePeriod perid = new TimePeriod(_containedEntity.Period.StartTime, newEndTime);
                    _containedEntity.Period = perid;
                }
            }
        }
      
        #endregion

        #region  Methods - Instance Members

        #region  Methods - Instance Members - Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DayOfWeekMultiplicatorDefinitionAdapter"/> class.
        /// </summary>
        /// <param name="dayOfWeekMultiplicatorDefinition">The day of week multiplicator definition.</param>
        public DayOfWeekMultiplicatorDefinitionAdapter(DayOfWeekMultiplicatorDefinition dayOfWeekMultiplicatorDefinition):base(dayOfWeekMultiplicatorDefinition)
        {
            _containedEntity = dayOfWeekMultiplicatorDefinition;
        }

        /// <summary>
        /// Parses the specified <see cref="DayOfWeekMultiplicatorDefinition"/> list to a list of <see cref="DayOfWeekMultiplicatorDefinitionAdapter"/>.
        /// </summary>
        /// <param name="dayOfWeekMultiplicatorDefinitions">The day of week multiplicator definitions.</param>
        /// <returns></returns>
        public static IList<DayOfWeekMultiplicatorDefinitionAdapter> Parse(IList<DayOfWeekMultiplicatorDefinition> dayOfWeekMultiplicatorDefinitions)
        {
            IList<DayOfWeekMultiplicatorDefinitionAdapter> list = new List<DayOfWeekMultiplicatorDefinitionAdapter>();
            foreach (var definition in dayOfWeekMultiplicatorDefinitions)
            {
                list.Add(new DayOfWeekMultiplicatorDefinitionAdapter(definition));
                
            }

            return list;
        }

        /// <summary>
        /// Parses the specified <see cref="DayOfWeekMultiplicatorDefinition"/> to <see cref="DayOfWeekMultiplicatorDefinitionAdapter"/>
        /// </summary>
        /// <param name="dayOfWeekMultiplicatorDefinition">The day of week multiplicator definition.</param>
        /// <returns></returns>
        public static DayOfWeekMultiplicatorDefinitionAdapter Parse(DayOfWeekMultiplicatorDefinition dayOfWeekMultiplicatorDefinition)
        {
            return new DayOfWeekMultiplicatorDefinitionAdapter(dayOfWeekMultiplicatorDefinition);
        }

        #endregion

        #endregion

    }
}
