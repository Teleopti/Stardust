using System;

namespace Teleopti.Ccc.SmartParts.Forecasting
{
    public class NamedEntity
    {
        public string Name { get; set; }
        public Guid Id { get; set; }

        public override bool Equals(object obj)
        {
            var rhs = obj as NamedEntity;
            if (Name.Equals(rhs.Name) && Id.Equals(rhs.Id))
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}