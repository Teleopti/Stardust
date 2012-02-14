using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public class DeletedDescription
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public Description AppendDeleted(Description description)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(" <");
            stringBuilder.Append(UserTexts.Resources.Deleted);
            stringBuilder.Append(">");

            var allowedLength = Description.MaxLengthOfName - stringBuilder.Length;
            string name = description.Name;
            if (name.Length > allowedLength)
            {
                name = name.Substring(0, allowedLength);
            }
            stringBuilder.Insert(0, name);

            return new Description(stringBuilder.ToString(), description.ShortName);
        }
    }
}