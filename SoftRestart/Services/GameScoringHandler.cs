using IPA.Utilities;
using UnityEngine;

namespace SoftRestart.Services
{
    internal class GameScoringHandler
    {
        private GameEnergyCounter _gameEnergyCounter;
        private ScoreController _scoreController;
        private readonly BeatmapObjectHandler _beatmapObjectHandler;
        private readonly GameplayCoreSceneSetupData _gameplayData;
        private readonly ScoreUIController _scoreUiController;

        public GameScoringHandler(
            GameEnergyCounter gameEnergyCounter,
            ScoreController scoreController,
            BeatmapObjectHandler beatmapObjectHandler,
            GameplayCoreSceneSetupData gameplayData)
        {
            _gameEnergyCounter = gameEnergyCounter;
            _scoreController = scoreController;
            _beatmapObjectHandler = beatmapObjectHandler;
            _gameplayData = gameplayData;
            _scoreUiController = Object.FindObjectOfType<ScoreUIController>();
        }

        public void ResetScore()
        {
            _beatmapObjectHandler.ObjectManager.noteWasCutEvent -= _scoreController.HandleNoteWasCut;
            _beatmapObjectHandler.ObjectManager.noteWasMissedEvent -= _scoreController.HandleNoteWasMissed;

            _scoreController.Start();

            ComboAcc(ref _scoreController) = 0;
            MaxComboAcc(ref _scoreController) = 0;
            CutOrMissedNotesAcc(ref _scoreController) = 0;
            ImmediateMaxPossibleRawScoreAcc(ref _scoreController) = 0;
            PlayerHeadWasInObstacleAcc(ref _scoreController) = false;
            FeverComboAcc(ref _scoreController) = 0;
            FeverStartTimeAcc(ref _scoreController) = 0f;
            FeverIsActiveAcc(ref _scoreController) = false;
            PrevFrameRawScoreAcc(ref _scoreController) = 0;
            BaseRawScoreAcc(ref _scoreController) = 0;
            _scoreController.NotifyForChange(true, true);

            if (!_gameplayData.playerSpecificSettings.noTextsAndHuds)
            {
                _scoreUiController.UpdateScore(0, 0);
            }
        }

        public void SetEnergy(float energy)
        {
            var diff = energy - _gameEnergyCounter.energy;
            _gameEnergyCounter.ProcessEnergyChange(diff);
        }

        public void ResetEnergy()
        {
            _beatmapObjectHandler.ObjectManager.noteWasCutEvent -= _gameEnergyCounter.HandleNoteWasCut;
            _beatmapObjectHandler.ObjectManager.noteWasMissedEvent -= _gameEnergyCounter.HandleNoteWasMissed;
            _gameEnergyCounter.Start();
            DidReach0EnergyAcc(ref _gameEnergyCounter) = false;
        }

        #region Accessors

        private static readonly FieldAccessor<ScoreController, int>.Accessor ComboAcc =
            FieldAccessor<ScoreController, int>.GetAccessor("_combo");

        private static readonly FieldAccessor<ScoreController, int>.Accessor MaxComboAcc =
            FieldAccessor<ScoreController, int>.GetAccessor("_maxCombo");

        private static readonly FieldAccessor<ScoreController, int>.Accessor CutOrMissedNotesAcc =
            FieldAccessor<ScoreController, int>.GetAccessor("_cutOrMissedNotes");

        private static readonly FieldAccessor<ScoreController, int>.Accessor ImmediateMaxPossibleRawScoreAcc =
            FieldAccessor<ScoreController, int>.GetAccessor("_immediateMaxPossibleRawScore");

        private static readonly FieldAccessor<ScoreController, bool>.Accessor PlayerHeadWasInObstacleAcc =
            FieldAccessor<ScoreController, bool>.GetAccessor("_playerHeadWasInObstacle");

        private static readonly FieldAccessor<ScoreController, int>.Accessor FeverComboAcc =
            FieldAccessor<ScoreController, int>.GetAccessor("_feverCombo");

        private static readonly FieldAccessor<ScoreController, float>.Accessor FeverStartTimeAcc =
            FieldAccessor<ScoreController, float>.GetAccessor("_feverStartTime");

        private static readonly FieldAccessor<ScoreController, bool>.Accessor FeverIsActiveAcc =
            FieldAccessor<ScoreController, bool>.GetAccessor("_feverIsActive");

        private static readonly FieldAccessor<ScoreController, int>.Accessor PrevFrameRawScoreAcc =
            FieldAccessor<ScoreController, int>.GetAccessor("_prevFrameRawScore");

        private static readonly FieldAccessor<ScoreController, int>.Accessor BaseRawScoreAcc =
            FieldAccessor<ScoreController, int>.GetAccessor("_baseRawScore");

        private static readonly FieldAccessor<GameEnergyCounter, bool>.Accessor DidReach0EnergyAcc =
            FieldAccessor<GameEnergyCounter, bool>.GetAccessor("_didReach0Energy");

        #endregion
    }
}