using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
{

    /// <summary>
    /// Enum for stating the action.
    /// </summary>
    public enum PasteAction
    {
        /// <summary>
        /// Add.
        /// </summary>
        Add,
        /// <summary>
        /// Remove all previous and insert the new
        /// </summary>
        Replace,
        /// <summary>
        /// Do not do any action
        /// </summary>
        Ignore
    }

    /// <summary>
    /// Specifies what should be pasted in a paste operation
    /// </summary>
    public class PasteOptions
    {
        

        private bool _mainShift;
	    private bool _mainShiftSpecial;
        private PasteAction _absences = PasteAction.Ignore;
        private IPasteBehavior _pasteBehavior = new NormalPasteBehavior();
        private bool _dayOff;
        private bool _personalShifts;
        private bool _overtime;
        private bool _default;
        private bool _defaultDelete;
        private bool _preference;
        private bool _studentAvailability;
        private bool _overtimeAvailability;
	    private bool _shiftAsOvertime;
	    private IMultiplicatorDefinitionSet _multiplicatorDefinitionSet;


        /// <summary>
        /// Gets or sets the paste behavior.
        /// </summary>
        /// <value>The paste behavior.</value>
        /// <remarks>
        /// Returns StandardPasteBehavior if not set
        /// </remarks>
        public IPasteBehavior PasteBehavior
        {
            get
            {
               return _pasteBehavior;
            }
            set { _pasteBehavior = value; }
        }


        /// <summary>
        /// MainShift
        /// </summary>
        public bool MainShift
        {
            get { return _mainShift; }
            set { _mainShift = value; }
        }

		public bool MainShiftSpecial
		{
			get { return _mainShiftSpecial; }
			set { _mainShiftSpecial = value; }
		}

        /// <summary>
        /// Absences
        /// </summary>
        public PasteAction Absences
        {
            get { return _absences; }
            set { _absences = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [day off].
        /// </summary>
        /// <value><c>true</c> if [day off]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-01-16
        /// </remarks>
        public bool DayOff
        {
            get { return _dayOff; }
            set { _dayOff = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether personal shifts should be included in the operation.
        /// </summary>
        /// <value><c>true</c> if [personal shifts]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-05-30
        /// </remarks>
        public bool PersonalShifts
        {
            get { return _personalShifts; }
            set { _personalShifts = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PasteOptions"/> is default.
        /// </summary>
        /// <value><c>true</c> if default; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-06-01
        /// </remarks>
        public bool Default
        {
            get { return _default; }
            set { _default = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating if this is a default delete
        /// </summary>
        public bool DefaultDelete
        {
            get { return _defaultDelete; }
            set { _defaultDelete = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PasteOptions"/> is overtime.
        /// </summary>
        /// <value><c>true</c> if overtime; otherwise, <c>false</c>.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-02-24    
        /// /// </remarks>
        public bool Overtime
        {
            get { return _overtime; }
            set { _overtime = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating if preferences should be included in the operation
        /// </summary>
        public bool Preference
        {
            get { return _preference; }
            set { _preference = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating if student availability should be included in the operation
        /// </summary>
        public bool StudentAvailability
        {
            get { return _studentAvailability; }
            set { _studentAvailability = value; }
        }

        public bool OvertimeAvailability
        {
            get { return _overtimeAvailability; }
            set { _overtimeAvailability = value; }
        }

	    public bool ShiftAsOvertime
	    {
			get { return _shiftAsOvertime; }
			set { _shiftAsOvertime = value; }
	    }

	    public IMultiplicatorDefinitionSet MulitiplicatorDefinitionSet
	    {
			get { return _multiplicatorDefinitionSet; }
			set { _multiplicatorDefinitionSet = value; }
	    }
    }
}
