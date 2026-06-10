# AutoMapper.Extensions.EnumMapping

The built-in enum mapper is not configurable, it can only be replaced. Alternatively, AutoMapper supports convention based mapping of enum values in a separate package [AutoMapper.Extensions.EnumMapping](https://www.nuget.org/packages/AutoMapper.Extensions.EnumMapping/).

### Usage

For method `CreateMap` this library provide a `ConvertUsingEnumMapping` method. This method add all default mappings from source to destination enum values.

If you want to change some mappings, then you can use `MapValue` method. This is a chainable method.

Default the enum values are mapped by value (explicitly: `MapByValue()`), but it is possible to map by name calling  `MapByName()`.

```c#
using AutoMapper.Extensions.EnumMapping;

public enum Source
{
    Default = 0,
    First = 1,
    Second = 2
}

public enum Destination
{
    Default = 0,
    Second = 2
}

internal class YourProfile : Profile
{
    public YourProfile()
    {
        CreateMap<Source, Destination>()
            .ConvertUsingEnumMapping(opt => opt
		        // optional: .MapByValue() or MapByName(), without configuration MapByValue is used
		        .MapValue(Source.First, Destination.Default)
            )
            .ReverseMap(); // to support Destination to Source mapping, including custom mappings of ConvertUsingEnumMapping
    }
}
    ...
```

### Default Convention

The package [AutoMapper.Extensions.EnumMapping](https://www.nuget.org/packages/AutoMapper.Extensions.EnumMapping/) will map all values from Source type to Destination type if both enum types have the same value (or by name or by value). All Source enum values which have no Target equivalent, will throw an exception if EnumMappingValidation is enabled.

### ReverseMap Convention

For method `ReverseMap` the same convention is used as for default mappings, but it also respects override enum value mappings if possible.

The following steps determines the reversed overrides:

1) Create mappings for `Source` to `Destination` (default convention), including custom overrides.

2) Create mappings for `Destination` to `Source` (default convention), without custom overrides (must be determined)

3) The mappings from step 1 will be used to determine the overrides for the `ReverseMap`. 
   Therefore the mappings are grouped by `Destination` value.
    
        3a) if there is a matching `Source` value for the `Destination` value, then that mapping is preferred and no override is needed

    It is possible that a `Destination` value has multiple `Source` values specified by override mappings.
    
    We have to determine which `Source` value will be the new `Destination` for the current `Destination` value (which is the new `Source` value)

    For every `Source` value per grouped `Destination` value:

        3b) if the `Source` enum value does not exists in the `Destination` enum type, then that mapping cannot reversed
    
        3c) if there is a `Source` value which is not a `Destination` part of the mappings from step 1, then that mapping cannot reversed
    
        3d) if the `Source` value is not excluded by option b and c, the that `Source` value is the new `Destination` value.

4) All overrides which are determined in step 3 will be applied to mappings from step 2.

5) Finally, the custom mappings provided to method `ReverseMap` will be applied.

### Testing

[AutoMapper](https://www.nuget.org/packages/AutoMapper/) provides a nice tooling for validating typemaps. This package adds an extra `EnumMapperConfigurationExpressionExtensions.EnableEnumMappingValidation` extension method to extend the existing `AssertConfigurationIsValid()` method to validate also the enum mappings.

To enable testing the enum mapping configuration:

```c#

public class MappingConfigurationsTests
{
    [Fact]
    public void WhenProfilesAreConfigured_ItShouldNotThrowException()
    {
        // Arrange
        var config = new MapperConfiguration(configuration =>
        {
            configuration.EnableEnumMappingValidation();

            configuration.AddMaps(typeof(AssemblyInfo).GetTypeInfo().Assembly);
        }, loggerFactory);
		
        // Assert
        config.AssertConfigurationIsValid();
    }
}
```
