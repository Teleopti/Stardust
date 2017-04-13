using System;
using System.Collections;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms
{
    public class ModifyCellEventArgs : EventArgs
    {
        IEnumerable _dataPeriods;
        ModifyCellOption _modifyCellOption;

        public ModifyCellEventArgs(ModifyCellOption option, IEnumerable dataPeriods)
        {
            _dataPeriods = dataPeriods;
            _modifyCellOption = option;
        }

        public ModifyCellOption ModifyCellOption
        {
            get { return _modifyCellOption; }
        }

        public IEnumerable DataPeriods
        {
            get { return _dataPeriods; }
        }
    }
}
