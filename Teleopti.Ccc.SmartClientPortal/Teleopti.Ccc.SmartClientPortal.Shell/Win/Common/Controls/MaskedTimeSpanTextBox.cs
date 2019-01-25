using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls
{
    public partial class MaskedTimeSpanTextBox : BaseUserControl
    {
        private TimeSpan _maximumValue = new TimeSpan(24, 0, 0);
        private TimeSpan _minimumValue = new TimeSpan(0, 0, 0);
        private bool _defaultInterpretAsMinutes;
        private ValidatedStatus _validatedStatus = ValidatedStatus.Ok;
        private readonly Icon _warningIcon;
        private readonly Icon _errorIcon;
        private readonly Size IconSize = new Size(16, 16);
        private TimeSpan _value;

        public MaskedTimeSpanTextBox()
        {
            InitializeComponent();

            _warningIcon = IconConverter.ConvertIcon(SystemIcons.Warning, IconSize);
            _errorIcon = Icon.FromHandle(errorProvider1.Icon.Handle);
        }

        public TimeSpan Value
        {
            get
            {
                _value = ValidateInput();
                return _value;
            }
            set { _value = value; }
        }

        private TimeSpan ValidateInput()
        {
           TimeSpan timeValue;
           if (!TimeHelper.TryParseLongHourStringDefaultInterpretation(maskedTextBox1.Text, _maximumValue, out timeValue, TimeFormatsType.HoursMinutes, _defaultInterpretAsMinutes))
           {
               SetErrorStatus(UserTexts.Resources.IllegalInput);
           }
           else
           {
               SetOkStatus();
           }
            var culture = TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.UICulture; 
           if (timeValue < _minimumValue)
               SetErrorStatus(string.Format(culture, UserTexts.Resources.MinimumIsParameterHoursDot, _minimumValue.TotalHours));
           else if (timeValue > _maximumValue)
               SetErrorStatus(string.Format(culture, UserTexts.Resources.MaximumIsParameterHoursDot, _maximumValue.TotalHours));
            return timeValue;
        }

        public ValidatedStatus ValidatedStatus
        {
            get
            {
                ValidateInput();
                return _validatedStatus;
            }
        }

        public void SetErrorStatus(string errorStatus)
        {
            _validatedStatus = ValidatedStatus.Error;
            errorProvider1.Icon = _errorIcon;
            errorProvider1.SetIconPadding(maskedTextBox1, 4);
            errorProvider1.SetError(maskedTextBox1, errorStatus);
        }

        public void SetOkStatus()
        {
            _validatedStatus = ValidatedStatus.Ok;
            errorProvider1.Clear();
        }

        public override bool HasHelp
        {
            get { return false; }
        }
        
        [Browsable(true)]
        public TimeSpan MaximumValue
        {
            get { return _maximumValue; }
            set { _maximumValue = value; }
        }

        [Browsable(true)]
        public TimeSpan MinimumValue
        {
            get { return _minimumValue; }
            set { _minimumValue = value; }
        }

        [Browsable(true)]
        public int TimeSpanBoxWidth
        {
            get { return maskedTextBox1.Width; }
            set
            {
                var iconWidthWithMargin = IconSize.Width + errorProvider1.GetIconPadding(maskedTextBox1);
                Width = value + iconWidthWithMargin;
                maskedTextBox1.Width = value;
            }
        }

        [Browsable(true)]
        public int TimeSpanBoxHeight
        {
            get { return maskedTextBox1.Height; }
            set
            {
                var iconHeightWithMargin = IconSize.Height + errorProvider1.GetIconPadding(maskedTextBox1);
                if (value < iconHeightWithMargin)
                    Height = iconHeightWithMargin;
                maskedTextBox1.Height = Height;
            }
        }

        [Browsable(true)]
        public bool DefaultInterpretAsMinutes
        {
            get { return _defaultInterpretAsMinutes; }
            set { _defaultInterpretAsMinutes = value; }
        }

        [Browsable(true)]
        public HorizontalAlignment AlignTextBoxText
        {
            get { return maskedTextBox1.TextAlign; }
            set { maskedTextBox1.TextAlign = value; }
        }
        
        public event EventHandler<EventArgs> TimeSpanChanged;

        public void InvokeTimeSpanChanged(EventArgs e)
        {
            var handler = TimeSpanChanged;
            if (handler != null) handler(this, e);
        }

        private void maskedTextBox1_TextChanged(object sender, EventArgs e)
        {
            InvokeTimeSpanChanged(e);
        }
    }

    public enum ValidatedStatus
    {
        Ok, Warning, Error
    }

    public static class IconConverter
    {
        public static Icon ConvertIcon(Icon icon, Size size)
        {
            if (icon == null) throw new ArgumentNullException("icon");
            using (var image = new Bitmap(size.Width, size.Height))
            {
                var g = Graphics.FromImage(image);

                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(icon.ToBitmap(), 0, 0, size.Width, size.Height);
                g.Flush();

                return Icon.FromHandle(image.GetHicon());
            }
        }
    }
}
