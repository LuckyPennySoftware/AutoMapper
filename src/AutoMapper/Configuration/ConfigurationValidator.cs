using AutoMapper.Internal.Mappers;
namespace AutoMapper.Configuration;

[EditorBrowsable(EditorBrowsableState.Never)]
public class ConfigurationValidator(IGlobalConfiguration config)
{
    IGlobalConfigurationExpression Expression => ((MapperConfiguration)config).ConfigurationExpression;
    public void AssertConfigurationExpressionIsValid(TypeMap[] typeMaps)
    {
        var duplicateTypeMapConfigs = Expression.Profiles.Append((Profile)Expression)
            .SelectMany(p => p.TypeMapConfigs, (profile, typeMap) => (profile, typeMap))
            .GroupBy(x => x.typeMap.Types)
            .Where(g => g.Count() > 1)
            .Select(g => (TypePair: g.Key, ProfileNames: g.Select(tmc => tmc.profile.ProfileName).ToArray()))
            .Select(g => new DuplicateTypeMapConfigurationException.TypeMapConfigErrors(g.TypePair, g.ProfileNames))
            .ToArray();
        if (duplicateTypeMapConfigs.Length != 0)
        {
            throw new DuplicateTypeMapConfigurationException(duplicateTypeMapConfigs);
        }
        AssertConfigurationIsValid(typeMaps);
    }
    public void AssertConfigurationIsValid(TypeMap[] typeMaps)
    {
        List<Exception> configExceptions = [];
        var badTypeMaps =
            (from typeMap in typeMaps
             where typeMap.ShouldCheckForValid
             let unmappedPropertyNames = typeMap.GetUnmappedPropertyNames()
             let canConstruct = typeMap.PassesCtorValidation
             where unmappedPropertyNames.Length > 0 || !canConstruct
             select new AutoMapperConfigurationException.TypeMapConfigErrors(typeMap, unmappedPropertyNames, canConstruct)).ToArray();
        if (badTypeMaps.Length > 0)
        {
            configExceptions.Add(new AutoMapperConfigurationException(badTypeMaps));
        }
        HashSet<TypeMap> typeMapsChecked = [];
        foreach (var typeMap in typeMaps)
        {
            DryRunTypeMap(typeMap.Types, typeMap, null);
        }
        if (configExceptions.Count > 1)
        {
            throw new AggregateException(configExceptions);
        }
        if (configExceptions.Count > 0)
        {
            throw configExceptions[0];
        }
        void DryRunTypeMap(TypePair types, TypeMap typeMap, MemberMap memberMap)
        {
            if (typeMap == null)
            {
                if (types.ContainsGenericParameters)
                {
                    return;
                }
                typeMap = config.ResolveTypeMap(types);
            }
            if (typeMap != null)
            {
                if (typeMapsChecked.Add(typeMap) && Validate(new(types, memberMap, configExceptions, typeMap)) && typeMap.ShouldCheckForValid)
                {
                    CheckPropertyMaps(typeMap);
                }
            }
            else
            {
                var mapperToUse = config.FindMapper(types);
                if (mapperToUse == null)
                {
                    configExceptions.Add(new AutoMapperConfigurationException(memberMap.TypeMap.Types) { MemberMap = memberMap });
                    return;
                }
                if (Validate(new(types, memberMap, configExceptions, ObjectMapper: mapperToUse)) && mapperToUse.GetAssociatedTypes(types) is TypePair newTypes &&
                    newTypes != types)
                {
                    DryRunTypeMap(newTypes, null, memberMap);
                }
            }
        }
        void CheckPropertyMaps(TypeMap typeMap)
        {
            foreach (var memberMap in typeMap.MemberMaps)
            {
                if (memberMap.Ignored || (memberMap is PropertyMap && typeMap.ConstructorParameterMatches(memberMap.DestinationName)))
                {
                    continue;
                }
                var sourceType = memberMap.SourceType;
                // when we don't know what the source type is, bail
                if (sourceType.IsGenericParameter || sourceType == typeof(object))
                {
                    continue;
                }
                DryRunTypeMap(new(sourceType, memberMap.DestinationType), null, memberMap);
            }
        }
        bool Validate(ValidationContext context)
        {
            try
            {
                foreach (var validator in Expression.Validators)
                {
                    validator(context);
                }
            }
            catch (Exception e)
            {
                configExceptions.Add(e);
                return false;
            }
            return true;
        }
    }
}
public readonly record struct ValidationContext(TypePair Types, MemberMap MemberMap, List<Exception> Exceptions, TypeMap TypeMap = null, IObjectMapper ObjectMapper = null);