﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Forecasting;
using System.Collections;

namespace Teleopti.Ccc.Win.Forecasting.Forms
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
