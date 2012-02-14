using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a DefinedRaptorApplicationFunctionPathsDto object.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class DefinedRaptorApplicationFunctionPathsDto
    {
        /// <summary>
        /// Gets or sets the open raptor application.
        /// </summary>
        /// <value>The open raptor application.</value>
        [DataMember]
        public string OpenRaptorApplication{get;set;}

        /// <summary>
        /// Gets or sets the raptor global.
        /// </summary>
        /// <value>The raptor global.</value>
        [DataMember]
        public  string RaptorGlobal{get;set;}

        /// <summary>
        /// Gets or sets the modify main shift.
        /// </summary>
        /// <value>The modify main shift.</value>
        [DataMember]
        public  string ModifyMainShift{get;set;}

        /// <summary>
        /// Gets or sets the modify personal shift.
        /// </summary>
        /// <value>The modify personal shift.</value>
        [DataMember]
        public  string ModifyPersonalShift{get;set;}

        /// <summary>
        /// Gets or sets the modify person absence.
        /// </summary>
        /// <value>The modify person absence.</value>
        [DataMember]
        public  string ModifyPersonAbsence{get;set;}

        /// <summary>
        /// Gets or sets the modify person day off.
        /// </summary>
        /// <value>The modify person day off.</value>
        [DataMember]
        public  string ModifyPersonDayOff{get;set;}

        /// <summary>
        /// Gets or sets the modify person assignment.
        /// </summary>
        /// <value>The modify person assignment.</value>
        [DataMember]
        public  string ModifyPersonAssignment{get;set;}

        /// <summary>
        /// Gets or sets the view unpublished schedules.
        /// </summary>
        /// <value>The view unpublished schedules.</value>
        [DataMember]
        public  string ViewUnpublishedSchedules{get;set;}

        /// <summary>
        /// Gets or sets the access to reports.
        /// </summary>
        /// <value>The access to reports.</value>
        [DataMember]
        public  string AccessToReports{get;set;}

        /// <summary>
        /// Gets or sets the open agent portal.
        /// </summary>
        /// <value>The open agent portal.</value>
        [DataMember]
        public  string OpenAgentPortal{get;set;}

        /// <summary>
        /// Gets or sets the open asm.
        /// </summary>
        /// <value>The open asm.</value>
        [DataMember]
        public  string OpenAsm{get;set;}

        /// <summary>
        /// Gets or sets the modify shift category preferences.
        /// </summary>
        /// <value>The modify shift category preferences.</value>
        [DataMember]
        public  string ModifyShiftCategoryPreferences{get;set;}

        /// <summary>
        /// Gets or sets the modify extended preferences.
        /// </summary>
        /// <value>The modify extended preferences.</value>
        [DataMember]
        public string ModifyExtendedPreferences { get; set; }

        /// <summary>
        /// Gets or sets the open my report.
        /// </summary>
        /// <value>The open my report.</value>
        [DataMember]
        public string OpenMyReport { get; set; } //todo: Property som heter open?!?

        /// <summary>
        /// Gets or sets the create text request.
        /// </summary>
        /// <value>The create text request.</value>
        [DataMember]
        public string CreateTextRequest { get; set; } //todo: Property som heter create?!?

        /// <summary>
        /// Gets or sets the create shift trade request.
        /// </summary>
        /// <value>The create shift trade request.</value>
        [DataMember]
        public string CreateShiftTradeRequest { get; set; } //todo: Property som heter create?!?

        /// <summary>
        /// Gets or sets the create absence request.
        /// </summary>
        /// <value>The create absence request.</value>
        [DataMember]
        public string CreateAbsenceRequest { get; set; } //todo: Property som heter create?!?

        /// <summary>
        /// Gets or sets the open scorecard.
        /// </summary>
        /// <value>The open scorecard.</value>
        [DataMember]
        public string OpenScorecard { get; set; } //todo: Property som heter open?!?

        /// <summary>
        /// Gets or sets the create student availability.
        /// </summary>
        /// <value>The create student availability.</value>
        [DataMember]
        public string CreateStudentAvailability { get; set; } //todo: Property som heter create?!?

        /// <summary>
        /// Gets or sets the ViewS chedule Period Calculation.
        /// </summary>
        /// <value>The View Schedule Period Calculation Permission.</value>
        [DataMember]
        public string ViewSchedulePeriodCalculation{ get; set; }

        /// <summary>
        /// Gets or sets the set planning time bank.
        /// </summary>
        /// <value>
        /// The set planning time bank.
        /// </value>
        [DataMember]
        public string SetPlanningTimeBank{ get; set; }
        
        /// <summary>
        /// Gets or sets the view custom team schedule
        /// </summary>
        /// <value>
        /// The view custom team schedule.
        /// </value>
        [DataMember]
        public string ViewCustomTeamSchedule { get; set; }
        
    }
}