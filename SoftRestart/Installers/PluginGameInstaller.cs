using UnityEngine;
using Zenject;

namespace SoftRestart.Installers
{
    public class PluginGameInstaller : Installer
    {
        public override void InstallBindings()
        {
            if (Container.TryResolve<MultiplayerController>() != null)
            {
                return;
            }

            Container.Bind<Bookmark>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameManager>().AsSingle();
        }
    }
}