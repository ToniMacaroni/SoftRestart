using IPA;
using IPA.Config;
using IPA.Config.Stores;
using SiraUtil.Zenject;
using SoftRestart.Installers;
using IPALogger = IPA.Logging.Logger;

namespace SoftRestart
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {

        [Init]
        public Plugin(IPALogger logger, Config conf, Zenjector zenjector)
        {
            var config = conf.Generated<PluginConfig>();
            zenjector.OnApp<PluginAppInstaller>().WithParameters(config);
            zenjector.OnGame<PluginGameInstaller>();
        }

        [OnStart]
        public void OnApplicationStart()
        {
        }

        [OnExit]
        public void OnApplicationQuit()
        {

        }

    }
}
