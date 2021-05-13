using IPA.Logging;
using SiraUtil;
using Zenject;

namespace SoftRestart.Installers
{
    public class PluginAppInstaller : Installer
    {
        private readonly PluginConfig _config;
        private readonly Logger _logger;

        public PluginAppInstaller(PluginConfig config, Logger logger)
        {
            _config = config;
            _logger = logger;
        }

        public override void InstallBindings()
        {
            Container.BindInstance(_config).AsSingle();
            Container.BindLoggerAsSiraLogger(_logger);
        }
    }
}