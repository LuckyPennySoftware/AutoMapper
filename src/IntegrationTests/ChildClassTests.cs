namespace AutoMapper.IntegrationTests
{
    namespace ChildClassTests
    {
        public class Base
        {
            public int BaseID { get; set; }

            [Required]
            public string Base1 { get; set; }

            [Required]
            public virtual Sub Sub { get; set; }
        }

        public class BaseDTO
        {
            public int BaseID { get; set; }
            public string Base1 { get; set; }
            public virtual SubDTO Sub { get; set; }
        }

        public class Sub
        {
            [Key]
            public int BaseId { get; set; }

            [Required]
            public string Sub1 { get; set; }
        }

        public class SubDTO
        {
            public string Sub1 { get; set; }
        }

        public class TestContext : LocalDbContext
        {
            public DbSet<Base> Bases { get; set; }
            public DbSet<Sub> Subs { get; set; }
        }

        public class DatabaseInitializer : DropCreateDatabaseAlways<TestContext>
        {
            protected override void Seed(TestContext testContext)
            {
                testContext.Bases.Add(new Base() { Base1 = "base1", Sub = new Sub() { Sub1 = "sub1" } });

                base.Seed(testContext);
            }
        }


        public class UnitTest(DatabaseFixture databaseFixture) : IntegrationTest<DatabaseInitializer>(databaseFixture)
        {
            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.CreateProjection<Base, BaseDTO>();
                cfg.CreateProjection<Sub, SubDTO>();
            });

            [Fact]
            public void AutoMapperEFRelationsTest()
            {
                using (var context = Fixture.CreateContext())
                {
                    var baseEntitiy = context.Bases.Include(b => b.Sub).FirstOrDefault();
                    baseEntitiy.ShouldNotBeNull();
                    baseEntitiy.BaseID.ShouldBe(1);
                    baseEntitiy.Sub.Sub1.ShouldBe("sub1");
                }

                using (var context = Fixture.CreateContext())
                {
                    var baseDTO = context.Bases.Select(b => new BaseDTO
                    {
                        Base1 = b.Base1,
                        BaseID = b.BaseID,
                        Sub = new SubDTO
                        {
                            Sub1 = b.Sub.Sub1,
                        }
                    }).FirstOrDefault();
                    baseDTO.ShouldNotBeNull();
                    baseDTO.BaseID.ShouldBe(1);
                    baseDTO.Sub.Sub1.ShouldBe("sub1");


                    baseDTO = ProjectTo<BaseDTO>(context.Bases).FirstOrDefault();
                    baseDTO.ShouldNotBeNull();
                    baseDTO.BaseID.ShouldBe(1);
                    baseDTO.Sub.Sub1.ShouldBe("sub1");
                }
            }
            [Fact]
            public void MapShouldThrow() => new Action(() => Mapper.Map<SubDTO>(new Sub())).ShouldThrow<AutoMapperConfigurationException>().Message.ShouldBe("CreateProjection works with ProjectTo, not with Map.");
        }
    }
}