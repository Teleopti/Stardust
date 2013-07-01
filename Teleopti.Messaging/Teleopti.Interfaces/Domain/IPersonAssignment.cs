using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Represents a person's work assignment. Roughly speaking it contains 
    /// all the information about the person's work assignment for a concrete period: what, when,
    /// how long to work with etc. However it only contains activities, does not contain absences.
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-02-25
    /// </remarks>
    public interface IPersonAssignment : IPersistableScheduleData, 
											IAggregateRootWithEvents,
                                            IChangeInfo,
                                            IBelongsToBusinessUnit,
                                            IRestrictionChecker<IPersonAssignment>, 
                                            IProjectionSource, 
                                            ICloneableEntity<IPersonAssignment>
    {
			void SetMainShiftLayers(IEnumerable<IMainShiftLayer> activityLayers, IShiftCategory shiftCategory);


        /// <summary>
        /// Clears the personal shift.
        /// </summary>
        void ClearPersonalLayers();


        /// <summary>
        /// Clears the main shift.
        /// </summary>
        void ClearMainLayers();

	    void ClearOvertimeLayers();

        /// <summary>
        /// Gets or sets the ZOrder for PersonAssignment, used in gui.
        /// </summary>
        DateTime ZOrder{get; set;}

			/// <summary>
			/// The date
			/// </summary>
	    DateOnly Date { get; }

	    IShiftCategory ShiftCategory { get; }
	    IEnumerable<IMainShiftLayer> MainLayers { get; }
			IEnumerable<IPersonalShiftLayer> PersonalLayers { get; }
	    IEnumerable<IOvertimeShiftLayer> OvertimeLayers { get; }

		/// <summary>
		/// Publish the ScheduleChangedEvent
		/// </summary>
		void ScheduleChanged(string dataSource);

	    bool RemoveLayer(IMainShiftLayer layer);
	    bool RemoveLayer(IPersonalShiftLayer layer);
	    bool RemoveLayer(IOvertimeShiftLayer layer);
	    void AddPersonalLayer(IActivity activity, DateTimePeriod period);
	    void AddOvertimeLayer(IActivity activity, DateTimePeriod period, IMultiplicatorDefinitionSet multiplicatorDefinitionSet);
    }
}