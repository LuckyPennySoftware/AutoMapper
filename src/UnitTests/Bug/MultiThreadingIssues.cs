namespace AutoMapper.UnitTests.Bug;
public class MultiThreadingIssues
{
    public class Type1
    {
        public string FirstName;
        public string MiddleName;
        public string LastName;
        public int Age;
    }

    public class Type1Point3
    {
        public string FirstName;
        public string MiddleName;
        public string LastName;
    }

    public class Type1Point2
    {
        public string FirstName;
        public string MiddleName;
    }

    public class Type1Point1
    {
        public string FirstName;
    }

    public class DestType
    {
        public string FirstName;
        public string MiddleName;
        public string LastName;
        public int Age;
    }

    static int _done;
    readonly ManualResetEvent _allDone = new ManualResetEvent(false);

    [Fact]
    public void ShouldMapToNewISet()
    {
        const int threadCount = 13;

        for(int i = 0; i < threadCount; i++)
        {
            Task.Factory.StartNew(doMapping).ContinueWith(
                a =>
                {
                    if(Interlocked.Increment(ref _done) == threadCount)
                    {
                        _allDone.Set();
                    }

                });
        }

        _allDone.WaitOne(TimeSpan.FromSeconds(10));
    }

    static void doMapping()
    {
        var source = createSource();

        Debug.WriteLine(@"Mapping {0} on thread {1}", source.GetType(), Thread.CurrentThread.ManagedThreadId);

        var config = new MapperConfiguration(cfg => cfg.CreateMap(source.GetType(), typeof(DestType)));

        DestType t2 = (DestType)config.CreateMapper().Map(source, source.GetType(), typeof(DestType));
    }

    static readonly Random _random = new Random();

    static object createSource()
    {

        int n = _random.Next(0, 4);

        if(n == 0)
        {
            return new Type1
            {
                Age = 12,
                FirstName = @"Fred",
                LastName = @"Smith",
                MiddleName = @"G"
            };
        }
        if(n == 1)
        {
            return new Type1Point1()
            {
                FirstName = @"Fred",
            };

        }
        if(n == 2)
        {
            return new Type1Point2()
            {
                FirstName = @"Fred",
                MiddleName = @"G"
            };

        }
        if(n == 3)
        {
            return new Type1Point3()
            {
                FirstName = @"Fred",
                LastName = @"Smith",
                MiddleName = @"G"
            };

        }

        throw new Exception();
    }
}

public class ResolveWithGenericMap
{
    public class SomeDtoA
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string PropertyX { get; set; }
    }

    public class SomeDtoB
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string PropertyX { get; set; }
    }

    public class SomeDtoC
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string PropertyX { get; set; }
    }

    public class SomeDtoD
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string PropertyX { get; set; }
    }

    public class SomeDtoE
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string PropertyX { get; set; }
    }

    public class SomeDtoF
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string PropertyX { get; set; }
    }

    public class SomeDtoG
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string PropertyX { get; set; }
    }

    public class SomeDtoH
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string PropertyX { get; set; }
    }

    public class SomeDtoI
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string PropertyX { get; set; }
    }

    public class SomeDtoJ
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string PropertyX { get; set; }
    }

    public class SomeDtoK
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string PropertyX { get; set; }
    }

    public class SomeDtoL
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string PropertyX { get; set; }
    }

    public class SomeDtoM
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string PropertyX { get; set; }
    }

    public class SomeDtoN
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string PropertyX { get; set; }
    }

    public class SomeDtoO
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string PropertyX { get; set; }
    }

    public class SomeDtoP
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string PropertyX { get; set; }
    }

    public class SomeEntityA
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string Property11 { get; set; }
    }

    public class SomeEntityB
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string Property11 { get; set; }
    }

    public class SomeEntityC
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string Property11 { get; set; }
    }

    public class SomeEntityD
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string Property11 { get; set; }
    }

    public class SomeEntityE
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string Property11 { get; set; }
    }

    public class SomeEntityF
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string Property11 { get; set; }
    }

    public class SomeEntityG
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string Property11 { get; set; }
    }

    public class SomeEntityH
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string Property11 { get; set; }
    }

    public class SomeEntityI
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string Property11 { get; set; }
    }

    public class SomeEntityJ
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string Property11 { get; set; }
    }

    public class SomeEntityK
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string Property11 { get; set; }
    }

    public class SomeEntityL
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string Property11 { get; set; }
    }

    public class SomeEntityM
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string Property11 { get; set; }
    }

    public class SomeEntityN
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string Property11 { get; set; }
    }

    public class SomeEntityO
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string Property11 { get; set; }
    }

    public class SomeEntityP
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string Property11 { get; set; }
    }

    public class Entity<T>
    {
        public T Data { get; set; }
    }

    public class Dto<T>
    {
        public T Data { get; set; }
        public int Value { get; set; }
    }

    [Fact]
    public async Task Should_work()
    {
        var sourceType = typeof(Entity<>);
        var destinationType = typeof(Dto<>);
        var c = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap(sourceType, destinationType).ForMember("Value", o => o.Ignore());
        });
        var types = new[]{
            new[] { typeof(SomeEntityA), typeof(SomeDtoA) },
            new[] { typeof(SomeEntityB), typeof(SomeDtoB) },
            new[] { typeof(SomeEntityC), typeof(SomeDtoC) },
            new[] { typeof(SomeEntityD), typeof(SomeDtoD) },
            new[] { typeof(SomeEntityE), typeof(SomeDtoE) },
            new[] { typeof(SomeEntityF), typeof(SomeDtoF) },
            new[] { typeof(SomeEntityG), typeof(SomeDtoG) },
            new[] { typeof(SomeEntityH), typeof(SomeDtoH) },
            new[] { typeof(SomeEntityI), typeof(SomeDtoI) },
            new[] { typeof(SomeEntityJ), typeof(SomeDtoJ) },
            new[] { typeof(SomeEntityK), typeof(SomeDtoK) },
            new[] { typeof(SomeEntityL), typeof(SomeDtoL) },
            new[] { typeof(SomeEntityM), typeof(SomeDtoM) },
            new[] { typeof(SomeEntityN), typeof(SomeDtoN) },
            new[] { typeof(SomeEntityO), typeof(SomeDtoO) },
            new[] { typeof(SomeEntityP), typeof(SomeDtoP) },
        };
        var tasks =
            types
            .Concat(types.Select(t => t.Reverse().ToArray()))
            .Select(t=>(SourceType: sourceType.MakeGenericType(t[0]), DestinationType: destinationType.MakeGenericType(t[1])))
            .ToArray()
            .Select(s => Task.Factory.StartNew(() => c.ResolveTypeMap(s.SourceType, s.DestinationType)))
            .ToArray();
        await Task.WhenAll(tasks);
    }
}

