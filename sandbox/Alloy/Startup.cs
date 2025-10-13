using Alloy.Extensions;
using EPiServer.Cms.Shell;
using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Scheduler;
using EPiServer.Web.Routing;
using SeoBoost.Business.Configuration;

namespace Alloy;

public class Startup
{
    private readonly IWebHostEnvironment _webHostingEnvironment;

    public Startup(IWebHostEnvironment webHostingEnvironment)
    {
        _webHostingEnvironment = webHostingEnvironment;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        if (_webHostingEnvironment.IsDevelopment())
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(_webHostingEnvironment.ContentRootPath, "App_Data"));

            services.Configure<SchedulerOptions>(options => options.Enabled = false);
        }

        services
            .AddCmsAspNetIdentity<ApplicationUser>()
            .AddCms()
            .AddAlloy()
            .AddAdminUserRegistration()
            .AddEmbeddedLocalization<Startup>();

        // Required by Wangkanai.Detection
        services.AddDetection();
        
        services.AddSeoBoost(x =>
        {
            x.CustomCanonicalTagFieldName = "CanonicalTag"; // Default fa
            x.UseSimpleAddressAsPath = true; // Default false
            x.EnableRobotsTxtSupport = true; // Default false
            x.UseMirrorPageReference = true; // Default false
            x.UseSiteUrlAsHost = true; // Default false
        });

        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromSeconds(10);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        //services.Configure<RoutingOptions>(options =>
        //{
        //    options.UseTrailingSlash = false;
        //});
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // Required by Wangkanai.Detection
        app.UseDetection();
        app.UseSession();

        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();


        //app.UseEndpoints(endpoints => { endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}"); });
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapDefaultControllerRoute();
            //endpoints.MapControllers();
            endpoints.MapContent();
        });
    }
}
