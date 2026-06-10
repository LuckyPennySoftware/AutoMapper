namespace AutoMapper;

[DebuggerDisplay("{DestinationMember.Name}")]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class PropertyMap : MemberMap
{
    private MemberMapDetails _details;
    public PropertyMap(MemberInfo destinationMember, Type destinationMemberType, TypeMap typeMap) : base(typeMap, destinationMemberType) =>
        DestinationMember = destinationMember;
    public PropertyMap(PropertyMap inheritedMappedProperty, TypeMap typeMap) : base(typeMap, inheritedMappedProperty.DestinationType)
    {
        DestinationMember = inheritedMappedProperty.DestinationMember;
        if (DestinationMember.DeclaringType.ContainsGenericParameters)
        {
            DestinationMember = typeMap.DestinationSetters.Single(m => m.Name == DestinationMember.Name);
        }
        if (DestinationType.ContainsGenericParameters)
        {
            DestinationType = DestinationMember.GetMemberType();
        }
        ApplyInheritedPropertyMap(inheritedMappedProperty);
    }
    public PropertyMap(PropertyMap includedMemberMap, TypeMap typeMap, IncludedMember includedMember)
        : this(includedMemberMap, typeMap) => Details.IncludedMember = includedMember.Chain(includedMemberMap.IncludedMember);
    private MemberMapDetails Details => _details ??= new();
    public MemberInfo DestinationMember { get; }
    public override string DestinationName => DestinationMember?.Name;
    public override MemberInfo[] SourceMembers { get; set; } = [];
    public override bool CanBeSet => DestinationMember.CanBeSet();
    public override bool Ignored { get; set; }
    public void ApplyInheritedPropertyMap(PropertyMap inheritedMap)
    {
        ApplyInheritedMap(inheritedMap);
        if (!Ignored && inheritedMap._details != null)
        {
            Details.ApplyInheritedPropertyMap(inheritedMap._details);
        }
    }
    public override IncludedMember IncludedMember => _details?.IncludedMember;
    public override bool? AllowNull { get => _details?.AllowNull; set => Details.AllowNull = value; }
    public int? MappingOrder { get => _details?.MappingOrder; set => Details.MappingOrder = value; }
    public override bool? ExplicitExpansion { get => _details?.ExplicitExpansion; set => Details.ExplicitExpansion = value; }
    public override bool? UseDestinationValue { get => _details?.UseDestinationValue; set => Details.UseDestinationValue = value; }
    public override object NullSubstitute { get => _details?.NullSubstitute; set => Details.NullSubstitute = value; }
    public override LambdaExpression PreCondition { get => _details?.PreCondition; set => Details.PreCondition = value; }
    public override LambdaExpression Condition { get => _details?.Condition; set => Details.Condition = value; }
    public void AddValueTransformation(ValueTransformerConfiguration config) => Details.AddValueTransformation(config);
    public override IReadOnlyCollection<ValueTransformerConfiguration> ValueTransformers => (_details?.ValueTransformers).NullCheck();
    class MemberMapDetails
    {
        public List<ValueTransformerConfiguration> ValueTransformers { get; private set; }
        public bool? AllowNull;
        public int? MappingOrder;
        public bool? ExplicitExpansion;
        public bool? UseDestinationValue;
        public object NullSubstitute;
        public LambdaExpression PreCondition;
        public LambdaExpression Condition;
        public IncludedMember IncludedMember;
        public void ApplyInheritedPropertyMap(MemberMapDetails inheritedMappedProperty)
        {
            AllowNull ??= inheritedMappedProperty.AllowNull;
            Condition ??= inheritedMappedProperty.Condition;
            PreCondition ??= inheritedMappedProperty.PreCondition;
            NullSubstitute ??= inheritedMappedProperty.NullSubstitute;
            MappingOrder ??= inheritedMappedProperty.MappingOrder;
            UseDestinationValue ??= inheritedMappedProperty.UseDestinationValue;
            ExplicitExpansion ??= inheritedMappedProperty.ExplicitExpansion;
            if (inheritedMappedProperty.ValueTransformers != null)
            {
                ValueTransformers ??= [];
                ValueTransformers.InsertRange(0, inheritedMappedProperty.ValueTransformers);
            }
        }
        public void AddValueTransformation(ValueTransformerConfiguration valueTransformerConfiguration)
        {
            ValueTransformers ??= [];
            ValueTransformers.Add(valueTransformerConfiguration);
        }
    }
}