using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using OrphanedProperties.Models;
using PropertyDefinition = EPiServer.DataAbstraction.PropertyDefinition;

namespace OrphanedProperties.Helpers
{
    public class OrphanedPropertiesService : IOrphanedPropertiesService
    {
        private readonly ContentTypeModelRepository _contentTypeModelRepository;

        private readonly IPropertyDefinitionRepository _propertyDefinitionRepository;
        private readonly IContentTypeRepository _contentTypeRepository;

        public OrphanedPropertiesService(ContentTypeModelRepository contentTypeModelRepository, IPropertyDefinitionRepository propertyDefinitionRepository, IContentTypeRepository contentTypeRepository)
        {
            _contentTypeModelRepository = contentTypeModelRepository;
            _propertyDefinitionRepository = propertyDefinitionRepository;
            _contentTypeRepository = contentTypeRepository;
        }

        public IList<OrphanedPropertyResult> GetMissingProperties()
        {
            var pageProperties = new List<OrphanedPropertyResult>();
            var excludedPrefixes = new[] { "episerver.", "seoboost.", "geta.", "a2z." , "AdvancedTaskManager." };

            foreach (var type in _contentTypeRepository.List())
            {
                var modelType = type.ModelTypeString;
                if (excludedPrefixes.Any(p => modelType.StartsWith(p, StringComparison.InvariantCultureIgnoreCase)))
                {
                    continue;
                }

                pageProperties.AddRange(type.PropertyDefinitions.Where(IsMissingModelProperty)
                .Select(property => new OrphanedPropertyResult
                {
                    TypeName = type.LocalizedName,
                    PropertyName = property.Name,
                    PropertyId = property.ID,
                    TypeId = type.ID,
                    IsBlockType = IsContentTypeBlockType(type),
                    Summary = $"{property.Name} (Type: {type.LocalizedName})"
                }));
            }

            return pageProperties.OrderBy(x => x.TypeName).ToList();
        }

        private bool IsMissingModelProperty(PropertyDefinition propertyDefinition)
        {
            return propertyDefinition != null
                   && !propertyDefinition.ExistsOnModel
                   && _contentTypeModelRepository.GetPropertyModel(propertyDefinition.ContentTypeID,
                       propertyDefinition) == null;
        }

        private static bool IsContentTypeBlockType(ContentType contentType)
        {
            // Exclude local blocks (block property on pages) if the current content type is a block type.
            var modelType = Type.GetType(contentType.ModelTypeString);
            return modelType != null && typeof(BlockData).IsAssignableFrom(modelType);
        }

        public DeletePropertyResponse ProcessProperty(int propertyId)
        {
            var result = new DeletePropertyResponse
            {
                Status = "Good"
            };

            if (propertyId != 0)
            {
                var propertyDefinition = _propertyDefinitionRepository.Load(propertyId);

                if (propertyDefinition != null)
                {
                    if (IsMissingModelProperty(propertyDefinition))
                    {
                        try
                        {
                            var writable = propertyDefinition.CreateWritableClone();
                            _propertyDefinitionRepository.Delete(writable);
                            result.Status = "Good";
                            result.Description = $"SUCCESS: {propertyDefinition.Name + " (" + propertyDefinition.Type.Name + ")"} was successfully deleted.";
                        }
                        catch (Exception ex)
                        {
                            result.Status = "Error";
                            result.Description = ex.Message;
                        }
                    }
                    else
                    {
                        result.Status = "Error";
                        result.Description = $"ERROR: {propertyDefinition.Name + " (" + propertyDefinition.Type.Name + ")"} is not an orphan property.";
                    }
                }
                else
                {
                    result.Status = "Error";
                    result.Description = $"ERROR: Property {propertyId} is null.";
                }
            }
            else
            {
                result.Status = "Error";
                result.Description = "ERROR: Property value is 0.";
            }
            return result;
        }

    }
}
