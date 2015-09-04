using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SystemSetting
{
    public class GlobalSettingData : SettingData,
                                        IBelongsToBusinessUnit,
                                        IChangeInfo,
                                        IVersioned
    {
#pragma warning disable 0649
        private IPerson _createdBy;
        private DateTime? _createdOn;
        private int? _version;
        private IPerson _updatedBy;
        private DateTime? _updatedOn;
#pragma warning restore 0649
        private IBusinessUnit _businessUnit;


        protected GlobalSettingData(){}

        public GlobalSettingData(string name) : base(name){}


        public virtual IBusinessUnit BusinessUnit
        {
            get { return _businessUnit ?? (_businessUnit = CurrentBusinessUnit.Instance.Current()); }
	        protected set { _businessUnit = value; }
        }

        public virtual IPerson CreatedBy
        {
            get { return _createdBy; }
        }

        public virtual DateTime? CreatedOn
        {
            get { return _createdOn; }
        }

        public virtual IPerson UpdatedBy
        {
            get { return _updatedBy; }
        }

        public virtual DateTime? UpdatedOn
        {
            get { return _updatedOn; }
        }

        public virtual int? Version
        {
            get { return _version; }
        }

        public virtual void SetVersion(int version)
        {
            _version = version;
        }
    }
}
