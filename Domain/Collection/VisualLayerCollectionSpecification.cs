﻿using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Collection
{

    public static class VisualLayerCollectionSpecification
    {
        private static readonly ISpecification<IVisualLayerCollection> _oneAbsenceLayer = new OneAbsenceLayerSpecification();

        public static ISpecification<IVisualLayerCollection> OneAbsenceLayer
        {
            get
            {
                return _oneAbsenceLayer;
            }
        }

        public static ISpecification<IVisualLayerCollection> IdenticalLayers(IVisualLayerCollection collectionToCompare)
        {
            return new IdenticalLayersSpecification(collectionToCompare);
        }
    }
}
