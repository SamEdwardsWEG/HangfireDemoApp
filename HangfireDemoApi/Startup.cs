using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.SqlServer;
using HangfireDemoApi.Services;
using HangfireDemoDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace HangfireDemoApi
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
            services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("HangfireDemoConnectionString"));
            });

            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(Configuration.GetConnectionString("HangfireDemoConnectionString"), new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    UsePageLocksOnDequeue = true,
                    DisableGlobalLocks = true
                })
                .UseNLogLogProvider());

            services.AddHangfireServer();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "HangfireDemo Api", Version = "v1" });
            });
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHangfireDashboard();

            BackgroundJob.Schedule(() => Console.WriteLine("Hangfire Works!!!"), TimeSpan.FromSeconds(30));

            app.UseHttpsRedirection();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "PPM Api V1");
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            StartHangfireJobs();
        }

        public void StartHangfireJobs()
        {
            // This will process the job as soon as it is created
            var jobId = BackgroundJob.Enqueue(() => HangfireDemoService.FireAndForget());

            // These three will delay processing the job for the given time
            BackgroundJob.Schedule(() => HangfireDemoService.Delayed(1), TimeSpan.FromMinutes(1));
            BackgroundJob.Schedule(() => HangfireDemoService.Delayed(2), TimeSpan.FromMinutes(2));
            var delayedJobId = BackgroundJob.Schedule(() => HangfireDemoService.Delayed(3), TimeSpan.FromMinutes(3));

            // This job will get processed every minute
            RecurringJob.AddOrUpdate(() => HangfireDemoService.Recurring(), "*/1 * * * *");

            // This job will get processed as soon as the fire and forget job is complete
            BackgroundJob.ContinueJobWith(jobId, () => Console.WriteLine("Fire and Forget done!!!"));

            // This job will get processed as soon as the 3rd delayed job is complete
            BackgroundJob.ContinueJobWith(delayedJobId, () => Console.WriteLine("Delayed jobs done!"));

            // This job will process a long job every 8 minutes
            RecurringJob.AddOrUpdate(() => HangfireDemoService.LongRecurring(), "*/8 * * * *");
        }
    }
}
