using System.Collections.Generic;
using OrphanedProperties.Models;

namespace OrphanedProperties.Helpers
{
    public interface IOrphanedPropertiesService
    {
        IList<OrphanedPropertyResult> GetMissingProperties(bool showFormProperties);
        DeletePropertyResponse ProcessProperty(int propertyId);
    }
}
