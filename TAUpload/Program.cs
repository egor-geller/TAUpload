using TAUpload.Repository;
using TAUpload.Repository.Interface;
using TAUpload.Service;
using TAUpload.Service.Interface;

namespace TAUpload
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddScoped<IGnEntityFilesRepository,  GnEntityFilesRepository>();
            builder.Services.AddScoped<IGnEntityFilesService,  GnEntityFilesService>();
            /*builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    builder.WithOrigins("http://localhost:8088")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .Build());
            });*/

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseRouting();

            //app.UseCors();
            app.UseStaticFiles();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}