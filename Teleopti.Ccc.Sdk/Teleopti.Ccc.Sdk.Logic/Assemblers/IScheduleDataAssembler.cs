using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public interface IScheduleDataAssembler<TDo, TDto> : IAssembler<TDo, TDto>
    {
        IPerson Person { get; set; }
        IScenario DefaultScenario { get; set; }
        DateOnly PartDate { get; set; }
    }
}