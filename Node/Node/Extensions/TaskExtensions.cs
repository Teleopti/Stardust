using System.Threading.Tasks;

namespace Stardust.Node.Extensions
{
    public static class TaskExtensions
    {
        public static bool IsNotNull(this Task task)
        {
            return task != null;
        }
    }
}