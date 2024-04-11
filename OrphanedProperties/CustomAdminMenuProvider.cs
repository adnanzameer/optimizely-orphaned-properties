using EPiServer.Framework.Localization;
using EPiServer.Shell.Navigation;
using System.Collections.Generic;
using OrphanedProperties.Models;

namespace OrphanedProperties
{
    [MenuProvider]
    public class CustomAdminMenuProvider : IMenuProvider
    {
        public IEnumerable<MenuItem> GetMenuItems()
        {
            var urlMenuItem1 = new UrlMenuItem(LocalizationService.Current.GetString("/plugins/orphanedproperties/displayname"),
                "/global/cms/admin/orphanedproperties", 
                "/OrphanedProperties/Index")
            {
                IsAvailable = context => true,
                SortIndex = 100,
                AuthorizationPolicy = Constants.PolicyName
            };

            return new List<MenuItem>(1)
            {
                urlMenuItem1
            };
        }
    }
}
