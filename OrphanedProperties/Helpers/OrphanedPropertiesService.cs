using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.DataAbstraction;
using EPiServer.DataAbstraction.RuntimeModel;
using EPiServer.Framework.Localization;
using OrphanedProperties.Models;
using PropertyDefinition = EPiServer.DataAbstraction.PropertyDefinition;

namespace OrphanedProperties.Helpers
{
    public class OrphanedPropertiesService : IOrphanedPropertiesService
    {
        private readonly IContentTypeModelRepository _contentTypeModelRepository;
        private readonly IPropertyDefinitionRepository _propertyDefinitionRepository;
        private readonly IContentTypeRepository _contentTypeRepository;
        private readonly LocalizationService _localizationService;

        public OrphanedPropertiesService(
            IContentTypeModelRepository contentTypeModelRepository,
            IPropertyDefinitionRepository propertyDefinitionRepository,
            IContentTypeRepository contentTypeRepository,
            LocalizationService localizationService)
        {
            _contentTypeModelRepository = contentTypeModelRepository;
            _propertyDefinitionRepository = propertyDefinitionRepository;
            _contentTypeRepository = contentTypeRepository;
            _localizationService = localizationService;
        }

        public IList<OrphanedPropertyResult> GetMissingProperties(bool showFormProperties)
        {
            var results = new List<OrphanedPropertyResult>();
            var excludedPrefixes = new[]
            {
                "episerver.",
                "seoboost.",
                "geta.",
                "a2z.",
                "AdvancedTaskManager."
            };

            foreach (var type in _contentTypeRepository.List())
            {
                var modelType = type.ModelTypeString;
                if (!string.IsNullOrWhiteSpace(modelType) &&
                    excludedPrefixes.Any(p =>
                        modelType.StartsWith(p, StringComparison.InvariantCultureIgnoreCase)))
                {
                    continue;
                }

                var defaultCategory = GetContentTypeCategory(type);

                var properties = type.PropertyDefinitions
                    .Where(IsMissingModelProperty);

                if (!showFormProperties)
                {
                    properties = properties
                        .Where(p => !IsFormsSystemProperty(p));
                }

                foreach (var property in properties)
                {
                    var isFormsProperty = IsFormsSystemProperty(property);

                    results.Add(new OrphanedPropertyResult
                    {
                        TypeName = _localizationService.GetContentTypeName(type),
                        PropertyName = property.Name,
                        PropertyId = property.ID,
                        TypeId = type.ID,
                        IsBlockType = defaultCategory.Equals(
                            "Block",
                            StringComparison.OrdinalIgnoreCase),

                        Summary = $"{property.Name} (Type: {_localizationService.GetContentTypeName(type)})",

                        // 🔑 force Forms category
                        Category = isFormsProperty
                            ? "Form"
                            : defaultCategory
                    });
                }
            }

            return results
                .OrderBy(x => x.Category)
                .ThenBy(x => x.TypeName)
                .ToList();
        }


        private bool IsMissingModelProperty(PropertyDefinition propertyDefinition)
        {
            if (propertyDefinition == null)
                return false;

            if (propertyDefinition.ExistsOnModel)
                return false;

            if (propertyDefinition.Name.StartsWith(
                    "atm_",
                    StringComparison.InvariantCultureIgnoreCase))
                return false;

            return _contentTypeModelRepository
                .GetPropertyModel(propertyDefinition.ContentTypeID, propertyDefinition) == null;
        }

        private bool IsFormsSystemProperty(PropertyDefinition propertyDefinition)
        {
            if (propertyDefinition == null)
                return false;

            var contentType = _contentTypeRepository.Load(propertyDefinition.ContentTypeID);
            if (contentType == null)
                return false;

            var modelType = contentType.ModelTypeString;
            var propertyName = propertyDefinition.Name;

            // 1. Any Optimizely Forms content type
            if (!string.IsNullOrWhiteSpace(modelType) &&
                modelType.StartsWith("EPiServer.Forms", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            // 2. FormContainerBlock and any derived container
            if (!string.IsNullOrWhiteSpace(modelType) &&
                modelType.Contains("FormContainerBlock", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            // 3. Custom Form Containers detected by system property set
            if (contentType.Base == ContentTypeBase.Block)
            {
                var propertyNames = contentType.PropertyDefinitions
                    .Select(p => p.Name)
                    .ToHashSet(StringComparer.InvariantCultureIgnoreCase);

                if (propertyNames.Contains("Form__Elements") &&
                    propertyNames.Contains("Form__Settings") &&
                    propertyNames.Contains("Form__Language"))
                {
                    return true;
                }
            }

            // 4. Forms technical property prefixes
            return !string.IsNullOrWhiteSpace(propertyName) && (propertyName.StartsWith("Form_", StringComparison.InvariantCultureIgnoreCase) ||
                                                                propertyName.StartsWith("Forms_", StringComparison.InvariantCultureIgnoreCase) ||
                                                                propertyName.StartsWith("Element_", StringComparison.InvariantCultureIgnoreCase) ||
                                                                propertyName.StartsWith("System_", StringComparison.InvariantCultureIgnoreCase) ||
                                                                propertyName.StartsWith("Visitor_", StringComparison.InvariantCultureIgnoreCase) ||
                                                                propertyName.StartsWith("Submission_", StringComparison.InvariantCultureIgnoreCase));
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
                var contentType = _contentTypeRepository.Load(propertyDefinition.ContentTypeID);
                var writableContentType = contentType.CreateWritableClone() as ContentType;
                var writableProperty = writableContentType.PropertyDefinitions
                    .FirstOrDefault(p => p.ID == propertyId);

                if (writableProperty != null)
                {
                    writableContentType.PropertyDefinitions.Remove(writableProperty);
                }

                _contentTypeRepository.Save(writableContentType);
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
