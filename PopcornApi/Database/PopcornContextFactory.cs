using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PopcornApi.Logger;

namespace PopcornApi.Database
{
    public class PopcornContextFactory : IDesignTimeDbContextFactory<PopcornContext>
    {
        public PopcornContext CreateDbContext(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json");
            var configuration = builder.Build();

            var optionsBuilder = new DbContextOptionsBuilder<PopcornContext>();
            optionsBuilder.UseSqlServer(configuration["SQL:ConnectionString"]);

            // Add logging
#if DEBUG
            ILoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new ContextLoggerProvider(logLevel => logLevel >= LogLevel.Information));

            optionsBuilder.UseLoggerFactory(loggerFactory);
#endif
            return new PopcornContext(optionsBuilder.Options);
        }
    }
}