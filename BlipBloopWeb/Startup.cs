using BlipBloopBot.Extensions;
using BlipBloopBot.Model.EventSub;
using BlipBloopBot.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Threading.Tasks;

namespace BlipBloopWeb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "BlipBloopWeb", Version = "v1" });
            });
            services.AddApplicationInsightsTelemetry();

            services.Configure<EventSubOptions>(Configuration.GetSection("Twitch:EventSub"));
            services.AddEventSub();
            services.AddEventSubHandler<TwitchEventSubChannelUpdateEvent>((context, eventSub) =>
            {
                context.Logger.LogInformation("Received a channel update for {channelName}, streaming {category} - {text}", eventSub.BroadcasterUserName, eventSub.CategoryName, eventSub.Title);
                return Task.CompletedTask;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BlipBloopWeb v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseTwitchEventSub("/webhooks/eventsub");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
