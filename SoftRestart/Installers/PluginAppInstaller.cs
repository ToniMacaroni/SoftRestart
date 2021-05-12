using Zenject;

namespace SoftRestart.Installers
{
    public class PluginAppInstaller : Installer
    {
        private readonly PluginConfig _config;

        public PluginAppInstaller(PluginConfig config)
        {
            _config = config;
        }

        public override void InstallBindings()
        {
            Container.BindInstance(_config).AsSingle();
        }
    }
}