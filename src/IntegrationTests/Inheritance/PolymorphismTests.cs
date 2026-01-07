namespace AutoMapper.IntegrationTests.Inheritance;

public class PolymorphismTests(DatabaseFixture databaseFixture) : IntegrationTest<PolymorphismTests.DatabaseInitializer>(databaseFixture)
{
    public abstract class Vehicle
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Car : Vehicle
    {
        public int AmountDoors { get; set; }
    }

    public class Motorcycle : Vehicle
    {
        public bool HasSidecar { get; set; }
    }
    
    public class Bicycle : Vehicle
    {
        public bool EBike { get; set; }
    }

    public class VehicleModel
    {
        public string Name { get; set; }
    }

    public class MotorcycleModel : VehicleModel
    {
        public bool HasSidecar { get; set; }
    }
    
    public class BicycleModel : VehicleModel
    {
        public bool EBike { get; set; }
    }

    public class Context : LocalDbContext
    {
        public DbSet<Vehicle> Vehicles { get; set; }
        
        public DbSet<Car> Cars { get; set; }
        
        public DbSet<Motorcycle> Motorcycles { get; set; }
        
        public DbSet<Bicycle> Bicycles { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration()
    {
        return new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Vehicle, VehicleModel>()
                .IncludeAllDerived();
	
            cfg.CreateMap<Motorcycle, MotorcycleModel>();
            cfg.CreateMap<Bicycle, BicycleModel>();
        });
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.Vehicles.Add(new Car { Name = "Car", AmountDoors = 4 });
            context.Vehicles.Add(new Bicycle { Name = "Bicycle", EBike = true });
            context.Vehicles.Add(new Motorcycle { Name = "Motorcycle", HasSidecar = true });

            base.Seed(context);
        }
    }

    [Fact]
    public void Should_project_base_queryable_to_derived_models_polymorphic()
    {
        using var context = Fixture.CreateContext();
        var results = context.Vehicles.ProjectTo<VehicleModel>(Configuration).ToArray();
        results.Length.ShouldBe(3);
        results.ShouldContain(x => x.GetType() == typeof(VehicleModel), 1);
        results.ShouldContain(x => x.GetType() == typeof(BicycleModel), 1);
        results.ShouldContain(x => x.GetType() == typeof(MotorcycleModel), 1);
    }
    
    [Fact]
    public void Should_project_derived_queryable_to_derived_models_if_derived_models_exist()
    {
        using var context = Fixture.CreateContext();
        var results = context.Motorcycles.ProjectTo<MotorcycleModel>(Configuration).ToArray();
        results.Length.ShouldBe(1);
        results.ShouldContain(x => x.GetType() == typeof(MotorcycleModel), 1);
    }
    
    [Fact]
    public void Should_project_derived_queryable_to_base_models_if_no_derived_models_exist()
    {
        using var context = Fixture.CreateContext();
        var results = context.Cars.ProjectTo<VehicleModel>(Configuration).ToArray();
        results.Length.ShouldBe(1);
        results.ShouldContain(x => x.GetType() == typeof(VehicleModel), 1);
    }
}

public class PolymorphismTptTests(DatabaseFixture databaseFixture) : IntegrationTest<PolymorphismTptTests.DatabaseInitializer>(databaseFixture)
{
    public abstract class Vehicle
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Car : Vehicle
    {
        public int AmountDoors { get; set; }
    }

    public class Motorcycle : Vehicle
    {
        public bool HasSidecar { get; set; }
    }
    
    public abstract class VehicleModel
    {
        public string Name { get; set; }
    }
    
    public class CarModel : VehicleModel
    {
        public int AmountDoors { get; set; }
    }

    public class MotorcycleModel : VehicleModel
    {
        public bool HasSidecar { get; set; }
    }
    
    public class Context : LocalDbContext
    {
        public DbSet<Vehicle> Vehicles { get; set; }
        
        public DbSet<Car> Cars { get; set; }
        
        public DbSet<Motorcycle> Motorcycles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Vehicle>().UseTptMappingStrategy();
        }
    }

    protected override MapperConfiguration CreateConfiguration()
    {
        return new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Vehicle, VehicleModel>()
                .Include<Car, CarModel>()
                .Include<Motorcycle, MotorcycleModel>();
	
            cfg.CreateMap<Car, CarModel>();
            cfg.CreateMap<Motorcycle, MotorcycleModel>();
        });
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.Vehicles.Add(new Car { Name = "Car", AmountDoors = 4 });
            context.Vehicles.Add(new Motorcycle { Name = "Motorcycle", HasSidecar = true });

            base.Seed(context);
        }
    }

    [Fact]
    public void Should_project_base_queryable_to_derived_models_polymorphic()
    {
        using var context = Fixture.CreateContext();
        var results = context.Vehicles.ProjectTo<VehicleModel>(Configuration);
        results.Count().ShouldBe(2);
        results.Count(x => x.Name == "Car").ShouldBe(1);
        results.Count(x => x.Name == "Motorcycle").ShouldBe(1);
    }
    
    [Fact]
    public void Should_project_derived_queryable_to_derived_models_if_derived_models_exist()
    {
        using var context = Fixture.CreateContext();
        var results = context.Motorcycles.ProjectTo<MotorcycleModel>(Configuration);
        results.Count().ShouldBe(1);
        results.Count(x => x.Name == "Motorcycle").ShouldBe(1);
    }
    
    [Fact]
    public void Should_map_base_queryable_to_derived_models_polymorphic()
    {
        using var context = Fixture.CreateContext();
        var results = Map<VehicleModel[]>(context.Vehicles);
        results.Length.ShouldBe(2);
        results.ShouldContain(x => x.GetType() == typeof(CarModel), 1);
        results.ShouldContain(x => x.GetType() == typeof(MotorcycleModel), 1);
    }
    
    [Fact]
    public void Should_map_derived_queryable_to_derived_models_if_derived_models_exist()
    {
        using var context = Fixture.CreateContext();
        var results = Map<MotorcycleModel[]>(context.Motorcycles);
        results.Length.ShouldBe(1);
        results.ShouldContain(x => x.GetType() == typeof(MotorcycleModel), 1);
    }
}