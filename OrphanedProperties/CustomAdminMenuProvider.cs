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
            var link = new UrlMenuItem(
                "Orphaned Properties",
                MenuPaths.Global + "/cms/admin/orphanedproperties", 
                "/OrphanedProperties/Index")
            {
                SortIndex = 100,
                AuthorizationPolicy = Constants.PolicyName
            };

            return new List<MenuItem>
            {
                link
            };
        }
    }
}
