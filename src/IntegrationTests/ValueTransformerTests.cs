namespace AutoMapper.IntegrationTests
{
    namespace ValueTransformerTests
    {
        public class BasicTransforming(DatabaseFixture databaseFixture) : IntegrationTest<BasicTransforming.DatabaseInitializer>(databaseFixture)
        {
            public class Source
            {
                [Key]
                public int Id { get; set; }
                public string Value { get; set; }
            }

            public class Dest
            {
                public string Value { get; set; }
            }

            public class Context : LocalDbContext
            {
                public DbSet<Source> Sources { get; set; }
            }

            public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
            {
                protected override void Seed(Context context)
                {
                    context.Sources.Add(new Source { Value = "Jimmy" });

                    base.Seed(context);
                }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.CreateProjection<Source, Dest>();
                cfg.ValueTransformers.Add<string>(dest => dest + " is straight up dope");
            });

            [Fact]
            public async Task Should_transform_value()
            {
                using (var context = Fixture.CreateContext())
                {
                    var dest = await ProjectTo<Dest>(context.Sources).SingleAsync();

                    dest.Value.ShouldBe("Jimmy is straight up dope");
                }
            }
        }

        public class StackingTransformers(DatabaseFixture databaseFixture) : IntegrationTest<StackingTransformers.DatabaseInitializer>(databaseFixture)
        {
            public class Source
            {
                [Key]
                public int Id { get; set; }
                public string Value { get; set; }
            }

            public class Dest
            {
                public string Value { get; set; }
            }

            public class Context : LocalDbContext
            {
                public DbSet<Source> Sources { get; set; }
            }

            public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
            {
                protected override void Seed(Context context)
                {
                    context.Sources.Add(new Source { Value = "Jimmy" });

                    base.Seed(context);
                }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.CreateProjection<Source, Dest>();
                cfg.ValueTransformers.Add<string>(dest => dest + " is straight up dope");
                cfg.ValueTransformers.Add<string>(dest => dest + "! No joke!");
            });

            [Fact]
            public async Task Should_stack_transformers_in_order()
            {
                using (var context = Fixture.CreateContext())
                {
                    var dest = await ProjectTo<Dest>(context.Sources).SingleAsync();

                    dest.Value.ShouldBe("Jimmy is straight up dope! No joke!");
                }
            }
        }

        public class DifferentProfiles(DatabaseFixture databaseFixture) : IntegrationTest<DifferentProfiles.DatabaseInitializer>(databaseFixture)
        {
            public class Source
            {
                [Key]
                public int Id { get; set; }
                public string Value { get; set; }
            }

            public class Dest
            {
                public string Value { get; set; }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.CreateProjection<Source, Dest>();
                cfg.ValueTransformers.Add<string>(dest => dest + " is straight up dope");
                cfg.CreateProfile("Other", p => p.ValueTransformers.Add<string>(dest => dest + "! No joke!"));
            });

            public class Context : LocalDbContext
            {
                public DbSet<Source> Sources { get; set; }
            }

            public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
            {
                protected override void Seed(Context context)
                {
                    context.Sources.Add(new Source { Value = "Jimmy" });

                    base.Seed(context);
                }
            }

            [Fact]
            public async Task Should_not_apply_other_transform()
            {
                using (var context = Fixture.CreateContext())
                {
                    var dest = await ProjectTo<Dest>(context.Sources).SingleAsync();

                    dest.Value.ShouldBe("Jimmy is straight up dope");
                }
            }
        }

        public class StackingRootConfigAndProfileTransform(DatabaseFixture databaseFixture) : IntegrationTest<StackingRootConfigAndProfileTransform.DatabaseInitializer>(databaseFixture)
        {
            public class Source
            {
                [Key]
                public int Id { get; set; }
                public string Value { get; set; }
            }

            public class Dest
            {
                public string Value { get; set; }
            }

            public class Context : LocalDbContext
            {
                public DbSet<Source> Sources { get; set; }
            }

            public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
            {
                protected override void Seed(Context context)
                {
                    context.Sources.Add(new Source { Value = "Jimmy" });

                    base.Seed(context);
                }
            }
            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.ValueTransformers.Add<string>(dest => dest + "! No joke!");
                cfg.CreateProfile("Other", p =>
                {
                    p.CreateProjection<Source, Dest>();
                    p.ValueTransformers.Add<string>(dest => dest + " is straight up dope");
                });
            });

            [Fact]
            public async Task ShouldApplyProfileFirstThenRoot()
            {
                using (var context = Fixture.CreateContext())
                {
                    var dest = await ProjectTo<Dest>(context.Sources).SingleAsync();

                    dest.Value.ShouldBe("Jimmy is straight up dope! No joke!");
                }
            }
        }

        public class TransformingValueTypes(DatabaseFixture databaseFixture) : IntegrationTest<TransformingValueTypes.DatabaseInitializer>(databaseFixture)
        {
            public class Source
            {
                [Key]
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public class Dest
            {
                public int Value { get; set; }
            }

            public class Context : LocalDbContext
            {
                public DbSet<Source> Sources { get; set; }
            }

            public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
            {
                protected override void Seed(Context context)
                {
                    context.Sources.Add(new Source { Value = 5 });

                    base.Seed(context);
                }
            }
            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.ValueTransformers.Add<int>(dest => dest * 2);
                cfg.CreateProfile("Other", p =>
                {
                    p.CreateProjection<Source, Dest>();
                    p.ValueTransformers.Add<int>(dest => dest + 3);
                });
            });

            [Fact]
            public async Task ShouldApplyProfileFirstThenRoot()
            {
                using (var context = Fixture.CreateContext())
                {
                    var dest = await ProjectTo<Dest>(context.Sources).SingleAsync();

                    dest.Value.ShouldBe((5 + 3) * 2);
                }
            }
        }

        public class StackingRootAndProfileAndMemberConfig(DatabaseFixture databaseFixture) : IntegrationTest<StackingRootAndProfileAndMemberConfig.DatabaseInitializer>(databaseFixture)
        {
            public class Source
            {
                [Key]
                public int Id { get; set; }
                public string Value { get; set; }
            }

            public class Dest
            {
                public string Value { get; set; }
            }

            public class Context : LocalDbContext
            {
                public DbSet<Source> Sources { get; set; }
            }

            public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
            {
                protected override void Seed(Context context)
                {
                    context.Sources.Add(new Source { Value = "Jimmy" });

                    base.Seed(context);
                }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.ValueTransformers.Add<string>(dest => dest + "! No joke!");
                cfg.CreateProfile("Other", p =>
                {
                    p.CreateProjection<Source, Dest>()
                        .ValueTransformers.Add<string>(dest => dest + ", for real,");
                    p.ValueTransformers.Add<string>(dest => dest + " is straight up dope");
                });
            });

            [Fact]
            public async Task ShouldApplyTypeMapThenProfileThenRoot()
            {
                using (var context = Fixture.CreateContext())
                {
                    var dest = await ProjectTo<Dest>(context.Sources).SingleAsync();

                    dest.Value.ShouldBe("Jimmy, for real, is straight up dope! No joke!");
                }
            }
        }

        public class StackingTypeMapAndRootAndProfileAndMemberConfig(DatabaseFixture databaseFixture) : IntegrationTest<StackingTypeMapAndRootAndProfileAndMemberConfig.DatabaseInitializer>(databaseFixture)
        {
            public class Source
            {
                [Key]
                public int Id { get; set; }
                public string Value { get; set; }
            }

            public class Dest
            {
                public string Value { get; set; }
            }

            public class Context : LocalDbContext
            {
                public DbSet<Source> Sources { get; set; }
            }

            public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
            {
                protected override void Seed(Context context)
                {
                    context.Sources.Add(new Source { Value = "Jimmy" });

                    base.Seed(context);
                }
            }
            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.ValueTransformers.Add<string>(dest => dest + "! No joke!");
                cfg.CreateProfile("Other", p =>
                {
                    p.CreateProjection<Source, Dest>()
                        .AddTransform<string>(dest => dest + ", for real,")
                        .ForMember(d => d.Value, opt => opt.AddTransform(d => d + ", seriously"));
                    p.ValueTransformers.Add<string>(dest => dest + " is straight up dope");
                });
            });

            [Fact]
            public async Task ShouldApplyTypeMapThenProfileThenRoot()
            {
                using (var context = Fixture.CreateContext())
                {
                    var dest = await ProjectTo<Dest>(context.Sources).SingleAsync();

                    dest.Value.ShouldBe("Jimmy, seriously, for real, is straight up dope! No joke!");
                }
            }
        }
    }
}