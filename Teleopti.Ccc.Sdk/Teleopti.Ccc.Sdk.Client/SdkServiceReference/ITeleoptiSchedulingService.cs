namespace Teleopti.Ccc.Sdk.Client.SdkServiceReference
{
    public interface ITeleoptiSchedulingService
    {
        PersonRequestDto SetShiftTradeRequest(PersonRequestDto personRequestDto, string subject, string message, ShiftTradeSwapDetailDto[] shiftTradeSwapDetailDtos);
        PersonRequestDto AcceptShiftTradeRequest(PersonRequestDto personRequestDto);
        PersonRequestDto DenyShiftTradeRequest(PersonRequestDto personRequestDto);
        void DeletePersonRequest(PersonRequestDto personRequestDto);
        PersonRequestDto[] GetAllRequestModifiedWithinPeriodOrPending([System.Xml.Serialization.XmlElement(IsNullable = true)] PersonDto person, System.DateTime utcStartDate, [System.Xml.Serialization.XmlIgnore()] bool utcStartDateSpecified, System.DateTime utcEndDate, [System.Xml.Serialization.XmlIgnore()] bool utcEndDateSpecified);
        PersonRequestDto SavePersonRequest(PersonRequestDto personRequest);
        void SavePersonAbsenceRequest([System.Xml.Serialization.XmlElement(IsNullable=true)] PersonRequestDto personRequestDto);
        ActivityDto[] GetActivities([System.Xml.Serialization.XmlElementAttribute(IsNullable = true)] LoadOptionDto loadOptionDto);
        AbsenceDto[] GetAbsences([System.Xml.Serialization.XmlElementAttribute(IsNullable = true)] AbsenceLoadOptionDto loadOptionDto);
        PlanningTimeBankDto GetPlanningTimeBank(PersonDto personDto, DateOnlyDto dateOnlyDto);
        void SavePlanningTimeBank(PersonDto personDto, DateOnlyDto dateOnlyDto, int balanceOutMinute, [System.Xml.Serialization.XmlIgnore()] bool balanceOutMinuteSpecified);
    }
}