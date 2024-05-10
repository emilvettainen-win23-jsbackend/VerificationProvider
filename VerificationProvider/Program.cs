using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VerificationProvider.Data.Contexts;
using VerificationProvider.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context,services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddDbContext<VerificationDataContext>(x => x.UseSqlServer(context.Configuration.GetConnectionString("AzureDb")));
        services.AddSingleton(new ServiceBusClient(context.Configuration.GetConnectionString("ServiceBusConnection")));

        services.AddScoped<ValidateVerificationService>();
        services.AddScoped<VerificationCleanerService>();
        services.AddScoped<VerificationGenerateService>();
    })
    .Build();

//using (var scope = host.Services.CreateScope())
//{
//    try
//    {
//        var context = scope.ServiceProvider.GetRequiredService<VerificationDataContext>();
//        var migration = context.Database.GetPendingMigrations();
//        if (migration != null && migration.Any())
//        {
//            context.Database.Migrate();
//        }
//    }
//    catch (Exception ex)
//    {
//        Debug.WriteLine($" ERROR : VerificationProvider.Program.cs.Migration :: {ex.Message}");
//    }
//}

host.Run();
