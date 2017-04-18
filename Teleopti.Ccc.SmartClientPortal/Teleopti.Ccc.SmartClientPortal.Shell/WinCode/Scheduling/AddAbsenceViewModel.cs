using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Time;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{

    /// <summary>
    /// Responsible for handling the logic when adding a new AbsenceLayer
    /// </summary>
    public class AddAbsenceViewModel : AddLayerViewModel<IAbsence>
    {

        public AddAbsenceViewModel(IEnumerable<IAbsence> absences, ISetupDateTimePeriod period, TimeSpan interval)
            : base(absences, period, UserTexts.Resources.AddAbsence, interval)
        {


        }

    }
}
