using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Teleopti.Ccc.WpfControls.Demo.Models;

namespace Teleopti.Ccc.WpfControls.Demo
{
   
    //TODO: Verify public calls is on the UI-thread
    public class TrackerModel : DataModel,IFetchableModel<int>
    {
        private Models.ModelState _state;
        #region IFetchableModel<int> Members

        public bool FetchData(out int data)
        {
            //TODO: 
            VerifyCalledOnUIThread();
            data = 0;
            return true;
        }

        public ModelState ModelState
        {
            get { return _state; }
            set
            {
                VerifyCalledOnUIThread();
                if (_state!=value)
                {
                    _state = value;
                    SendPropertyChanged("ModelState");
                }
            }
        }

        #endregion

  
    }
}
