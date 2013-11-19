using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Common
{
    /// <summary>
    /// Represents an scenario
    /// </summary>
    public class Scenario : VersionedAggregateRootWithBusinessUnit, IScenario, IDeleteTag
    {
        #region Fields

        private Description _description;
        private bool _defaultScenario;
        private bool _enableReporting;
        private bool _isDeleted;
    	private bool _restricted;

        #endregion

        #region Constructor
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Scenario"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-26
        /// </remarks>
        public Scenario(string name)
        {
            _description = new Description(name);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Scenario"/> class for NHibernate.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-26
        /// </remarks>
        protected Scenario()
        {
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public virtual Description Description
        {
            get { return _description; }
            set { _description = value; }
        }


        /// <summary>
        /// Gets if set to default workspace.
        /// </summary>
        /// <value>Default or not.</value>
        public virtual bool DefaultScenario
        {
            get { return _defaultScenario; }
            //TODO: only one scenario may be default, create special scenariocollection for this
            set { _defaultScenario = value; }
        }

        public virtual bool IsDeleted
        {
            get { return _isDeleted; }
        }

        /// <summary>
        /// Gets if scenario is enabled for reporting.
        /// </summary>
        /// <value>Scenario is enabled for reporting</value>
        public virtual bool EnableReporting
        {
            get { return _enableReporting; }
            set { _enableReporting = value; }
        }

        #endregion

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>.
        /// </returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 11/18/2007
        /// </remarks>
        public virtual int CompareTo(IScenario other)
        {
            return String.Compare(Description.Name, other.Description.Name, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 11/18/2007
        /// </remarks>
        public override string ToString()
        {
            return String.Concat(base.ToString(), " ", Description.ToString());
        }

        public virtual void SetDeleted()
        {
            _isDeleted = true;
        }

		#region IScenario Members


		public virtual bool Restricted
		{
			get { return _restricted; }
			set { _restricted = value; }
		}

		#endregion
	}
}
