using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Testcontainers.MsSql;

namespace AutoMapper.IntegrationTests;

[CollectionDefinition(nameof(DatabaseFixture))]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture> { }

public class DatabaseFixture : IAsyncLifetime
{
    private MsSqlContainer _msSqlContainer;
    
    async Task IAsyncLifetime.DisposeAsync()
    {
        await _msSqlContainer.DisposeAsync();
    }

    public string GetConnectionString() => _msSqlContainer.GetConnectionString();

    async Task IAsyncLifetime.InitializeAsync()
    {
        _msSqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04")
            .Build();
        
        await _msSqlContainer.StartAsync();
    }
}

[Collection(nameof(DatabaseFixture))]
public abstract class IntegrationTest<TDbContextFixture> : AutoMapperSpecBase, IAsyncLifetime 
    where TDbContextFixture : IDbContextFixture, new()
{
    private readonly DatabaseFixture _databaseFixture;

    protected IntegrationTest(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
    }
    
    protected TDbContextFixture Fixture { get; private set; }

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    async Task IAsyncLifetime.InitializeAsync()
    {
        var connectionString = _databaseFixture.GetConnectionString();
        
        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = GetType().FullName
        };

        Fixture = new TDbContextFixture
        {
            ConnectionString = builder.ToString()
        };
        
        await Fixture.Migrate();
    }
}

public interface IDbContextFixture
{
    Task Migrate();
    string ConnectionString { get; set; }
}

public class DropCreateDatabaseAlways<TContext> : IDbContextFixture where TContext : LocalDbContext, new()
{
    public string ConnectionString { get; set; }

    protected virtual void Seed(TContext context){}
    
    public async Task Migrate()
    {
        await using var context = new TContext
        {
            ConnectionString = ConnectionString
        };
        
        var database = context.Database;
        await database.EnsureDeletedAsync();
        var strategy = database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () => await database.EnsureCreatedAsync());

        Seed(context);

        await context.SaveChangesAsync();
    }

    public TContext CreateContext()
    {
        return new TContext
        {
            ConnectionString = ConnectionString
        };
    }
}

public abstract class LocalDbContext : DbContext
{
    public string ConnectionString { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlServer(
        ConnectionString,
        o => o.EnableRetryOnFailure(maxRetryCount: 10).CommandTimeout(120));
}