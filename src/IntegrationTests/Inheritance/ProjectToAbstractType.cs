using System.Collections.ObjectModel;

namespace AutoMapper.IntegrationTests.Inheritance;

public class ProjectToAbstractType(DatabaseFixture databaseFixture) : IntegrationTest<ProjectToAbstractType.DatabaseInitializer>(databaseFixture)
{
    ITypeA[] _destinations;

    public interface ITypeA
    {
        int ID { get; set; }
        string Name { get; set; }
    }

    public class ConcreteTypeA : ITypeA
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class DbEntityA
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.EntityA.AddRange(new[]
            {
                new DbEntityA { Name = "Alain Brito"},
                new DbEntityA { Name = "Jimmy Bogard"},
                new DbEntityA { Name = "Bill Gates"}
            });
            base.Seed(context);
        }
    }
    public class Context : LocalDbContext
    {
        public DbSet<DbEntityA> EntityA { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<DbEntityA, ITypeA>().As<ConcreteTypeA>();
        cfg.CreateProjection<DbEntityA, ConcreteTypeA>();
    });

    [Fact]
    public void Should_project_to_abstract_type()
    {
        using(var context = Fixture.CreateContext())
        {
            _destinations = ProjectTo<ITypeA>(context.EntityA).ToArray();
        }
        _destinations.Length.ShouldBe(3);
        _destinations[2].Name.ShouldBe("Bill Gates");
    }
}

public class ProjectToInterface(DatabaseFixture databaseFixture) : IntegrationTest<ProjectToInterface.DatabaseInitializer>(databaseFixture)
{
    //Data Objects
    public class DataLayer
    {
        public class BaseDbObject
        {
            public Guid Id { get; set; }
        }
        public class Calendar : BaseDbObject
        {
            public string Name { get; set; }
            public Guid? ReferenceId { get; set; }
            public Guid BusinessUnitId { get; set; }
            public DateTime? ValidFrom { get; set; }
            public DateTime? ValidTo { get; set; }

            public virtual Calendar Reference { get; set; }
            public virtual ICollection<CalendarDay> Days { get; set; }
        }
        public class CalendarDay : BaseDbObject
        {
            public DateTime Date { get; set; }
            public Guid DayTypeId { get; set; }
            public Guid CalendarId { get; set; }
            public bool Cancel { get; set; }
            public bool Deleted { get; set; }

            public virtual ValidityDayType DayType { get; set; }
            public virtual Calendar Calendar { get; set; }
        }

        public class ValidityDayType : BaseDbObject
        {
            public string Name { get; set; }
            public string Acronym { get; set; }

            public virtual ICollection<CalendarDay> Days { get; set; }
            //        public virtual ICollection<ValiditySignDayType> Signs { get; set; }
        }
    }

    public interface ICalendar
    {
        Guid Id { get; }
        string Name { get; }
        Guid BusinessUnitId { get; }

        ICalendar Reference { get; }
        DateTime? ValidFrom { get; }
        DateTime? ValidTo { get; }

        ICollection<ICalendarDay> Days { get; set; }
    }

    public interface ICalendarDay
    {
        Guid Id { get; }
        DateTime Date { get; }
        IValidityDayType DayType { get; }
        ICalendar Calendar { get; }
        bool Cancel { get; }
        bool Deleted { get; }
    }

    public interface IValidityDayType
    {
        Guid Id { get; }

        string Name { get; }

        string Acronym { get; }

        //        ICollection<IValiditySignDayType> Signs { get; internal set; } = ImmutableList<IValiditySignDayType>.Empty;

        ICollection<ICalendarDay> Days { get; }
    }

    //Domain Models
    public class Calendar : ICalendar
    {
        public Guid Id { get; internal set; }
        public string Name { get; internal set; }
        public Guid BusinessUnitId { get; internal set; }

        public ICalendar Reference { get; internal set; }
        public DateTime? ValidFrom { get; internal set; }
        public DateTime? ValidTo { get; internal set; }

        public virtual ICollection<ICalendarDay> Days { get; set; }

        internal Calendar()
        {

        }

        public Calendar(string name, Guid businessUnitId, ICalendar reference, DateTime? validFrom, DateTime? validTo)
        {
            if(businessUnitId == Guid.Empty) throw new ArgumentException();

            Name = name;
            BusinessUnitId = businessUnitId;
            Reference = reference;
            ValidFrom = validFrom;
            ValidTo = validTo;
        }
    }

