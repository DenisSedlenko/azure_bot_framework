// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using BusinessLayer.Implementation;
using BusinessLayer.Services;
using Common.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TeamsAuth.Interfaces.Service;
using TeamsAuth.Services;

namespace Microsoft.BotBuilderSamples
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();

            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            services.AddSingleton<IStorage, MemoryStorage>();

            services.AddSingleton<UserState>()
                    .AddSingleton<ConversationState>();

            services.AddSingleton<MainDialog>();

            services.AddTransient<IRepository, Repository>();
            services.AddTransient<IStorageService, StorageService>();
            services.AddTransient<IKeyVaultService, KeyVaultService>();
            services.AddTransient<IBotService, BotService>();

            services.AddTransient<IBot, TeamsBot<MainDialog>>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }
    }
}
