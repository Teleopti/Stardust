using System;
using System.ComponentModel;

namespace Teleopti.Ccc.WinCode.Common.Models
{
    public class DataModelWithInvalidState : DataModel, IDataErrorInfo
    {
        bool _isValid = true;
        
        public string this[string columnName]
        {
            get
            {
                string ret = CheckProperty(columnName);
                IsValid = string.IsNullOrEmpty(ret);
                return ret;
                
            }
        }

        //Not used for now
        public virtual string Error
        {
            get { return UserTexts.Resources.InvalidCredential; }
        }


        protected virtual string CheckProperty(string propertyName)
        {
            return null;
        }

        //Note: maybe change this to be included in ModelState?
        public bool IsValid
        {
            get{ return _isValid;}
            set
            {
                if(_isValid!=value)
                {
                    _isValid = value;
                }
                NotifyProperty(()=>IsValid);

            }
        }

        
    }
}
