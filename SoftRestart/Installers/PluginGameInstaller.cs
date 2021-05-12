using Zenject;

namespace SoftRestart.Installers
{
    public class PluginGameInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<Bookmark>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameManager>().AsSingle();
        }
    }
}