using IPA;
using IPA.Config;
using IPA.Config.Stores;
using SiraUtil.Zenject;
using SoftRestart.Installers;
using IPALogger = IPA.Logging.Logger;

namespace SoftRestart
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {

        [Init]
        public Plugin(IPALogger logger, Config conf, Zenjector zenjector)
        {
            var config = conf.Generated<PluginConfig>();
            zenjector.OnApp<PluginAppInstaller>().WithParameters(config);
            zenjector.OnGame<PluginGameInstaller>();
        }

        [OnEnable]
        public void OnEnable()
        {
        }

        [OnDisable]
        public void OnDisable()
        {

        }

    }
}
