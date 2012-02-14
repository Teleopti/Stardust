using System;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Template
{
    /// <summary>
    /// Class containing information about references to templates
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-06-09
    /// </remarks>
    public class TemplateReference : ITemplateReference
    {
        public const string LongtermTemplateKey = "<LONGTERM>";
        protected const string TemplateNameFormat = "<{0}>";
        private string _templateName;
        private Guid _templateId;
        private int _versionNumber;
        private int? _dayOfWeek;
    	private DateTime _updatedDate = DateTime.UtcNow;

    	private DayOfWeek? dayOfWeek
        {
            get
            {
                if (!_dayOfWeek.HasValue) return null;
                return (DayOfWeek)_dayOfWeek.Value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateReference"/> struct.
        /// </summary>
        /// <param name="templateId">The template id.</param>
        /// <param name="versionNumber">The version number.</param>
        /// <param name="templateName">Name of the template.</param>
        /// <param name="dayOfWeek">The day of week.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-09
        /// </remarks>
        public TemplateReference(Guid templateId, int versionNumber, string templateName, DayOfWeek? dayOfWeek)
        {
            _versionNumber = versionNumber;
            _templateId = templateId;
            _templateName = templateName;
            if (dayOfWeek.HasValue)
                _dayOfWeek = (int)dayOfWeek.Value;
            else
                _dayOfWeek = null;
        }
        /// <summary>
        /// For NHibernate, and for creating a "broken" reference
        /// </summary>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-10-22
        /// </remarks>
        public TemplateReference()
        {
            // should references be "broken" to start with, or just blank?
            //_templateName = "<NONE>";
        }

        /// <summary>
        /// Gets or sets the name of the template.
        /// </summary>
        /// <value>The name of the template.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-09
        /// </remarks>
        public virtual string TemplateName
        {
            get
            {
                return DisplayName(dayOfWeek, _templateName, false);
            }
            set { _templateName = value; }
        }

    	public virtual DateTime UpdatedDate
    	{
			get { return _updatedDate; }
			set { _updatedDate = value; }
    	}

    	/// <summary>
        /// Gets the name of the template.
        /// </summary>
        /// <param name="weekday">The week day.</param>
        /// <param name="name">The name.</param>
        /// <param name="old">if set to <c>true</c> [old].</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-12
        /// </remarks>
        public string DisplayName(DayOfWeek? weekday, string name, bool old)
        {
            if (!weekday.HasValue) return name;

            string presentedName = CultureInfo.CurrentUICulture.DateTimeFormat.GetAbbreviatedDayName(weekday.Value)
                .ToUpper(CultureInfo.CurrentUICulture);
            string oldPrefix = "OLD";

            if(old) {
                return string.Format(CultureInfo.CurrentUICulture, "<{0}{1}>", oldPrefix, presentedName);
            }
            return string.Format(CultureInfo.CurrentUICulture, "<{0}>", presentedName);
        }

        /// <summary>
        /// Gets or sets the template id.
        /// </summary>
        /// <value>The template id.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-09
        /// </remarks>
        public Guid TemplateId
        {
            get { return _templateId; }
            set { _templateId = value; }
        }

        /// <summary>
        /// Gets the day of week.
        /// </summary>
        /// <value>The day of week.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-12
        /// </remarks>
        public DayOfWeek? DayOfWeek
        {
            get { return dayOfWeek; }
            set
            {
                if (value.HasValue)
                    _dayOfWeek = (int)value.Value;
                else
                    _dayOfWeek = null;
            }
        }

        /// <summary>
        /// Gets or sets the version number.
        /// </summary>
        /// <value>The version number.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-10-16
        /// </remarks>
        public int VersionNumber
        {
            get { return _versionNumber;  }
            set { _versionNumber = value;}
        }

        #region IEquatable<TemplateReference> Members

        ///<summary>
        ///Indicates whether the current object is equal to another object of the same type.
        ///</summary>
        ///
        ///<returns>
        ///true if the current object is equal to the other parameter; otherwise, false.
        ///</returns>
        ///
        ///<param name="other">An object to compare with this object.</param>
        public bool Equals(ITemplateReference other)
        {
            return (TemplateName == other.TemplateName &&
                    TemplateId == other.TemplateId &&
                    VersionNumber == other.VersionNumber &&
                    //Old == other.Old &&
                    DayOfWeek == other.DayOfWeek);
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>
        /// true if obj and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            ITemplateReference templateReference = obj as TemplateReference;
            if (templateReference == null) return false;

            return Equals(templateReference);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            return (_templateName == null ? 1 : _templateName.GetHashCode()) ^ _templateId.GetHashCode() ^
                    (_dayOfWeek == null ? 1 : _dayOfWeek.GetHashCode());
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="templateReference1">The template reference1.</param>
        /// <param name="templateReference2">The template reference2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(TemplateReference templateReference1, TemplateReference templateReference2)
        {
            return templateReference1.Equals(templateReference2);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="templateReference1">The template reference1.</param>
        /// <param name="templateReference2">The template reference2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(TemplateReference templateReference1, TemplateReference templateReference2)
        {
            return !templateReference1.Equals(templateReference2);
        }

        #endregion

        static protected string TrimNameDecorations(string name)
        {
            if(name == null)
                return null;
            return name.TrimStart('<').TrimEnd('>');
        }
    }
}