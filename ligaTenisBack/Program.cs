using ligaTenisBack.Models.DbModels;
using Microsoft.EntityFrameworkCore;
using Mscc.GenerativeAI.Web;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGenerativeAI(builder.Configuration.GetSection("Gemini"));

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});


string conn = builder.Configuration.GetConnectionString("DefaultConnectionMySQL")!;

builder.Services.AddScoped<ligaTenisBack.Services.LigaTenisService>();

builder.Services.AddDbContext<LigatenisContext>(options =>
    options.UseMySql(conn, ServerVersion.AutoDetect(conn))
);

builder.Services.AddHttpClient("ApiInterna", client =>
{
    client.BaseAddress = new Uri("https://localhost:44372/");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();
app.MapControllers();
app.Run();
