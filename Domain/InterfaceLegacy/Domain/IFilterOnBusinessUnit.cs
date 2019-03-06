
namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    public interface IFilterOnBusinessUnit
    {
		IBusinessUnit BusinessUnit { get; set; }
		IBusinessUnit GetOrFillWithBusinessUnit_DONTUSE();
	}
}
