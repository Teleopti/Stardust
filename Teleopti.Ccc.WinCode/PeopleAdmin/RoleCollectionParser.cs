using System;
using System.Collections.Generic;
using System.Text;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.PeopleAdmin
{
    public class RoleCollectionParser
    {
        private readonly IEnumerable<IApplicationRole> _roleCollection;
        private readonly IRoleDisplay _roleDisplay;
        
        public RoleCollectionParser(IEnumerable<IApplicationRole> roleCollection, IRoleDisplay roleDisplay)
        {
            _roleCollection = roleCollection;
            _roleDisplay = roleDisplay;
        }

        public IEnumerable<IApplicationRole> ParseRoleCollection(string value)
        {
            if (string.IsNullOrEmpty(value)) return new List<IApplicationRole>(0);

            Char[] separator = { ',' };
            string[] roleNameCollection = value.Split(separator);

            HashSet<IApplicationRole> roleCollection = new HashSet<IApplicationRole>();
            for (int i = 0; i < roleNameCollection.Length; i++)
            {
                foreach (var role in _roleCollection)
                {
                    if (_roleDisplay.Description(role) == roleNameCollection[i].Trim())
                    {
                        roleCollection.Add(role);
                    }
                }
            }
            return roleCollection;
        }

        public string GetRoleCollectionDisplayText(IEnumerable<IApplicationRole> roleCollection)
        {
            StringBuilder roleCollectionDisplayString = new StringBuilder();

            if (roleCollection != null)
            {
                foreach (IApplicationRole role in roleCollection)
                {
                    if (roleCollectionDisplayString.Length > 0)
                        roleCollectionDisplayString.Append(", ");

                    roleCollectionDisplayString.Append(_roleDisplay.Description(role));
                }
            }

            return roleCollectionDisplayString.ToString();
        }
    }
}