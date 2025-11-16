using Microsoft.Extensions.DependencyInjection;

namespace wmine.Services
{
    public static class AppServiceProvider
    {
        private static IServiceProvider? _provider;

        public static void SetProvider(IServiceProvider provider)
        {
            _provider = provider;
        }

        public static IServiceProvider? Provider => _provider;

        public static T? GetService<T>() where T : class
        {
            return _provider?.GetService<T>();
        }
    }
}
