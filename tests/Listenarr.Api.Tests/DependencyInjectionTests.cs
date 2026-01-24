// csharp
using Listenarr.Api.Services;
using Listenarr.Infrastructure.Extensions; // extension lives in Infrastructure project
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Listenarr.Api.Tests
{
    public class DependencyInjectionTests
    {
        [Fact]
        public void InfrastructureRegistrations_ResolveIAudiobookRepository()
        {
            var services = new ServiceCollection();

            // Register an in-memory DbContext so repository dependencies are satisfied.
            services.AddDbContext<ListenArrDbContext>(options =>
                options.UseInMemoryDatabase("di-test-db"));

            // Register infrastructure implementations (the extension lives in Infrastructure project)
            services.AddListenarrInfrastructure();

            using var sp = services.BuildServiceProvider(validateScopes: true);

            // Resolve scoped services from a created scope to satisfy DI validation rules.
            using var scope = sp.CreateScope();
            var repo = scope.ServiceProvider.GetService<IAudiobookRepository>();

            Assert.NotNull(repo);
        }
    }
}
