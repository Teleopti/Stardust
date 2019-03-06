using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{
	// Used for translation auto search, DO NOT REMOVE!
	// UserTexts.Resources.SuperRole

	public class ApplicationRole : SystemRole, IApplicationRole
    {
	    private IList<IApplicationFunction> _applicationFunctionCollection = new List<IApplicationFunction>();
        private IAvailableData _availableData;
        private bool _builtIn;
        private IBusinessUnit _businessUnit;
        private static IUserTextTranslator _userTextTranslator = new UserTextTranslator();

	    public virtual ICollection<IApplicationFunction> ApplicationFunctionCollection => new ReadOnlyCollection<IApplicationFunction>(_applicationFunctionCollection);

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
					if (!string.IsNullOrEmpty(base.DescriptionText) &&( Enum.IsDefined(typeof(ShippedCustomRoles), base.DescriptionText) || _builtIn))
						return _userTextTranslator.TranslateText(base.DescriptionText);
	            return base.DescriptionText;
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
			_businessUnit = businessUnit;
        }

		public virtual IBusinessUnit BusinessUnit
		{
			get { return _businessUnit; }
			set { _businessUnit = value; }
		}
        
		public virtual IBusinessUnit GetOrFillWithBusinessUnit_DONTUSE()
		{
			return _businessUnit;
		}

        public virtual ApplicationRole NoneEntityClone()
        {
            var retObj = (ApplicationRole)MemberwiseClone();
            retObj.SetId(null);

            CopyFunctionsInto(retObj);
            ((IVersioned)retObj).SetVersion(0);
            return retObj;
        }

        public virtual ApplicationRole EntityClone()
        {
            var retObj = (ApplicationRole)MemberwiseClone();

            CopyFunctionsInto(retObj);

            return retObj;
        }

        protected void CopyFunctionsInto(IApplicationRole role)
        {
            ((ApplicationRole)role)._applicationFunctionCollection = new List<IApplicationFunction>();
            foreach (var func in _applicationFunctionCollection)
            {
                role.AddApplicationFunction(func);
            }
        }

        public virtual object Clone()
        {
            return EntityClone();
        }
    }
}
