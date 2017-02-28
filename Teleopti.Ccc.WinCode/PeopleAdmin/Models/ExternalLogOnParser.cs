using System;
using System.Collections.Generic;
using System.Text;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Models
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
            StringBuilder personExternalLogOnDisplayString = new StringBuilder();

            if (externalLogOns != null)
            {
                foreach (IExternalLogOn externalLogOn in externalLogOns)
                {
                    if (personExternalLogOnDisplayString.Length > 0)
                        personExternalLogOnDisplayString.Append(", ");
                    
                    personExternalLogOnDisplayString.Append(_externalLogOnDisplay.Description(externalLogOn));
                }
            }

            return personExternalLogOnDisplayString.ToString();
        }
    }
}