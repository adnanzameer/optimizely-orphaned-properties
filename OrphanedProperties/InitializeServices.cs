using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrphanedProperties.Helpers;
using OrphanedProperties.Models;
using System;
using System.Linq;
using EPiServer.Authorization;
using EPiServer.Shell.Modules;
using Microsoft.AspNetCore.Mvc.Razor;

namespace OrphanedProperties
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class InitializeServices : IConfigurableModule
    {
        private static readonly Action<AuthorizationPolicyBuilder> DefaultPolicy = p => p.RequireRole(Roles.Administrators, Roles.WebAdmins, Roles.CmsAdmins);

        void IConfigurableModule.ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Services.AddScoped<IOrphanedPropertiesService, OrphanedPropertiesService>();

            context.Services.Configure<ProtectedModuleOptions>(
                pm =>
                {
                    if (!pm.Items.Any(i => i.Name.Equals(Constants.ModuleName, StringComparison.OrdinalIgnoreCase)))
                    {
                        pm.Items.Add(new ModuleDetails { Name = Constants.ModuleName });
                    }
                });

            context.Services.Configure(
                (Action<RazorViewEngineOptions>)(ro =>
                {
                    if (ro.ViewLocationExpanders.Any(
                            v =>
                                v.GetType() == typeof(ModuleLocationExpander)))
                        return;
                    ro.ViewLocationExpanders.Add(
                        new ModuleLocationExpander());
                }));

            context.Services.AddAuthorization(options =>
            {
                options.AddPolicy(Constants.PolicyName, DefaultPolicy);
            });
        }

        void IInitializableModule.Initialize(InitializationEngine context)
        {
        }

        void IInitializableModule.Uninitialize(InitializationEngine context)
        {
        }
    }
}
