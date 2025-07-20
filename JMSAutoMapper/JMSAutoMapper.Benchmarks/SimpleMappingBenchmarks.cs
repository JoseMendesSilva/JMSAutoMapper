using AutoMapper;
using BenchmarkDotNet.Attributes;
using JMSAutoMapper;

namespace JMSAutoMapper.Benchmarks
{
    [MemoryDiagnoser]
    [RankColumn]
    public class SimpleMappingBenchmarks
    {
        private JMSMapper _jmsMapper;
        private AutoMapper.IMapper _autoMapper;
        private Source _source;

        [GlobalSetup]
        public void Setup()
        {
            // JMSAutoMapper setup
            var jmsConfig = new MapperConfiguration();
            jmsConfig.CreateMap<Source, Destination>();
            _jmsMapper = new JMSMapper(jmsConfig);

            // AutoMapper setup
            var autoMapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Destination>();
            });
            _autoMapper = autoMapperConfig.CreateMapper();

            _source = new Source
            {
                Id = 1,
                Name = "Test Name",
                Value = 123.45m,
                CreatedDate = DateTime.UtcNow
            };
        }

        [Benchmark]
        public Destination JMSAutoMapper_Map()
        {
            return _jmsMapper.Map<Destination>(_source);
        }

        [Benchmark]
        public Destination AutoMapper_Map()
        {
            return _autoMapper.Map<Destination>(_source);
        }

        [Benchmark]
        public Destination Manual_Map()
        {
            return new Destination
            {
                Id = _source.Id,
                Name = _source.Name,
                Value = _source.Value,
                CreatedDate = _source.CreatedDate
            };
        }
    }

    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
