﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin
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
            if (roleCollection != null)
			{
				return string.Join(", ", roleCollection.Select(role => _roleDisplay.Description(role)));
            }

            return string.Empty;
        }
    }
}