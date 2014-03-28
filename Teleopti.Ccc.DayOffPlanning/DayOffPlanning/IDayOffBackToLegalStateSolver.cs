namespace Teleopti.Ccc.Domain.Secret.DayOffPlanning
{
    public interface IDayOffBackToLegalStateSolver
    {
        MinMaxNumberOfResult ResolvableState();
        bool SetToManyBackToLegalState();
        bool SetToFewBackToLegalState();
        string ResolverDescriptionKey { get; }
    }
}