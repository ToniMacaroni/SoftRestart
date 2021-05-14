namespace SoftRestart.Services
{
    internal class PauseHandler
    {
        public PauseMenuManager PauseMenuManager => _pauseMenuManager;

        private readonly PauseMenuManager _pauseMenuManager;
        private readonly PauseController _pauseController;

        public PauseHandler(
            PauseMenuManager pauseMenuManager,
            PauseController pauseController)
        {
            _pauseMenuManager = pauseMenuManager;
            _pauseController = pauseController;
        }

        public void Unpause()
        {
            _pauseController.HandlePauseMenuManagerDidPressContinueButton();
        }
    }
}