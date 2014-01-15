using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{
    public class ApplicationRole : SystemRole, IApplicationRole
    {
        #region Variables

        private IList<IApplicationFunction> _applicationFunctionCollection = new List<IApplicationFunction>();
        private IAvailableData _availableData;
        private bool _builtIn;
        private IBusinessUnit _businessUnit;
        private IUserTextTranslator _userTextTranslator = new UserTextTranslator();

        #endregion

        #region Interface 

        public virtual ICollection<IApplicationFunction> ApplicationFunctionCollection
        {
            get { return new ReadOnlyCollection<IApplicationFunction>(_applicationFunctionCollection); }
        }

        public virtual void AddApplicationFunction(IApplicationFunction applicationFunction)
        {
            if(!_applicationFunctionCollection.Contains(applicationFunction))
                _applicationFunctionCollection.Add(applicationFunction);
        }

        public virtual bool RemoveApplicationFunction(IApplicationFunction applicationFunction)
        {
            return _applicationFunctionCollection.Remove(applicationFunction);
        }

        public virtual bool BuiltIn
        {
            get { return _builtIn; }
            set { _builtIn = value; }
        }

        public override string DescriptionText
        {
            get
            {
                return _userTextTranslator.TranslateText(base.DescriptionText);
            }
        }

        public override string AuthorizationValue
        {
            get
            {
                if (BuiltIn) 
                    return "xxBuiltIn";
                return string.Empty;
            }
        }

        public virtual IAvailableData AvailableData
        {
            get { return _availableData; }
            set { _availableData = value; }
        }

        /// <summary>
        /// Sets or gets the local user text translator.
        /// </summary>
        /// <value>The user text translator.</value>
        /// <remarks>
        /// Used for test purposes to change the default translator to an explicit one.
        /// </remarks>
        public virtual IUserTextTranslator UserTextTranslator
        {
            get { return _userTextTranslator; }
            set { _userTextTranslator = value; }
        }

        public virtual void SetBusinessUnit(IBusinessUnit businessUnit)
        {
            BusinessUnit = businessUnit;
        }

        public virtual IBusinessUnit BusinessUnit
        {
            get { return _businessUnit; }
            protected set { _businessUnit = value; }
        }

        #endregion

        #region ICloneableEntity<ApplicationRole> Members

        /// <summary>
        /// Returns a clone of this T with IEntitiy.Id set to null.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 2008-06-19
        /// </remarks>
        public virtual ApplicationRole NoneEntityClone()
        {
            ApplicationRole retObj = (ApplicationRole)MemberwiseClone();
            retObj.SetId(null);

            CopyFunctionsInto(retObj);
            ((IVersioned)retObj).SetVersion(0);
            return retObj;
        }

        /// <summary>
        /// Returns a clone of this T with IEntitiy.Id as this T.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 2008-06-19
        /// </remarks>
        public virtual ApplicationRole EntityClone()
        {
            ApplicationRole retObj = (ApplicationRole)MemberwiseClone();

            CopyFunctionsInto(retObj);

            return retObj;
        }

        /// <summary>
        /// Creates a list of application functions that is a copy of the current instance.
        /// </summary>
        /// <param name="role">Application role need to be copied.</param>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 2008-06-19
        /// </remarks>
        protected void CopyFunctionsInto(IApplicationRole role)
        {
            ((ApplicationRole)role).CreateApplicationFunctionCollection(new List<IApplicationFunction>());
            foreach (ApplicationFunction func in _applicationFunctionCollection)
            {
                role.AddApplicationFunction(func);
            }
        }


        private void CreateApplicationFunctionCollection(IList<IApplicationFunction> applicationFunctions)
        {
            _applicationFunctionCollection = applicationFunctions;
        }

        #endregion

        #region ICloneable Members

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 2008-06-19
        /// </remarks>
        public virtual object Clone()
        {
            return EntityClone();
        }

        #endregion

    }

}
