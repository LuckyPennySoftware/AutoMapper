# Projection

Projection transforms a source to a destination beyond flattening the object model.  Without extra configuration, AutoMapper requires a flattened destination to match the source type's naming structure.  When you want to project source values into a destination that does not exactly match the source structure, you must specify custom member mapping definitions.  For example, we might want to turn this source structure:
```c#
public class CalendarEvent
{
	public DateTime Date { get; set; }
	public string Title { get; set; }
}
```

Into something that works better for an input form on a web page:

```c#
public class CalendarEventForm
{
	public DateTime EventDate { get; set; }
	public int EventHour { get; set; }
	public int EventMinute { get; set; }
	public string Title { get; set; }
}
```

Because the names of the destination properties do not exactly match the source property (`CalendarEvent.Date` would need to be `CalendarEventForm.EventDate`), we need to specify custom member mappings in our type map configuration:

```c#
// Model
var calendarEvent = new CalendarEvent
{
	Date = new DateTime(2008, 12, 15, 20, 30, 0),
	Title = "Company Holiday Party"
};

// Configure AutoMapper
var configuration = new MapperConfiguration(cfg =>
  cfg.CreateMap<CalendarEvent, CalendarEventForm>()
	.ForMember(dest => dest.EventDate, opt => opt.MapFrom(src => src.Date.Date))
	.ForMember(dest => dest.EventHour, opt => opt.MapFrom(src => src.Date.Hour))
	.ForMember(dest => dest.EventMinute, opt => opt.MapFrom(src => src.Date.Minute)), loggerFactory);

// Perform mapping
CalendarEventForm form = mapper.Map<CalendarEvent, CalendarEventForm>(calendarEvent);

form.EventDate.ShouldEqual(new DateTime(2008, 12, 15));
form.EventHour.ShouldEqual(20);
form.EventMinute.ShouldEqual(30);
form.Title.ShouldEqual("Company Holiday Party");
```

Each custom member configuration uses an action delegate to configure each individual member.  In the above example, we used the `MapFrom` option to perform custom source-to-destination member mappings.  The `MapFrom` method takes a lambda expression as a parameter, which is then evaluated later during mapping.  The `MapFrom` expression can be any `Func<TSource, object>` lambda expression.
