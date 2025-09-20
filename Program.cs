using System.Net;
using MPLUS_GW_WebCore.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using MPLUS_GW_WebCore;
using MPLUS_GW_WebCore.Services;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using System;
using System.Net.WebSockets;
using OfficeOpenXml;
using MPLUS_GW_WebCore.Controllers.Materials;

var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/debug-log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
ExcelPackage.LicenseContext = LicenseContext.Commercial;

builder.Services.AddScoped<CheckHoldingService>();
builder.Services.AddHttpClient();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddDbContext<MplusGwContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MPLUSGW54")));
builder.Services.AddDbContext<EslsystemContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Eink1")));
builder.Services.AddDbContext<ExportDataQadContext>(options =>
   options.UseSqlServer(builder.Configuration.GetConnectionString("QADcontext")));

builder.Services.AddTransient<ScheduledTaskService>();

builder.Services.AddHangfire(x => x.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("MPLUSGW54")
));

builder.Services.AddHangfireServer();
builder.Services.AddTransient<ExcelData>();
builder.Services.AddTransient<ProductionCalculator>();

try
{
    builder.Services.AddSignalR();

    builder.Services.AddScoped<MaterialsController>();
    builder.Services.AddScoped<MplusGwContext>();
    builder.Services.AddScoped<ConnectMES.Classa>();

    builder.Services.AddSession();
    Log.Information("Starting up the application....");
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();
    app.UseSession();

    app.UseHangfireDashboard();

    RecurringJob.AddOrUpdate<ScheduledTaskService>(
        "update_content_job",
        x => x.UpdateListSubmaterials(),
        "0 23 * * *",
        new RecurringJobOptions
        {
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
        });

    RecurringJob.AddOrUpdate<ScheduledTaskService>(
        "update_receiving_pl",
        x => x.UpdateItemInRecevingPLMES(),
         "*/1 * * * *",
        new RecurringJobOptions
        {
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
        });

    RecurringJob.AddOrUpdate<CheckHoldingService>(
        "check_holding_material",
        x => x.CheckHolding(),
        "*/1 * * * *",
         new RecurringJobOptions
         {
             TimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
         });
    RecurringJob.AddOrUpdate<ScheduledTaskService>(
       "StockMes_Eink",
       x => x.StockMes(),
        "*/1 * * * *",
       new RecurringJobOptions
       {
           TimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
       });

    RecurringJob.AddOrUpdate<ScheduledTaskService>(
     "fetch-workorder-mes-in3month",
     x => x.GetWorkOrdersMES(),
      "*/1 * * * *",
     new RecurringJobOptions
     {
         TimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
     });

    RecurringJob.AddOrUpdate<ScheduledTaskService>(
      "fetch-data-machines-printlabel",
      x => x.FetchDataPrintLabelMCDiv(),
       "*/1 * * * *",
      new RecurringJobOptions
      {
          TimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
      });
    RecurringJob.AddOrUpdate<ScheduledTaskService>(
        "cleanup-inactive-users",
        job => job.InActiveUser(),
        Cron.Minutely,
          new RecurringJobOptions
          {
              TimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
          });

    RecurringJob.AddOrUpdate<CheckHoldingService>(
        "cleanup-minute-jobs",
        x => x.CleanupMinuteJobs(),
        "0 0 * * *",
         new RecurringJobOptions
         {
             TimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
         });
    app.UseStatusCodePages(appError =>
    {
        appError.Run(async context =>
        {
            var respone = context.Response;
            var code = respone.StatusCode;

            var content = @$"
        <html>
            <head>
                <meta charset='utf-8'/>
                <title>Lỗi {code}</title>
            </head>
            <body>
                <div style='text-align: center;
                            color: red;
                            font-size: 35px;
                            margin-top: 35px;'>
                    <p>
                        Có lỗi xảy ra: {code} - {(HttpStatusCode)code}
                    </p>
                    <a style='text-decoration: none;' href='/'>Quay lại Trang chủ</a>
                </div>
            </body>
        </html>";
            await respone.WriteAsync(content);
        });
    }); // Lỗi 400 - 599

    app.UseAuthentication(); // Xác định danh tính
    app.UseAuthorization(); // Xác định quyền truy cập

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
    app.MapHub<MaterialTaskHub>("/materialTaskHub");
    app.MapRazorPages();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}
