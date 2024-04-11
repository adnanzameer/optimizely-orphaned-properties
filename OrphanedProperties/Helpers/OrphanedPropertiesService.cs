using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Framework.Localization;
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
            var pageProperties = from type in _contentTypeRepository.List()
                                 from property in type.PropertyDefinitions.Where(property => IsMissingModelProperty(property) && !type.Name.StartsWith("form", StringComparison.InvariantCultureIgnoreCase))
                                 select new OrphanedPropertyResult
                                 {
                                     TypeName = type.LocalizedName,
                                     PropertyName = property.Name,
                                     PropertyId = property.ID,
                                     TypeId = type.ID,
                                     IsBlockType = IsContentTypeBlockType(type),
                                     Summary = $"{property.Name} (Type: {type.LocalizedName})"
                                 };

            return pageProperties.OrderBy(x => x.TypeName).ToList();
        }

        private bool IsMissingModelProperty(PropertyDefinition propertyDefinition)
        {
            return propertyDefinition != null
                   && !propertyDefinition.ExistsOnModel
                   && _contentTypeModelRepository.GetPropertyModel(propertyDefinition.ContentTypeID, propertyDefinition) == null
                   && !propertyDefinition.Name.StartsWith("form", StringComparison.InvariantCultureIgnoreCase)
                   && !propertyDefinition.Type.Name.StartsWith("form", StringComparison.InvariantCultureIgnoreCase);
        }

        private bool IsContentTypeBlockType(ContentType contentType)
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
                            result.Description = string.Format(LocalizationService.Current.GetString("/plugins/orphanedproperties/successdelete"), propertyDefinition.Name + " (" + propertyDefinition.Type.Name + ")");
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
                        result.Description = string.Format(LocalizationService.Current.GetString("/plugins/orphanedproperties/errordelete"), propertyDefinition.Name + " (" + propertyDefinition.Type.Name + ")");
                    }
                }
                else
                {
                    result.Status = "Error";
                    result.Description = string.Format(LocalizationService.Current.GetString("/plugins/orphanedproperties/propertynull"), propertyId);
                }
            }
            else
            {
                result.Status = "Error";
                result.Description =LocalizationService.Current.GetString("/plugins/orphanedproperties/propertyvaluezero");
            }
            return result;
        }

    }
}
