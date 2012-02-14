using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    /// <summary>
    /// Class describing an PersonAbsence
    /// </summary>
    public class PersonAbsence : AggregateRootWithBusinessUnit, 
                                    IPersonAbsence,
                                    IExportToAnotherScenario
    {
        private IPerson _person;
        private IScenario _scenario;
        private IAbsenceLayer _layer;
        private DateTime? _lastChange;

        /// <summary>
        /// Creates a new instance of PersonAbsence
        /// </summary>
        /// <param name="agent">The agent.</param>
        /// <param name="scenario">The scenario.</param>
        /// <param name="layer">The layer.</param>
        public PersonAbsence(IPerson agent,  
                             IScenario scenario,
                             IAbsenceLayer layer)
        {
            _person = agent;
            _scenario = scenario;
            _layer = layer;
        	_layer.SetParent(this);
        }

        /// <summary>
        /// Constructor for NHibernate
        /// </summary>
        protected PersonAbsence()
        {
        }

        /// <summary>
        /// Gets the layer.
        /// </summary>
        /// <value>The layer.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-02-15
        /// </remarks>
        public virtual IAbsenceLayer Layer
        {
            get { return _layer;}
        }



        /// <summary>
        /// Gets or sets the Person
        /// </summary>
        public virtual IPerson Person
        {
            get { return _person; }
        }

        /// <summary>
        /// Gets or sets the Scenario
        /// </summary>
        public virtual IScenario Scenario
        {
            get { return _scenario; }
        }

        public virtual bool BelongsToScenario(IScenario scenario)
        {
            return Scenario.Equals(scenario);
        }

        public virtual string FunctionPath
        {
            get { return DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence; }
        }

        public virtual IPersistableScheduleData CreateTransient()
        {
            return NoneEntityClone();
        }

        public virtual IPersistableScheduleData CloneAndChangeParameters(IScheduleParameters parameters)
        {
            var retObj = (PersonAbsence)NoneEntityClone();
            retObj._scenario = parameters.Scenario;
            retObj._person = parameters.Person;
            return retObj;
        }


        /// <summary>
        /// Gets the Period.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-02-06
        /// </remarks>
        public virtual DateTimePeriod Period
        {
            get { return Layer.Period; }
        }

        /// <summary>
        /// Returns a list with the absence splitted on specified period
        /// </summary>
        /// <param name="period"></param>
        /// <returns></returns>
        public virtual IList<IPersonAbsence> Split(DateTimePeriod period)
        {
            IList<IPersonAbsence> splitList = new List<IPersonAbsence>();

            if (!period.Contains(Period))
            {
                if (_layer.Period.Intersect(period))
                {
                	var newPeriods = Period.ExcludeDateTimePeriod(period);
                	foreach (var dateTimePeriod in newPeriods)
                	{
						if (dateTimePeriod.ElapsedTime() > TimeSpan.Zero)
						{
							IPersonAbsence personAbsence = NoneEntityClone();
							personAbsence.Layer.Period = dateTimePeriod;
							splitList.Add(personAbsence);
						}
                	}
                }
            }

            return splitList;
        }


        /// <summary>
        /// return a merged absence, or null if no merge
        /// </summary>
        /// <param name="personAbsence"></param>
        /// <returns></returns>
        public virtual IPersonAbsence Merge(IPersonAbsence personAbsence)
        {
            IPersonAbsence mergedPersonAbsence = null;

            if (personAbsence.Layer.Payload.Equals(Layer.Payload))
            {
                if (personAbsence.Period.Intersect(Period) || personAbsence.Period.Adjacent(Period))
                {
                    mergedPersonAbsence = NoneEntityClone();
                    mergedPersonAbsence.Layer.Period = Period.MaximumPeriod(personAbsence.Period);
                }
            }

            return mergedPersonAbsence;
        }

        public virtual DateTime? LastChange
        {
            get{return _lastChange;}
            set { _lastChange = value; }
        }

        #region ICloneableEntity<PersonAbsence> Members

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2007-11-09
        /// </remarks>
        public virtual object Clone()
        {
            return EntityClone();
        }

        public virtual bool BelongsToPeriod(IDateOnlyAsDateTimePeriod dateAndPeriod)
        {
            var addExtraEndDayDueToNightShifts = dateAndPeriod.Period().ChangeEndTime(TimeSpan.FromDays(1));
            return addExtraEndDayDueToNightShifts.Intersect(Period);
        }

        public virtual bool BelongsToPeriod(DateOnlyPeriod dateOnlyPeriod)
        {
            DateTimePeriod dateTimePeriod =
                dateOnlyPeriod.ToDateTimePeriod(Person.PermissionInformation.DefaultTimeZone());

            var addExtraEndDayDueToNightShifts = dateTimePeriod.ChangeEndTime(TimeSpan.FromDays(1));
            return addExtraEndDayDueToNightShifts.Intersect(Period);
        }

        /// <summary>
        /// Returns a clone of this T with IEntitiy.Id set to null.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-05-27
        /// </remarks>
        public virtual IPersonAbsence NoneEntityClone()
        {
            PersonAbsence retObj = (PersonAbsence)MemberwiseClone();
            retObj.SetId(null);
            retObj._layer = (AbsenceLayer)Layer.NoneEntityClone();

            return retObj;
        }

        /// <summary>
        /// Returns a clone of this T with IEntitiy.Id as this T.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-05-27
        /// </remarks>
        public virtual IPersonAbsence EntityClone()
        {
            PersonAbsence retObj = (PersonAbsence)MemberwiseClone();
            retObj._layer = (AbsenceLayer)Layer.EntityClone();

            return retObj;
        }

        #endregion

        public virtual IVisualLayerFactory CreateVisualLayerFactory()
        {
            return new VisualLayerFactory(); //maybe another object not allowing setuplayers?
        }

        public virtual IAggregateRoot MainRoot
        {
            get { return Person; }
        }
    }
}
