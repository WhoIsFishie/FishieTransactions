
using FishieTransactions.Data;
using FishieTransactions.Services;
using Microsoft.Extensions.Options;

namespace FishieTransactions
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));

            builder.Services.AddSingleton<IMongoDbSettings>(serviceProvider =>
                serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value);


            builder.Services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));
            builder.Services.AddScoped(typeof(IApiClient), typeof(ApiClient));
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();


            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddHttpClient<ApiClient>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            //load settings to memory 
            try { Statics.GetUrlFromFile(); } catch (Exception ex) { }
            try { Statics.LoadLoginDetails(); } catch (Exception ex) { }
            try { Statics.GetAccountsFromFile(); } catch (Exception ex) { }



            Statics.key = builder.Configuration.GetValue<string>("MyKey");

            app.Run();
        }
    }
}