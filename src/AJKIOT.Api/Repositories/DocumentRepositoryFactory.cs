namespace AJKIOT.Api.Repositories
{
    public class DocumentRepositoryFactory : IDocumentRepositoryFactory
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public DocumentRepositoryFactory(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public IDocumentRepository CreateDocumentRepository()
        {
            var scope = _serviceScopeFactory.CreateScope();
            return scope.ServiceProvider.GetRequiredService<IDocumentRepository>();
        }
    }
}