    public class CalendarDay : ICalendarDay
    {
        public Guid Id { get; internal set; }
        public DateTime Date { get; internal set; }
        public IValidityDayType DayType { get; internal set; }
        public ICalendar Calendar { get; internal set; }
        public bool Cancel { get; private set; }
        public bool Deleted { get; private set; }

        internal CalendarDay()
        {

        }

        public CalendarDay(DateTime date, IValidityDayType dayType, ICalendar calendar)
        {
            DayType = dayType ?? throw new ArgumentNullException();
            Calendar = calendar ?? throw new ArgumentNullException();
            Date = date;
        }

        public void SetDeleted()
        {
            Deleted = true;
        }

        public void SetCancel()
        {
            Cancel = true;
        }
    }

    public class ValidityDayType : IValidityDayType
    {
        public Guid Id { get; internal set; }

        public string Name { get; internal set; }

        public string Acronym { get; internal set; }

        public ICollection<ICalendarDay> Days { get; internal set; } = new List<ICalendarDay>();

        internal ValidityDayType()
        {

        }

        public ValidityDayType(string name, string acronym)
        {
            Name = name;
            Acronym = acronym;
        }

        public ICalendarDay ApplyToDay(DateTime date, ICalendar calendar)
        {
            var day = new CalendarDay(date, this, calendar);
            return day;
        }

        public IEnumerable<ICalendarDay> GetCalendarDays(ICalendar calendar)
        {
            if(calendar == null)
                throw new ArgumentNullException();

            return Days.Where(d => d.Calendar == calendar);
        }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.Calendars.AddRange(CreateCalendarList());
        }

        private static List<DataLayer.Calendar> CreateCalendarList()
        {
            var dayType = new DataLayer.ValidityDayType()
            {
                Name = "WorkDays",
                Acronym = "WD",
            };

            var day = new DataLayer.CalendarDay()
            {
                Id = new Guid(),
                Cancel = true,
                Deleted = false,
                DayType = dayType,
                Date = DateTime.Parse("2018-03-31")
            };

            var cal1 = new DataLayer.Calendar()
            {
                Id = Guid.NewGuid(),
                Name = "Regional 2018",
                BusinessUnitId = Guid.NewGuid(),
                ValidFrom = DateTime.Parse("2018-01-01"),
                ValidTo = null,
                Days = new Collection<DataLayer.CalendarDay>()
                {
                    day
                }
            };

            var cal2 = new DataLayer.Calendar()
            {
                Id = Guid.NewGuid(),
                Name = "City 2018",
                ReferenceId = cal1.Id,
                Reference = cal1,
                BusinessUnitId = Guid.NewGuid(),
                ValidFrom = DateTime.Parse("2018-01-01"),
                ValidTo = null,
                Days = new Collection<DataLayer.CalendarDay>()
            };

            var dataCalendars = new List<DataLayer.Calendar>()
            {
                cal1, cal2
            };
            return dataCalendars;
        }
    }

    public class Context : LocalDbContext
    {
        public DbSet<DataLayer.Calendar> Calendars { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.AddProfile<MyProfile>());

    [Fact]
    public void Should_project_to_abstract_type()
    {
        using(var context = Fixture.CreateContext())
        {
            var domainCalendars = ProjectTo<ICalendar>(context.Calendars).ToList();
            domainCalendars.Count.ShouldBe(2);
        }
    }

    public class MyProfile : Profile
    {
        public MyProfile()
        {
            DisableConstructorMapping();

            CreateMap<DataLayer.Calendar, ICalendar>().As<Calendar>();
            CreateProjection<DataLayer.Calendar, Calendar>();

            CreateMap<DataLayer.CalendarDay, ICalendarDay>().As<CalendarDay>();
            CreateProjection<DataLayer.CalendarDay, CalendarDay>();
            //.ForMember(d => d.DayType, opt => opt.Ignore());

            //Include to mapping -> this causes the exception!
            CreateMap<DataLayer.ValidityDayType, IValidityDayType>().As<ValidityDayType>();
            CreateProjection<DataLayer.ValidityDayType, ValidityDayType>();

            CreateProjection<ICalendar, DataLayer.Calendar>();

            CreateProjection<ICalendarDay, DataLayer.CalendarDay>();

            CreateProjection<IValidityDayType, DataLayer.ValidityDayType>();
        }
    }
}