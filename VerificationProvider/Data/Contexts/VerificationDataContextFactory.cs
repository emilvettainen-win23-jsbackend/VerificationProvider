using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VerificationProvider.Data.Contexts;

public class VerificationDataContextFactory : IDesignTimeDbContextFactory<VerificationDataContext>
{
    public VerificationDataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<VerificationDataContext>();
        optionsBuilder.UseSqlServer("Server=tcp:evettainen.database.windows.net,1433;Initial Catalog=Silicon-SqlServer;Persist Security Info=False;User ID=SqlAdmin;Password=ZWQ3OTM1MzUtMDA3Ni00YmRhLWIyOGQtZmEzY2ZkZDkyMTcx;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
        return new VerificationDataContext(optionsBuilder.Options);
    }
}