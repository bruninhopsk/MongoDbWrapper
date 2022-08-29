# MongoDbWrapper

1. Resolvendo a injeção de dependencia:

var mongoConfig = new MongoConfig()
{
    ConnectionString = configuration["MongoDb:ConnectionString"],
    Database = configuration["MongoDb:DataBase"]
};

services.AddSingleton<IOptions<MongoConfig>>(Options.Create(mongoConfig));
services.AddSingleton<IMongoDbClient<Entidade, string>, MongoDbClient<Entidade, string>>();

===========================================================================================
2. Usando a Lib em um repositório:

public class MongoDbRepository : IMongoDbRepository
{
    private IMongoDbClient<Entidade, string> EntidadeRepository { get; }

    public MongoDbRepository(IMongoDbClient<Entidade, string> entidadeRepository)
    {
        EntidadeRepository = entidadeRepository;
    }

    public List<Entidade> BuscarTodosRegistros()
    {
        var registros = MongoDbRepository.GetAll();

        return registros;
    }
}