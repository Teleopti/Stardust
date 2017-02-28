namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// 
    /// </summary>
    public interface IValidatedRequest
    {
        string ValidationErrors { get; set; }
        bool IsValid { get; set; }
		PersonRequestDenyOption? DenyOption { get; set; }
	}
}