public class ResolveGenericTypeMapThreadingIssues
{
    public class SomeDtoA
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string PropertyX { get; set; }
    }

    public class SomeDtoB
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string PropertyX { get; set; }
    }

    public class SomeDtoC
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string PropertyX { get; set; }
    }

    public class SomeDtoD
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string PropertyX { get; set; }
    }

    public class SomeDtoE
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string PropertyX { get; set; }
    }

    public class SomeDtoF
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string PropertyX { get; set; }
    }

    public class SomeDtoG
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string PropertyX { get; set; }
    }

    public class SomeDtoH
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string PropertyX { get; set; }
    }

    public class SomeDtoI
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string PropertyX { get; set; }
    }

    public class SomeDtoJ
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string PropertyX { get; set; }
    }

    public class SomeDtoK
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string PropertyX { get; set; }
    }

    public class SomeDtoL
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string PropertyX { get; set; }
    }

    public class SomeDtoM
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string PropertyX { get; set; }
    }

    public class SomeDtoN
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string PropertyX { get; set; }
    }

    public class SomeDtoO
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string PropertyX { get; set; }
    }

    public class SomeDtoP
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string PropertyX { get; set; }
    }

    public class SomeEntityA
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string Property11 { get; set; }
    }

    public class SomeEntityB
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string Property11 { get; set; }
    }

    public class SomeEntityC
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string Property11 { get; set; }
    }

    public class SomeEntityD
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string Property11 { get; set; }
    }

    public class SomeEntityE
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string Property11 { get; set; }
    }

    public class SomeEntityF
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string Property11 { get; set; }
    }

    public class SomeEntityG
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string Property11 { get; set; }
    }

    public class SomeEntityH
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string Property11 { get; set; }
    }

    public class SomeEntityI
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string Property11 { get; set; }
    }

    public class SomeEntityJ
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string Property11 { get; set; }
    }

    public class SomeEntityK
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string Property11 { get; set; }
    }

    public class SomeEntityL
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string Property11 { get; set; }
    }

    public class SomeEntityM
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string Property11 { get; set; }
    }

    public class SomeEntityN
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string Property11 { get; set; }
    }

    public class SomeEntityO
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string Property11 { get; set; }
    }

    public class SomeEntityP
    {
        public string Property1 { get; set; }
        public string Property21 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }
        public string Property6 { get; set; }
        public string Property7 { get; set; }
        public string Property8 { get; set; }
        public string Property9 { get; set; }
        public string Property10 { get; set; }
        public string Property11 { get; set; }
    }

    public class Entity<T>
    {
        public T Data { get; set; }
    }

    public class Dto<T>
    {
        public T Data { get; set; }
        public int Value { get; set; }
    }

    [Fact]
    public async Task Should_work()
    {
        var sourceType = typeof(Entity<>);
        var destinationType = typeof(Dto<>);
        var c = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap(sourceType, destinationType).ForMember("Value", o => o.Ignore());
        });
        var mapper = c.CreateMapper();
        var types = new[]{
            new[] { typeof(SomeEntityA), typeof(SomeDtoA) },
            new[] { typeof(SomeEntityB), typeof(SomeDtoB) },
            new[] { typeof(SomeEntityC), typeof(SomeDtoC) },
            new[] { typeof(SomeEntityD), typeof(SomeDtoD) },
            new[] { typeof(SomeEntityE), typeof(SomeDtoE) },
            new[] { typeof(SomeEntityF), typeof(SomeDtoF) },
            new[] { typeof(SomeEntityG), typeof(SomeDtoG) },
            new[] { typeof(SomeEntityH), typeof(SomeDtoH) },
            new[] { typeof(SomeEntityI), typeof(SomeDtoI) },
            new[] { typeof(SomeEntityJ), typeof(SomeDtoJ) },
            new[] { typeof(SomeEntityK), typeof(SomeDtoK) },
            new[] { typeof(SomeEntityL), typeof(SomeDtoL) },
            new[] { typeof(SomeEntityM), typeof(SomeDtoM) },
            new[] { typeof(SomeEntityN), typeof(SomeDtoN) },
            new[] { typeof(SomeEntityO), typeof(SomeDtoO) },
            new[] { typeof(SomeEntityP), typeof(SomeDtoP) },
        };
        var tasks =
            types
            .Concat(types.Select(t => t.Reverse().ToArray()))
            .Select(t=>(SourceType: sourceType.MakeGenericType(t[0]), DestinationType: destinationType.MakeGenericType(t[1])))
            .ToArray()
            .Select(s => Task.Factory.StartNew(() => mapper.Map(null, s.SourceType, s.DestinationType)))
            .ToArray();
        await Task.WhenAll(tasks);
    }
}