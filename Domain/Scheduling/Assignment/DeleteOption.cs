
namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    /// <summary>
    /// Hold options for what should be deleted in a delete operation
    /// </summary>
    public class DeleteOption
    {
        private bool _mainShift;
		private bool _mainShiftSpecial;
        private bool _absence;
        private bool _dayOff;
        private bool _personalShift;
        private bool _overtime;
        private bool _preference;
        private bool _studentAvailability;
        private bool _default;
        private bool _overtimeAvailability;

        /// <summary>
        /// Main shift
        /// </summary>
        public bool MainShift
        {
            get { return _mainShift; }
            set 
            {
                if (value)
                    Default = false;

                _mainShift = value; 
            }

        }

		public bool MainShiftSpecial
		{
			get { return _mainShiftSpecial; }
			set
			{
				if (value)
					Default = false;

				_mainShiftSpecial = value;
			}

		}

        /// <summary>
        /// Absence
        /// </summary>
        public bool Absence
        {
            get { return _absence; }
            set 
            {
                if (value)
                    Default = false;

                _absence = value; 
            }
        }

        /// <summary>
        /// Day off
        /// </summary>
        public bool DayOff
        {
            get { return _dayOff; }
            set 
            {
                if (value)
                    Default = false;

                _dayOff = value; 
            }
        }

        /// <summary>
        /// Personal shift
        /// </summary>
        public bool PersonalShift
        {
            get { return _personalShift; }
            set 
            {
                if (value)
                    Default = false;

                _personalShift = value; 
            }
        }

        /// <summary>
        /// Default
        /// </summary>
        public bool Default
        {
            get { return _default; }
            set
            {
                if (value)
                {
                    MainShift = false;
                    Absence = false;
                    DayOff = false;
                    PersonalShift = false;
                }

                _default = value;
            }
        }

        /// <summary>
        /// Overtime
        /// </summary>
        public bool Overtime
        {
            get { return _overtime; }
            set
            {
                if (value)
                    Default = false;

                _overtime = value;
            }
        }

        /// <summary>
        /// Preference
        /// </summary>
        public bool Preference
        {
            get { return _preference; }
            set
            {
                if (value)
                    Default = false;

                _preference = value;
            }
        }

        /// <summary>
        /// Student availability
        /// </summary>
        public bool StudentAvailability
        {
            get { return _studentAvailability; }
            set
            {
                if (value)
                    Default = false;

                _studentAvailability = value;
            }
        }

        public bool OvertimeAvailability
        {
            get { return _overtimeAvailability; }
            set
            {
                if (value)
                    Default = false;

                _overtimeAvailability = value;
            }
        }
    }
}
