using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationSteps;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.Security
{
    /// <summary>
    /// AuthorizationStep testable class
    /// </summary>
    /// <remarks>
    /// Created by: tamasb
    /// Created date: 11/20/2007
    /// </remarks>
    public class AuthorizationStepTestClass : AuthorizationStep
    {
        private bool _refreshOwnListCalled;
        private IList<IAuthorizationEntity> _resultProvidedList;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationStepTestClass"/> class.
        /// </summary>
        /// <param name="panelName">Name of the panel.</param>
        public AuthorizationStepTestClass(
            string panelName)
            : base(panelName)
        {
            //
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationStepTestClass"/> class.
        /// </summary>
        /// <param name="panelName">Name of the panel.</param>
        /// <param name="description">The description.</param>
        public AuthorizationStepTestClass(
            string panelName,
            string description)
            : base(panelName, description)
        {
            //
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationStepTestClass"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="panelName">Name of the panel.</param>
        /// <param name="description">The description.</param>
        public AuthorizationStepTestClass(
           IAuthorizationStep parent,
           string panelName,
           string description)
            : base(parent, panelName, description)
        {
            //
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationStepTestClass"/> class.
        /// </summary>
        /// <param name="parents">The parents.</param>
        /// <param name="panelName">Name of the panel.</param>
        /// <param name="description">The description.</param>
        public AuthorizationStepTestClass(
           IList<IAuthorizationStep> parents,
           string panelName,
           string description)
            : base(parents, panelName, description)
        {
            //
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationStepTestClass"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="panelName">Name of the panel.</param>
        public AuthorizationStepTestClass(
            IAuthorizationStep parent,
            string panelName)
            : base(parent, panelName)
        {
            //
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationStepTestClass"/> class.
        /// </summary>
        /// <param name="parents">The parents.</param>
        /// <param name="panelName">Name of the panel.</param>
        public AuthorizationStepTestClass(
            IList<IAuthorizationStep> parents,
            string panelName)
            : base(parents, panelName)
        {
            //
        }

        #endregion

        /// <summary>
        /// Gets a value indicating whether the own list refreshed method has been called.
        /// </summary>
        /// <value><c>true</c> if own list refreshed has been called; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// For testing purposes only.
        /// Created by: tamasb
        /// Created date: 11/23/2007
        /// </remarks>
        public bool RefreshedOwnListCalled
        {
            get { return _refreshOwnListCalled; }
        }

        /// <summary>
        /// Sets the protected name of the panel in the base class.
        /// </summary>
        /// <param name="panelName">Name of the panel.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/23/2007
        /// </remarks>
        public void SetPanelName(string panelName)
        {
            PanelName = panelName;
        }

        /// <summary>
        /// Sets the protected panel description in the base class.
        /// </summary>
        /// <param name="panelDescription">The panel description.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/23/2007
        /// </remarks>
        public void SetPanelDescription(string panelDescription)
        {
            PanelDescription = panelDescription;
        }

        /// <summary>
        /// Sets the protected parents in the base class.
        /// </summary>
        /// <param name="parents">The parents.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/23/2007
        /// </remarks>
        public void SetParents(IList<IAuthorizationStep> parents)
        {
            Parents = parents;
        }

        /// <summary>
        /// Sets the refresh own list result for storing.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/27/2007
        /// </remarks>
        public void SetRefreshOwnListResult(IList<IAuthorizationEntity> list)
        {
            _resultProvidedList = list;
        }

        /// <summary>
        /// Sets the inner exception for test purposes only.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/27/2007
        /// </remarks>
        public void SetInnerException(Exception exception)
        {
            InnerException = exception;
        }

        /// <summary>
        /// Refreshes the own list. Provides a dummy implewmentation to the 
        /// template abstact method
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 2007-10-31
        /// </remarks>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/23/2007
        /// </remarks>
        protected override IList<IAuthorizationEntity> RefreshOwnList()
        {
            _refreshOwnListCalled = true;
            return _resultProvidedList;
        }
    }
}
