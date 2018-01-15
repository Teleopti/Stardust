using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models
{
    public class ExternalLogOnParser
    {
        private readonly IEnumerable<IExternalLogOn> _externalLogOnCollection;
        private readonly ExternalLogOnDisplay _externalLogOnDisplay;

        public ExternalLogOnParser(IEnumerable<IExternalLogOn> externalLogOnCollection, ExternalLogOnDisplay externalLogOnDisplay)
        {
            _externalLogOnCollection = externalLogOnCollection;
            _externalLogOnDisplay = externalLogOnDisplay;
        }

        public IEnumerable<IExternalLogOn> ParsePersonExternalLogOn(string value)
        {
            if (string.IsNullOrEmpty(value)) return new List<IExternalLogOn>(0);

            Char[] separator = { ',' };
            string[] personExternalLogOnNameCollection = value.Split(separator);

            HashSet<IExternalLogOn> externalLogOns = new HashSet<IExternalLogOn>();
            for (int i = 0; i < personExternalLogOnNameCollection.Length; i++)
            {
                foreach (var externalLogOn in _externalLogOnCollection)
                {
                    if (_externalLogOnDisplay.Description(externalLogOn) == personExternalLogOnNameCollection[i].Trim())
                    {
                        externalLogOns.Add(externalLogOn);
                    }
                }
            }
            return externalLogOns;
        }

        public string GetExternalLogOnsDisplayText(IEnumerable<IExternalLogOn> externalLogOns)
        {
            if (externalLogOns != null)
			{
				return string.Join(", ", externalLogOns.Select(externalLogOn => _externalLogOnDisplay.Description(externalLogOn)));
			}

            return string.Empty;
        }
    }
}