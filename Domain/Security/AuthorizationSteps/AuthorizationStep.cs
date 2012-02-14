using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.AuthorizationSteps
{
    /// <summary>
    /// Base class for authorization providers. Implements a local store 
    /// for the result list.
    /// </summary>
    public abstract class AuthorizationStep : IAuthorizationStep
    {

        #region Variables

        private IList<IAuthorizationEntity> _providedList;
        private IList<IAuthorizationStep> _parents;
        private string _stepName;
        private string _stepDescription;
        private Exception _innerException;
        private string _warningMessage;
        private bool _enabled = true;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationStep"/> class.
        /// </summary>
        /// <param name="stepName">Name of the panel.</param>
        protected AuthorizationStep(
            string stepName)
        {
            _stepName = stepName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationStep"/> class.
        /// </summary>
        /// <param name="stepName">Name of the panel.</param>
        /// <param name="description">The description.</param>
        protected AuthorizationStep(
            string stepName,
            string description)
        {
            _stepName = stepName;
            _stepDescription = description;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationStep"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="stepName">Name of the panel.</param>
        /// <param name="description">The description.</param>
        protected AuthorizationStep(
           IAuthorizationStep parent,
           string stepName,
           string description)
            : this(stepName, description)
        {
            if (parent != null)
            {
                _parents = new List<IAuthorizationStep>();
                _parents.Add(parent);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationStep"/> class.
        /// </summary>
        /// <param name="parents">The parents.</param>
        /// <param name="stepName">Name of the panel.</param>
        /// <param name="description">The description.</param>
        protected AuthorizationStep(
           IList<IAuthorizationStep> parents,
           string stepName,
           string description)
            : this(stepName, description)
        {
            _parents = parents;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationStep"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="stepName">Name of the panel.</param>
        protected AuthorizationStep(
            IAuthorizationStep parent,
            string stepName)
            : this(parent, stepName, string.Empty)
        {
            //
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationStep"/> class.
        /// </summary>
        /// <param name="parents">The parents.</param>
        /// <param name="stepName">Name of the panel.</param>
        protected AuthorizationStep(
            IList<IAuthorizationStep> parents,
            string stepName)
            : this(parents, stepName, string.Empty)
        {
            //
        }

        #endregion

        #region IAuthorizationPanel Members

        /// <summary>
        /// Gets the list
        /// </summary>
        /// <returns></returns>
        public IList<T> ProvidedList<T>() where T : IAuthorizationEntity
        {
            if (_providedList == null)
                _providedList = new List<IAuthorizationEntity>();
            return new List<T>(_providedList.OfType<T>());
        }

        /// <summary>
        /// Re-read list from source and refreshes the local ProvidedList variable.
        /// </summary>
        public void RefreshList()
        {
            foreach (IAuthorizationStep parent in Parents)
            {
                if (parent != null)
                    parent.RefreshList();
            }
            if (Enabled)
            {
                InnerException = null;
                WarningMessage = string.Empty;
                try
                {
                    _providedList = RefreshOwnList();
                }
                catch (Exception ex)
                {
                    // Note: it is intended to hide and store exception.
                    _innerException = ex;
                }
            }
        }

        /// <summary>
        /// Refreshes the own list. Template abstact method
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 2007-10-31
        /// </remarks>
        protected abstract IList<IAuthorizationEntity> RefreshOwnList();

        /// <summary>
        /// Gets the parents.
        /// </summary>
        /// <remarks>
        /// For usability reasons, the property never gives back null, but an empty list instead.
        /// </remarks>
        public IList<IAuthorizationStep> Parents
        {
            get
            {
                if (_parents == null)
                    _parents = new List<IAuthorizationStep>();
                return _parents;
            }
            protected set { _parents = value; }
        }

        /// <summary>
        /// Get the name of the panel
        /// </summary>
        public string PanelName
        {
            get { return _stepName; }
            protected set { _stepName = value; }
        }

        /// <summary>
        /// Get the description of what the panel does
        /// </summary>
        public string PanelDescription
        {
            get { return _stepDescription; }
            protected set { _stepDescription = value; }
        }

        /// <summary>
        /// Gets the inner exception occurred while RefreshList() method.
        /// </summary>
        /// <value>The inner exception.</value>
        public Exception InnerException
        {
            get { return _innerException; }
            protected set { _innerException = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="IAuthorizationStep"/> is enabled by the user.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        /// <summary>
        /// Gets or sets the warning message.
        /// </summary>
        /// <value>The warning message.</value>
        public string WarningMessage
        {
            get { return _warningMessage; }
            set { _warningMessage = value; }
        }

        #endregion
    }
}
