using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrphanedProperties.Helpers;
using OrphanedProperties.Models;
using System;
using EPiServer.Authorization;

namespace OrphanedProperties
{
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class InitializeServices : IConfigurableModule
    {
        private static readonly Action<AuthorizationPolicyBuilder> DefaultPolicy = p => p.RequireRole(Roles.Administrators, Roles.WebAdmins, Roles.CmsAdmins);

        void IConfigurableModule.ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Services.AddScoped<IOrphanedPropertiesService, OrphanedPropertiesService>();

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
