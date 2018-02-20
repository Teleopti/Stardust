namespace Teleopti.Ccc.Domain.DayOffPlanning
{
    public interface IDayOffBackToLegalStateSolver
    {
        MinMaxNumberOfResult ResolvableState();
        bool SetToManyBackToLegalState();
        bool SetToFewBackToLegalState();
    }
}