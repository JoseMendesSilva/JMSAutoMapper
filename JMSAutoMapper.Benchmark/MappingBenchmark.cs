using AutoMapper;
using BenchmarkDotNet.Attributes;

namespace JMSAutoMapper.Benchmark
{
    [MemoryDiagnoser]
    public class MappingBenchmark
    {
        private JMSAutoMapper.IMapper _jmsMapper;
        private AutoMapper.IMapper _autoMapper;
        private Source _source;

        [GlobalSetup]
        public void Setup()
        {
            var jmsConfig = new JMSAutoMapper.MapperConfiguration();
            jmsConfig.CreateMap<Source, Destination>();
            _jmsMapper = new JMSAutoMapper.JMSMapper(jmsConfig);

            var autoMapperConfig = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Destination>();
            });
            _autoMapper = autoMapperConfig.CreateMapper();

            _source = new Source { Id = 1, Name = "Test" };
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
}
