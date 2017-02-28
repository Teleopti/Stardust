using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Configuration
{
    public class AbsenceRequestPeriodTypeModel
    {
        private readonly IAbsenceRequestOpenPeriod _item;

        public AbsenceRequestPeriodTypeModel(IAbsenceRequestOpenPeriod item, string displayText)
        {
            _item = item;
            DisplayText = displayText;
        }

        /// <summary>
        /// Gets the item (always returns a clone!).
        /// </summary>
        /// <value>The item.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2010-04-27
        /// </remarks>
        public IAbsenceRequestOpenPeriod Item
        {
            get
            {
                return _item.NoneEntityClone();
            }
        }

        public string DisplayText { get; internal set; }

        public override bool Equals(object obj)
        {
            AbsenceRequestPeriodTypeModel model = obj as AbsenceRequestPeriodTypeModel;
            if (model==null) return false;

            return model.Item.GetType() == _item.GetType();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_item != null ? _item.GetHashCode() : 0)*397) ^ (DisplayText != null ? DisplayText.GetHashCode() : 0);
            }
        }
    }
}