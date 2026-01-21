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

        public OrphanedPropertiesService(
            ContentTypeModelRepository contentTypeModelRepository,
            IPropertyDefinitionRepository propertyDefinitionRepository,
            IContentTypeRepository contentTypeRepository)
        {
            _contentTypeModelRepository = contentTypeModelRepository;
            _propertyDefinitionRepository = propertyDefinitionRepository;
            _contentTypeRepository = contentTypeRepository;
        }

        public IList<OrphanedPropertyResult> GetMissingProperties()
        {
            var results = new List<OrphanedPropertyResult>();
            var excludedPrefixes = new[] { "episerver.", "seoboost.", "geta.", "a2z.", "AdvancedTaskManager." };

            foreach (var type in _contentTypeRepository.List())
            {
                var modelType = type.ModelTypeString;
                if (!string.IsNullOrWhiteSpace(modelType) &&
                    excludedPrefixes.Any(p => modelType.StartsWith(p, StringComparison.InvariantCultureIgnoreCase)))
                {
                    continue;
                }

                var category = GetContentTypeCategory(type);

                foreach (var property in type.PropertyDefinitions.Where(IsMissingModelProperty))
                {
                    results.Add(new OrphanedPropertyResult
                    {
                        TypeName = type.LocalizedName,
                        PropertyName = property.Name,
                        PropertyId = property.ID,
                        TypeId = type.ID,
                        IsBlockType = category.Equals("Block", StringComparison.OrdinalIgnoreCase),
                        Summary = $"{property.Name} (Type: {type.LocalizedName})",
                        Category = category // new optional property if you want to display this in the view
                    });
                }
            }

            return results.OrderBy(x => x.TypeName).ToList();
        }

        private bool IsMissingModelProperty(PropertyDefinition propertyDefinition)
        {
            if (propertyDefinition == null) return false;
            if (propertyDefinition.ExistsOnModel) return false;
            if (propertyDefinition.Name.StartsWith("atm_", StringComparison.InvariantCultureIgnoreCase)) return false;

            return _contentTypeModelRepository.GetPropertyModel(propertyDefinition.ContentTypeID, propertyDefinition) == null;
        }

        private static string GetContentTypeCategory(ContentType contentType)
        {
            return contentType != null ? GetContentTypeName(contentType.Base) : "Unknown";
        }

        private static string GetContentTypeName(ContentTypeBase baseType)
        {
            if (baseType == ContentTypeBase.Page)
                return "Page";

            if (baseType == ContentTypeBase.Block)
                return "Block";

            if (baseType == ContentTypeBase.Folder)
                return "Folder";

            if (baseType == ContentTypeBase.Media)
                return "Media";

            if (baseType == ContentTypeBase.Image)
                return "Image";

            if (baseType == ContentTypeBase.Video)
                return "Video";

            if (baseType == ContentTypeBase.Undefined)
                return "Undefined";

            return "Unknown";
        }

        public DeletePropertyResponse ProcessProperty(int propertyId)
        {
            var result = new DeletePropertyResponse { Status = "Good" };

            if (propertyId == 0)
            {
                result.Status = "Error";
                result.Description = "ERROR: Property value is 0.";
                return result;
            }

            var propertyDefinition = _propertyDefinitionRepository.Load(propertyId);
            if (propertyDefinition == null)
            {
                result.Status = "Error";
                result.Description = $"ERROR: Property {propertyId} is null.";
                return result;
            }

            if (!IsMissingModelProperty(propertyDefinition))
            {
                result.Status = "Error";
                result.Description = $"ERROR: {propertyDefinition.Name} ({propertyDefinition.Type.Name}) is not an orphan property.";
                return result;
            }

            try
            {
                var writable = propertyDefinition.CreateWritableClone();
                _propertyDefinitionRepository.Delete(writable);
                result.Status = "Good";
                result.Description = $"SUCCESS: {propertyDefinition.Name} ({propertyDefinition.Type.Name}) was successfully deleted.";
            }
            catch (Exception ex)
            {
                result.Status = "Error";
                result.Description = ex.Message;
            }

            return result;
        }
    }
}
