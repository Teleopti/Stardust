using System;

namespace Teleopti.Wfm.Adherence.Configuration
{
    [Serializable]
    public struct Description
    {
        private string _name;
        private string _shortName;
        private const int _nameLength = 50;
        private const int _shortNameLength = 25;

        public Description(string name, string shortName)
        {
            ValidateName(name, shortName);
            _name = name;
            _shortName = shortName;
        }

		public Description(string name) : this(name,null)
        {
        }

        private static void ValidateName(string name, string shortName)
        {
            if (name.Length > _nameLength)
                throw new ArgumentOutOfRangeException(nameof(name), "String too long.");
            if (shortName!=null && shortName.Length > _shortNameLength)
                throw new ArgumentOutOfRangeException(nameof(shortName), "String too long.");
        }

        public string Name
        {
            get
            {
                if (_name == null)
                    _name = string.Empty;
                return _name.Trim();
            }
        }

        public string ShortName
        {
            get
            {
                if (_shortName == null)
                    _shortName = string.Empty;
                return _shortName.Trim();
            }
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Name))
                return ShortName;
            if (string.IsNullOrEmpty(ShortName))
                return Name;
            return string.Concat(ShortName, ", ", Name);
        }

        public bool Equals(Description other)
        {
            return (Name == other.Name && ShortName == other.ShortName);
        }

        public override bool Equals(object obj)
		{
			return obj is Description description && Equals(description);
		}

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ ShortName.GetHashCode();
        }

        public static bool operator ==(Description description1, Description description2)
        {
            return description1.Equals(description2);
        }

        public static bool operator !=(Description description1, Description description2)
        {
            return !description1.Equals(description2);
        }
    }
}