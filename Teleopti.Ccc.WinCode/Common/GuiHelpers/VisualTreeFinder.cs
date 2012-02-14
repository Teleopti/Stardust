﻿using System;
using System.Windows;
using System.Windows.Media;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.GuiHelpers
{
    // Henrik:
    // Helperclass for finding elements in the visualtree
    // If you need to find a auto-generated element such as a scrollviewer in a datagrid, otherwise its faster to find it by direct access (using x:Name)
    // Note: Do not overuse, the need to find an element is often a sign of the wrong approach to a gui-problem.....
    public class VisualTreeFinder
    {

        #region ctor
        public VisualTreeFinder():this(new AlwaysSatisfied())
        {
            
        }

        public VisualTreeFinder(ISpecification<Visual> elementMustBeSatisfiedBy)
        {
            _elementMustBeSatisfiedBy = elementMustBeSatisfiedBy;
        }
        #endregion

        #region methods
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        public Visual GetDescendantByType(Visual element, Type type)
        {
            if (element == null) return null;
            if (element.GetType() == type && _elementMustBeSatisfiedBy.IsSatisfiedBy(element)) return element; 
            
            Visual foundElement = null;

            if (element is FrameworkElement)
                (element as FrameworkElement).ApplyTemplate();

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                Visual visual = VisualTreeHelper.GetChild(element, i) as Visual; foundElement = GetDescendantByType(visual, type);
                if (foundElement != null)
                    break;
            }
            return foundElement;
        }

       

        public DependencyObject GetAncestorByType(DependencyObject element, Type type) 
        { 
            if (element == null) return null; 
            if (element.GetType() == type) return element; 
            return GetAncestorByType(VisualTreeHelper.GetParent(element), type); 
        }   
   
        #endregion

        #region Extra Criterias
        private readonly ISpecification<Visual> _elementMustBeSatisfiedBy;

        private class AlwaysSatisfied : Specification<Visual>
        {
            public override bool IsSatisfiedBy(Visual obj)
            {
                return true;
            }
        }
        #endregion
        
    }

}
