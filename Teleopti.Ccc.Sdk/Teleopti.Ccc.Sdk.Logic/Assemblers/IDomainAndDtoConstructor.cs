namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public interface IDomainAndDtoConstructor<TDo, TDto>
    {
        TDo CreateNewDomainObject();
        TDto CreateNewDto();
    }
}