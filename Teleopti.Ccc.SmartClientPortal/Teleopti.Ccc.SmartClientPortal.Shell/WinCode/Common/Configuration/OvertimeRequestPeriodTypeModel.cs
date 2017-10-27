using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration
{
	public class OvertimeRequestPeriodTypeModel
	{
		private readonly IOvertimeRequestOpenPeriod _item;

		public OvertimeRequestPeriodTypeModel(IOvertimeRequestOpenPeriod item, string displayText)
		{
			_item = item;
			DisplayText = displayText;
		}

		public IOvertimeRequestOpenPeriod Item => _item.NoneEntityClone();

		public string DisplayText { get; internal set; }

		public override bool Equals(object obj)
		{
			OvertimeRequestPeriodTypeModel model = obj as OvertimeRequestPeriodTypeModel;
			if (model == null) return false;

			return model.Item.GetType() == _item.GetType();
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((_item != null ? _item.GetHashCode() : 0) * 397) ^ (DisplayText != null ? DisplayText.GetHashCode() : 0);
			}
		}
	}
}