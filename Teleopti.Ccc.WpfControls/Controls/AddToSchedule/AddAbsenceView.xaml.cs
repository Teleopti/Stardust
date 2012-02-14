using System;
using System.Collections.Generic;
using System.ComponentModel;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.WinCode.Converters.DateTimeConverter;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WpfControls.Controls.AddToSchedule
{
    /// <summary>
    /// Interaction logic for AddAbsence.xaml
    /// </summary>
    public partial class AddAbsenceView
    {
        public AddAbsenceView()
        {
            InitializeComponent();


            if (DesignerProperties.GetIsInDesignMode(this))
            {
                DateTimeToLocalStringConverter.IsDesignTime = true;
                DataContext = new AddAbsenceViewModel(new List<IAbsence>(), new SetupDateTimePeriodToDefaultPeriod(new DateTimePeriod(2001, 1, 1, 2001, 1, 2)), TimeSpan.FromMinutes(15));
            }
        }
    }
}