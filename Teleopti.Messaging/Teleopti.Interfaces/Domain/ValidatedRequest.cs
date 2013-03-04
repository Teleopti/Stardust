namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// 
    /// </summary>
    public class ValidatedRequest:IValidatedRequest
    {
        public string ValidationErrors { get; set; }
        public bool IsValid { get; set; }
    }
}
