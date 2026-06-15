using AutoMapper;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging.Abstractions;

namespace JMSAutoMapper.Benchmarks
{
    [MemoryDiagnoser]
    public class MappingBenchmark
    {
        private Abstractions.IMapper _jmsMapper = null!;
        private IMapper _autoMapper = null!;
        private Source _source = null!;
        private List<Source> _sourceList = null!;
        private ComplexSource _complexSource = null!;

        [GlobalSetup]
        public void Setup()
        {
            var jmsConfig = new JMSAutoMapper.Configuration.MapperConfiguration();
            jmsConfig.CreateMap<Source, Destination>();
            jmsConfig.CreateMap<ComplexSource, ComplexDestination>();
            _jmsMapper = new JMSAutoMapper.Core.JMSMapper(jmsConfig);



            var autoMapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Destination>();
                cfg.CreateMap<ComplexSource, ComplexDestination>();
            }, NullLoggerFactory.Instance);
            
            _autoMapper = autoMapperConfig.CreateMapper();

            _source = new Source { Id = 1, Name = "Test" };
            _sourceList = Enumerable.Range(0, 100).Select(i => new Source { Id = i, Name = $"Name {i}" }).ToList();
            _complexSource = new ComplexSource { 
                Id = 1, 
                Sub = new Source { Id = 2, Name = "Nested" } 
            };
        }

        [Benchmark]
        public Destination JmsMapper_SimpleMap()
        {
            return _jmsMapper.Map<Destination>(_source);
        }

        [Benchmark]
        public Destination AutoMapper_SimpleMap()
        {
            return _autoMapper.Map<Destination>(_source);
        }

        [Benchmark]
        public List<Destination> JmsMapper_CollectionMap()
        {
            return _jmsMapper.Map<List<Destination>>(_sourceList);
        }

        [Benchmark]
        public List<Destination> AutoMapper_CollectionMap()
        {
            return _autoMapper.Map<List<Destination>>(_sourceList);
        }

        [Benchmark]
        public ComplexDestination JmsMapper_ComplexMap()
        {
            return _jmsMapper.Map<ComplexDestination>(_complexSource);
        }

        [Benchmark]
        public ComplexDestination AutoMapper_ComplexMap()
        {
            return _autoMapper.Map<ComplexDestination>(_complexSource);
        }
    }

    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ComplexSource
    {
        public int Id { get; set; }
        public Source Sub { get; set; }
    }

    public class ComplexDestination
    {
        public int Id { get; set; }
        public Destination Sub { get; set; }
    }
}
