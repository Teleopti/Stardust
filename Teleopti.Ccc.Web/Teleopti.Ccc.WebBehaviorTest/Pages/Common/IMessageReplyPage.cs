using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages.Common
{
    public interface IMessageReplyPage
    {
        Div MessageDetailSection { get; }
        Label Title { get; }
        Label Message { get; }
    }
}