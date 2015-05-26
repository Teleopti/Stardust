namespace Teleopti.Analytics.Etl.ConfigTool.Code.Gui.DataSourceConfiguration
{
    public class MessageBoxProperties
    {
        public MessageBoxProperties(bool isQuestion, string text, string caption)
        {
            IsQuestion = isQuestion;
            Text = text;
            Caption = caption;
        }

        public bool IsQuestion { get; private set; }
        public string Text { get; private set; }
        public string Caption { get; private set; }
    }
}
