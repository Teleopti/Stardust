namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IShiftTradeBusinessRuleConfig
	{
		string BusinessRuleType { get; set; }

		string FriendlyName { get; set; }

		bool Enabled { get; set; }

		RequestHandleOption? HandleOptionOnFailed { get; set; }

		int Order { get; set; }
	}
}