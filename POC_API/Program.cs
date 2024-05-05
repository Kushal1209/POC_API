using ExcelToDatabase.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using POC_API.EmailSender;
using POC_API.SMS;
using POC_API.sms_campaign;
using Telnyx;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Poc_api") ??
    throw new InvalidOperationException("Connection String 'DefaultConnection' not found");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));


string telnyxApiKey = "KEY018EDF5AD92F09666CCB334A8420AE07_yR0HLP0dEJLn4AHQsLG159";
TelnyxConfiguration.SetApiKey(telnyxApiKey);

// Add services
builder.Services.AddTransient<SmsService>(); // Add SmsService to DI container


// Add services to the container.
builder.Services.AddTransient<IEmailSender, EmailSender>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseSwagger();
app.UseSwaggerUI(c =>
{
c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
c.RoutePrefix = "swagger";  // Set the Swagger UI at this route as default
});

// Redirect from root to Swagger UI
app.Use(async (context, next) =>
    {
        if (context.Request.Path == "/")
        {
            // Redirecting to Swagger UI
            context.Response.Redirect("/swagger");
            return;  // Important to return after redirect
        }
        await next();
});


    app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
