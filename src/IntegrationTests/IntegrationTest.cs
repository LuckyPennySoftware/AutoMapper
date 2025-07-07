namespace AutoMapper.IntegrationTests;

public abstract class IntegrationTest<TInitializer> : AutoMapperSpecBase, IAsyncLifetime where TInitializer : IInitializer, new()
{
    ValueTask IAsyncLifetime.InitializeAsync() => new TInitializer().Migrate();
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

public interface IInitializer
{
    ValueTask Migrate();
}

public class DropCreateDatabaseAlways<TContext> : IInitializer where TContext : DbContext, new()
{
    protected virtual void Seed(TContext context)
    {
    }

    public async ValueTask Migrate()
    {
        await using var context = new TContext();
        var database = context.Database;
        await database.EnsureDeletedAsync();
        var strategy = database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () => await database.EnsureCreatedAsync());

        Seed(context);

        await context.SaveChangesAsync();
    }
}

public abstract class LocalDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlServer(
        @$"Data Source=(localdb)\mssqllocaldb;Integrated Security=True;MultipleActiveResultSets=True;Database={GetType()};Connection Timeout=300",
        o => o.EnableRetryOnFailure(maxRetryCount: 10).CommandTimeout(120));
}