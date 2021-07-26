using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevIO.API.Configuration
{
    public static class ApiConfig
    {
        public static IServiceCollection WebApiConfig(this IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true; // assume valor default
                options.DefaultApiVersion = new ApiVersion(1, 0); // Versão da api
                options.ReportApiVersions = true; // vai passar no header a info da api

            });

            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true; // vai setar o valor indicado dentro da api

            });

            services.Configure<ApiBehaviorOptions>(Options =>
            {
                Options.SuppressModelStateInvalidFilter = true;
            });

            // Cors não é um recurso de segurança , e sim um relaxamento de segurança
            services.AddCors(Options =>
            {
                Options.AddPolicy("Development",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());


                Options.AddPolicy("Production",
                    builder =>
                    builder
                    .WithMethods("GET") //somente metodos get
                    .WithOrigins("http://desenvolvedor.io") // somente para esta origem
                    .SetIsOriginAllowedToAllowWildcardSubdomains() // permissão para subdominios da API
                   // .WithHeaders(HeaderNames.ContentType, "x-custom-header") // restrições de headers
                    .AllowAnyHeader()); //
            });




            return services;
        }

        public static IApplicationBuilder UseMvcConfiguration(this IApplicationBuilder app)
        {

            app.UseHttpsRedirection();
            app.UseMvc();          

            return app;
        }
    }
}
