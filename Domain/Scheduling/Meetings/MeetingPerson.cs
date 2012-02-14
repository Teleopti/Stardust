using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Meetings
{
    /// <summary>
    /// Person stored in meetings
    /// </summary>
    public class MeetingPerson : AggregateEntity, IMeetingPerson
    {
        private IPerson _person;
        private Boolean _optional;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="person"></param>
        /// <param name="optional"></param>
        public MeetingPerson(IPerson person, Boolean optional)
        {
            _person = person;
            _optional = optional;
        }

        /// <summary>
        /// Empty constructor, NHibernate wants it
        /// </summary>
        protected MeetingPerson()
        { }

        /// <summary>
        /// Get/Set person
        /// </summary>
        public virtual IPerson Person
        {
            get { return _person; }
            set { _person = value; }
        }

        /// <summary>
        /// Get/Set optional value
        /// </summary>
        public virtual Boolean Optional
        {
            get { return _optional; }
            set { _optional = value; }
        }

        #region ICloneableEntity<MeetingPerson> Members


        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public virtual object Clone()
        {
            IMeetingPerson retObj = EntityClone();
            return retObj;
        }

        /// <summary>
        /// Returns a clone of this T with IEntitiy.Id set to null.
        /// </summary>
        /// <returns></returns>
        public virtual IMeetingPerson NoneEntityClone()
        {
            MeetingPerson retObj = (MeetingPerson)MemberwiseClone();
            retObj.SetId(null);
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
        public virtual IMeetingPerson EntityClone()
        {
            MeetingPerson retObj = (MeetingPerson)MemberwiseClone();
            return retObj;
        }

        #endregion
    }
}