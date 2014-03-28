namespace Teleopti.Ccc.Domain.Secret
{
    public interface IDayOffBackToLegalStateSolver
    {
        MinMaxNumberOfResult ResolvableState();
        bool SetToManyBackToLegalState();
        bool SetToFewBackToLegalState();
        string ResolverDescriptionKey { get; }
    }
}