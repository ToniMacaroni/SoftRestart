using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace SoftRestart
{
    public class GameManager : IInitializable
    {
        private Bookmark _bookmark;
        private PauseMenuManager _pauseMenuManager;

        [Inject]
        public void Construct(
            Bookmark bookmark,
            PauseMenuManager pauseMenuManager)
        {
            _bookmark = bookmark;
            _pauseMenuManager = pauseMenuManager;
        }

        public void Initialize()
        {
            CreateUi();
        }

        private void CreateUi()
        {
            var canvas = _pauseMenuManager.GetField<LevelBar, PauseMenuManager>("_levelBar")
                .transform
                .parent
                .parent
                .GetComponent<Canvas>();
            if (!canvas) return;


        }
    }
}