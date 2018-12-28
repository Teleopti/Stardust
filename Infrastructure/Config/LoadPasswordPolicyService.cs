using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security;

namespace Teleopti.Ccc.Infrastructure.Config
{
    public class LoadPasswordPolicyService : ILoadPasswordPolicyService
    {
        public string FileName {get;} = "PasswordPolicy.xml";

		private XDocument _file;
    	private const int _defaultMaxNumberOfAttempts = 3;
    	private const int _defaultInvalidAttemptWindow = 0;
    	private const int _defaultPasswordValidForDayCount = int.MaxValue;
    	private const int _defaultPasswordExpireWarningDayCount = 0;
    	private readonly ILog _logger = LogManager.GetLogger(typeof(LoadPasswordPolicyService));
	    private const int _defaultMinAccepted = 1;
		private Action _reset = null;

	    public XDocument File => LoadFile();

		public LoadPasswordPolicyService(XDocument file)
        {
            _file = file;
        }

        public LoadPasswordPolicyService(string path)
        {
            Path = path;
        }

        public TimeSpan LoadInvalidAttemptWindow()
        {
            var retValue = GetValueFromPasswordPolicyDocument("InvalidAttemptWindow",
                                                    _defaultInvalidAttemptWindow);

            return TimeSpan.FromMinutes(retValue);


        }

        public int LoadMaxAttemptCount()
        {
            return GetValueFromPasswordPolicyDocument("MaxNumberOfAttempts",
                                                    _defaultMaxNumberOfAttempts);
        }

        public int LoadPasswordValidForDayCount()
        {
            return GetValueFromPasswordPolicyDocument("PasswordValidForDayCount",
                                                     _defaultPasswordValidForDayCount);
        }


        public int LoadPasswordExpireWarningDayCount()
        {
            return GetValueFromPasswordPolicyDocument("PasswordExpireWarningDayCount",
                                                      _defaultPasswordExpireWarningDayCount);
        }


        public IList<IPasswordStrengthRule> LoadPasswordStrengthRules()
        {
            try
            {
                IEnumerable<IPasswordStrengthRule> rules = from rule in LoadFile().Descendants("Rule")
                                                           where rule.Attribute("MinAccepted") != null
                                                           select new PasswordStrengthRule(
                                                               (int)rule.Attribute("MinAccepted"),
                                                               (
                                                               from passwordStrengthSpec in rule.Descendants("PasswordStrengthRule")
                                                               where passwordStrengthSpec.Attribute("RegEx") != null
                                                               select new RegExpSpecification(passwordStrengthSpec.Attribute("RegEx").Value) as ISpecification<string>).ToList()
                                                               ) as IPasswordStrengthRule;


                return rules.ToList();
            }

            catch (FormatException)
            {

                return new List<IPasswordStrengthRule>();
            }


        }

        public string Path { get; } = string.Empty;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private int GetValueFromPasswordPolicyDocument(string xPath, int defaultValue)
        {
            try
            {
                var elements = from c in LoadFile().Descendants("PasswordPolicy")
                               select (int)c.Attribute(xPath);

                return elements.First();
            }
            catch (Exception e) //Catch any exception 
            {
				_logger.Error("Error when getting value from document.",e);
                return defaultValue;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private XDocument LoadFile()
        {
            if (_file == null)
            {
                try
                {
	                var policy = System.IO.Path.Combine(Path, FileName);
					var exists = System.IO.File.Exists(policy);
					_logger.DebugFormat("Policy exists at {1}: {0}",exists,policy);
					_file = exists ? XDocument.Load(policy) : defaultXDocument();
                }
                catch (Exception e)
                {
					_logger.Error("Error when loading policy.", e);
                    _logger.Info("Default password policy will be used:  " + e.Message);
                    return defaultXDocument();
                }

            }
            return _file;
        }

	    public void Reset()
	    {
		    _reset?.Invoke();
			_reset = null;
		}

	    public string DocumentAsString => File.ToString();

		private static XDocument defaultXDocument()
		{
			return new XDocument(
			 new XDeclaration("1.0", "utf-8", "yes"),
			 new XComment("Default config data"),
			 new XElement("PasswordPolicy",
				 new XAttribute("MaxNumberOfAttempts", _defaultMaxNumberOfAttempts),
				 new XAttribute("InvalidAttemptWindow", _defaultInvalidAttemptWindow),
				 new XAttribute("PasswordValidForDayCount", _defaultPasswordValidForDayCount),
				 new XAttribute("PasswordExpireWarningDayCount", _defaultPasswordExpireWarningDayCount),
				 new XElement("Rule" , new XAttribute("MinAccepted", _defaultMinAccepted),
					 new XElement("PasswordStrengthRule",  new XAttribute("RegEx" ,".{1,}" ))
				 ))
			  );
		}

		public void UseMinLengthRule()
		{
			var currentFile = _file;
			_reset = () => _file = currentFile;

			_file = new XDocument(
				new XDeclaration("1.0", "utf-8", "yes"),
				new XComment("Default config data"),
				new XElement("PasswordPolicy",
					new XAttribute("MaxNumberOfAttempts", 3),
					new XAttribute("InvalidAttemptWindow", 30),
					new XAttribute("PasswordValidForDayCount", 30),
					new XAttribute("PasswordExpireWarningDayCount", 3),
					new XElement("Rule", new XAttribute("MinAccepted", 1),
						new XElement("PasswordStrengthRule", new XAttribute("RegEx", ".{8,}"))
					))
			);
		}
    }
}
