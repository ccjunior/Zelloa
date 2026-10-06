using Microsoft.EntityFrameworkCore;

namespace Zelloa.Infrastructure.Persistence;

public sealed class ZelloaDbContext(DbContextOptions<ZelloaDbContext> options) : DbContext(options);
