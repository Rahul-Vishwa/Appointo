using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Taskzen.Helpers;

namespace Taskzen.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            DotNetEnv.Env.Load();
            var basePath = Path.Combine(Directory.GetCurrentDirectory());
            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.Development.json", optional: false)
                .Build();

            var encryptedConnectionString = config.GetConnectionString("DefaultConnection");

            // var decrypted = encryptedConnectionString;
            var decrypted = EncryptionHelper.Decrypt(encryptedConnectionString);

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(decrypted); // Or whatever provider you're using

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}