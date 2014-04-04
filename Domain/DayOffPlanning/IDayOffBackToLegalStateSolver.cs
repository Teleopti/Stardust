namespace Teleopti.Ccc.DayOffPlanning
{
    public interface IDayOffBackToLegalStateSolver
    {
        MinMaxNumberOfResult ResolvableState();
        bool SetToManyBackToLegalState();
        bool SetToFewBackToLegalState();
        string ResolverDescriptionKey { get; }
    }
}