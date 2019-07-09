using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreCHILIDemo.Models;
using AspNetCoreCHILIDemo.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCoreCHILIDemo
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public static bool testMode = false;

        public Startup(IConfiguration configuration)
        {
;           this.configuration = configuration;

            bool.TryParse(configuration["TestMode"], out testMode);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddMvc();

            services.AddSingleton<ChiliConnector>(new ChiliConnector(configuration.GetValue<string>("ChiliUrl")));

            services.AddDistributedMemoryCache();
            services.ConfigureApplicationCookie(options => options.LoginPath = "/home/index");
            services.AddSession();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSession();
            app.UseMvcWithDefaultRoute();

            app.UseStaticFiles();
        }
    }
}
