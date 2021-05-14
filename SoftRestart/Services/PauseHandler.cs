using IPA.Utilities;

namespace SoftRestart.Services
{
    internal class PauseHandler
    {
        public PauseMenuManager PauseMenuManager => _pauseMenuManager;

        private readonly PauseMenuManager _pauseMenuManager;
        private readonly PauseController _pauseController;
        private readonly BeatmapObjectHandler _beatmapObjectHandler;
        private readonly PauseAnimationController _pauseAnimationController;

        public PauseHandler(
            PauseMenuManager pauseMenuManager,
            PauseController pauseController,
            BeatmapObjectHandler beatmapObjectHandler)
        {
            _pauseMenuManager = pauseMenuManager;
            _pauseController = pauseController;
            _beatmapObjectHandler = beatmapObjectHandler;

            _pauseAnimationController = PauseAnimationControllerAcc(ref pauseMenuManager);
        }

        public void Unpause(bool instant = false)
        {
            if (!instant)
            {
                _pauseController.HandlePauseMenuManagerDidPressContinueButton();
                return;
            }

            _pauseMenuManager.enabled = false;
            _pauseAnimationController.ResumeFromPauseAnimationDidFinish();
            _pauseMenuManager.transform.Find("Wrapper/MenuWrapper")?.gameObject.SetActive(false);
            _pauseMenuManager.transform.Find("MenuControllers")?.gameObject.SetActive(false);
            _beatmapObjectHandler.Resume();
        }

        #region Accessors

        private static readonly FieldAccessor<PauseMenuManager, PauseAnimationController>.Accessor PauseAnimationControllerAcc =
            FieldAccessor<PauseMenuManager, PauseAnimationController>.GetAccessor("_pauseAnimationController");

        #endregion
    }
}