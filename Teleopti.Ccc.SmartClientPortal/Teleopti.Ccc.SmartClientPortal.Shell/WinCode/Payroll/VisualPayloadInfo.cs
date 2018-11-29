using System;
using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll
{
    public class VisualPayloadInfo
    {
        private readonly DateTime _startTime;
        private readonly DateTime _endTime;
        private readonly Color _color;
        private readonly string _name;
        private readonly string _value;
        private readonly DateTimePeriod _originalDateTimePeriod;
        private Rectangle _bounds;
        private string _shortName;

        /// <summary>
        /// Gets the start time.
        /// </summary>
        /// <value>The start time.</value>
        public DateTime StartTime
        {
            get { return _startTime; }
        }

        /// <summary>
        /// Gets the end time.
        /// </summary>
        /// <value>The end time.</value>
        public DateTime EndTime
        {
            get { return _endTime; }
        }

        /// <summary>
        /// Gets the original date time period.
        /// </summary>
        /// <value>The original date time period.</value>
        public DateTimePeriod OriginalDateTimePeriod
        {
            get
            {
                return _originalDateTimePeriod;
            }
        }

        /// <summary>
        /// Gets the color.
        /// </summary>
        /// <value>The color.</value>
        public Color Color
        {
            get { return _color; }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return _name; }
        }

        public string ShortName
        {
            get{ return _shortName; }
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public string Value
        {
            get
            {
                return _value;
            }
        }

        public Rectangle Bounds
        {
            get { return _bounds; }
            set { _bounds = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualPayloadInfo"/> struct.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <param name="color">The color.</param>
        /// <param name="name">The name.</param>
        /// <param shortName="name">The Short Name.</param>
        /// <param name="value">The value.</param>
        /// <param name="originalDateTimePeriod">The original date time period.</param>
        public VisualPayloadInfo(DateTime startTime, DateTime endTime, Color color, string name, string shortName, string value, DateTimePeriod originalDateTimePeriod)
        {
            _startTime = startTime;
            _endTime = endTime;
            _color = color;
            _name = name;
            _shortName = shortName;
            _value = value;
            _originalDateTimePeriod = originalDateTimePeriod;
            _bounds = new Rectangle();
        }

        public VisualPayloadInfo()
        {}
    }
}
