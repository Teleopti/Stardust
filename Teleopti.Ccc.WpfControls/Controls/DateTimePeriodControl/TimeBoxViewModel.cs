using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.WinCode.Common.Models;

namespace Teleopti.Ccc.WpfControls.Controls.DateTimePeriodControl
{
    public class TimeBoxViewModel : DataModel
    {
        private DateTime _value = DateTime.UtcNow;
        public DateTime Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                SendPropertyChanged("Value");
                
            }
        }
    }
}
