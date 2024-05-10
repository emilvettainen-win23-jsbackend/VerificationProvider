using Microsoft.EntityFrameworkCore;
using VerificationProvider.Data.Entities;

namespace VerificationProvider.Data.Contexts;

public class VerificationDataContext(DbContextOptions<VerificationDataContext> options) : DbContext(options)
{
    public DbSet<VerificationRequestEntity> VerificationRequests { get; set; }
}
