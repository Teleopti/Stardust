using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Settings
{
	public class ShiftTradeBusinessRuleConfigView
	{
		private readonly IShiftTradeBusinessRuleConfig _shiftTradeBusinessRuleConfig;
		private ShiftTradeRequestHandleOptionView _shiftTradeRequestHandleOptionView;

		public ShiftTradeBusinessRuleConfigView(IShiftTradeBusinessRuleConfig shiftTradeBusinessRuleConfig)
		{
			_shiftTradeBusinessRuleConfig = shiftTradeBusinessRuleConfig;
		}

		public string Name
		{
			get { return _shiftTradeBusinessRuleConfig.FriendlyName; }
		}

		public bool Enabled
		{
			get { return _shiftTradeBusinessRuleConfig.Enabled; }
			set { _shiftTradeBusinessRuleConfig.Enabled = value; }
		}

		public ShiftTradeRequestHandleOptionView HandleOptionOnFailed
		{
			get { return _shiftTradeRequestHandleOptionView; }
			set
			{
				_shiftTradeRequestHandleOptionView = value;
				if (value != null)
				{
					_shiftTradeBusinessRuleConfig.HandleOptionOnFailed = value.RequestHandleOption;
				}
				else
				{
					_shiftTradeBusinessRuleConfig.HandleOptionOnFailed = null;
				}
				
			}
		}
	}
}
