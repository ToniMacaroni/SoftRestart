using SoftRestart.Services;
using UnityEngine;
using Zenject;

namespace SoftRestart.Installers
{
    public class PluginGameInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<Bookmark>().AsSingle();
            Container.Bind<BeatmapObjectHandler>().AsSingle();
            Container.Bind<GameScoringHandler>().AsSingle();
            Container.Bind<PauseHandler>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameManager>().AsSingle();
        }
    }
}