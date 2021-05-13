using System;
using System.Collections.Generic;
using HMUI;
using IPA.Utilities;
using Polyglot;
using SiraUtil.Services;
using SiraUtil.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Zenject;
using Exception = System.Exception;
using Object = UnityEngine.Object;

namespace SoftRestart
{
    public class GameManager : IInitializable
    {
        private readonly Bookmark _bookmark;
        private readonly PauseMenuManager _pauseMenuManager;
        private readonly AudioTimeSyncController _audioTimeSyncController;
        private readonly PauseController _pauseController;
        private BeatmapObjectCallbackController _beatmapObjectCallbackController;
        private BasicBeatmapObjectManager _beatmapObjectManager;
        private readonly PluginConfig _config;
        private readonly Submission _submission;
        private GameEnergyCounter _gameEnergyCounter;
        private ScoreController _scoreController;
        private readonly SiraLog _log;
        private readonly ScoreUIController _scoreUiController;

        private NoTransitionsButton _bookmarkButton;
        private readonly bool _enabled = true;

        #region Accessors

        private static readonly FieldAccessor<BeatmapObjectCallbackController, IReadonlyBeatmapData>.Accessor BeatmapDataAcc =
            FieldAccessor<BeatmapObjectCallbackController, IReadonlyBeatmapData>.GetAccessor("_beatmapData");

        private static readonly FieldAccessor<BeatmapObjectCallbackController, float>.Accessor SpawningStartTimeAcc =
            FieldAccessor<BeatmapObjectCallbackController, float>.GetAccessor("_spawningStartTime");

        private static readonly FieldAccessor<BeatmapObjectCallbackController, int>.Accessor NextEventIndexAcc =
            FieldAccessor<BeatmapObjectCallbackController, int>.GetAccessor("_nextEventIndex");

        private static readonly FieldAccessor<BeatmapObjectCallbackController, List<BeatmapObjectCallbackData>>.Accessor BeatmapObjectCallbackDataAcc =
            FieldAccessor<BeatmapObjectCallbackController, List<BeatmapObjectCallbackData>>.GetAccessor("_beatmapObjectCallbackData");

        private static readonly FieldAccessor<BasicBeatmapObjectManager, MemoryPoolContainer<GameNoteController>>.Accessor GameNotePoolContainerAcc =
            FieldAccessor<BasicBeatmapObjectManager, MemoryPoolContainer<GameNoteController>>.GetAccessor("_gameNotePoolContainer");

        private static readonly FieldAccessor<BasicBeatmapObjectManager, MemoryPoolContainer<BombNoteController>>.Accessor BombNotePoolContainerAcc =
            FieldAccessor<BasicBeatmapObjectManager, MemoryPoolContainer<BombNoteController>>.GetAccessor("_bombNotePoolContainer");

        private static readonly FieldAccessor<BasicBeatmapObjectManager, MemoryPoolContainer<ObstacleController>>.Accessor ObstaclePoolContainerAcc =
            FieldAccessor<BasicBeatmapObjectManager, MemoryPoolContainer<ObstacleController>>.GetAccessor("_obstaclePoolContainer");


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

        private static readonly FieldAccessor<StandardLevelRestartController, StandardLevelScenesTransitionSetupDataSO>.Accessor SceneSetupDataAcc =
            FieldAccessor<StandardLevelRestartController, StandardLevelScenesTransitionSetupDataSO>.GetAccessor("_standardLevelSceneSetupData");

        #endregion

        public GameManager(
            Bookmark bookmark,
            PauseMenuManager pauseMenuManager,
            AudioTimeSyncController audioTimeSyncController,
            PauseController pauseController,
            BeatmapObjectCallbackController beatmapObjectCallbackController,
            BasicBeatmapObjectManager beatmapObjectManager,
            PluginConfig config,
            Submission submission,
            GameEnergyCounter gameEnergyCounter,
            ScoreController scoreController,
            SiraLog log, StandardLevelRestartController restartController)
        {
            if (SceneSetupDataAcc(ref restartController).gameMode == "Replay")
            {
                _enabled = false;
                return;
            }

            _bookmark = bookmark;
            _pauseMenuManager = pauseMenuManager;
            _audioTimeSyncController = audioTimeSyncController;
            _pauseController = pauseController;
            _beatmapObjectCallbackController = beatmapObjectCallbackController;
            _beatmapObjectManager = beatmapObjectManager;
            _config = config;
            _submission = submission;
            _gameEnergyCounter = gameEnergyCounter;
            _scoreController = scoreController;
            _log = log;
            _scoreUiController = Object.FindObjectOfType<ScoreUIController>();
        }

        public void Initialize()
        {
            if (!_enabled)
            {
                return;
            }

            try
            {
                CreateUi();
            }
            catch (Exception)
            {
                _log.Error("Couldn't create Soft Restart UI");
            }
        }

        private void CreateUi()
        {
            var buttonsTemplate = _pauseMenuManager.GetField<LevelBar, PauseMenuManager>("_levelBar")
                .transform
                .parent
                .Find("Buttons").gameObject;
            if (!buttonsTemplate) return;

            var buttons = Object.Instantiate(buttonsTemplate, buttonsTemplate.transform.parent);
            buttons.name = "SRButtons";
            buttons.GetComponent<RectTransform>().anchoredPosition = new Vector2(-2.5f, -29);

            var t = buttons.transform;
            var menuButton = t.GetChild(0).gameObject;
            var restartButton = t.GetChild(1).gameObject;
            var continueButton = t.GetChild(2).gameObject;

            _bookmarkButton = restartButton.GetComponent<NoTransitionsButton>();
            _bookmarkButton.interactable = false;

            SetupButton(menuButton, "Soft Restart", OnRestartClick);
            SetupButton(restartButton, "Bookmark Restart", OnRestartBookmarkClick);
            SetupButton(continueButton, "Set Bookmark", OnBookmarkClick);
        }

        private void SetupButton(GameObject go, string name, Action onClick)
        {
            var btn = go.GetComponent<NoTransitionsButton>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(new UnityAction(onClick));
            Object.Destroy(go.GetComponentInChildren<LocalizedTextMeshProUGUI>());
            go.GetComponentInChildren<TextMeshProUGUI>().text = name;
        }

        private void OnRestartClick()
        {
            _submission.DisableScoreSubmission("Soft Restart", "'Soft Restart' used");
            SeekTo(0, false);
            ResetEnergy();

            _pauseController.HandlePauseMenuManagerDidPressContinueButton();

            DeactivateActiveObjects();

            // unregister events since they are being registered in ScoreController.Start()
            _beatmapObjectManager.noteWasCutEvent -= _scoreController.HandleNoteWasCut;
            _beatmapObjectManager.noteWasMissedEvent -= _scoreController.HandleNoteWasMissed;

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
            _scoreUiController.UpdateScore(0, 0);
        }

        private void OnRestartBookmarkClick()
        {
            _submission.DisableScoreSubmission("Soft Restart", "'Bookmark Restart' used");
            SeekTo(_bookmark.Time, true);
            _pauseController.HandlePauseMenuManagerDidPressContinueButton();

            DeactivateActiveObjects();
        }

        private void OnBookmarkClick()
        {
            _bookmark.Set(_audioTimeSyncController.songTime);
            _bookmarkButton.interactable = true;
        }

        /// <summary>
        /// Set <see cref="AudioTimeSyncController"/> to specified time
        /// and adjust <see cref="BeatmapObjectCallbackController"/> and <see cref="BeatmapObjectManager"/> accordingly
        /// </summary>
        /// <param name="t">the time to jump to</param>
        /// <param name="withOffset">
        /// prevent beatmap objects from spawing for certain amount of time
        /// specified in <see cref="PluginConfig"/>
        /// </param>
        private void SeekTo(float t, bool withOffset)
        {
            var offset = withOffset ? _config.SpawnOffset : 0f;
            _audioTimeSyncController.SeekTo(t);

            InitBeatmap();

            NextEventIndexAcc(ref _beatmapObjectCallbackController) = 0;
            SpawningStartTimeAcc(ref _beatmapObjectCallbackController) = t + offset;
        }

        private void SetEnergy(float energy)
        {
            var diff = energy - _gameEnergyCounter.energy;
            _gameEnergyCounter.ProcessEnergyChange(diff);
        }

        private void ResetEnergy()
        {
            _beatmapObjectManager.noteWasCutEvent -= _gameEnergyCounter.HandleNoteWasCut;
            _beatmapObjectManager.noteWasMissedEvent -= _gameEnergyCounter.HandleNoteWasMissed;
            _gameEnergyCounter.Start();
            DidReach0EnergyAcc(ref _gameEnergyCounter) = false;
        }

        /// <summary>
        /// Rebase beatmap
        /// otherwise beatmapobjects only start spawing
        /// after the time we unpaused
        /// </summary>
        private void InitBeatmap()
        {
            var beatmapData = BeatmapDataAcc(ref _beatmapObjectCallbackController);
            var callbackDataList = BeatmapObjectCallbackDataAcc(ref _beatmapObjectCallbackController);

            foreach (var beatmapObjectCallbackData in callbackDataList)
            {
                if (beatmapObjectCallbackData.nextObjectIndexInLine.Length < beatmapData.beatmapLinesData.Count)
                {
                    beatmapObjectCallbackData.nextObjectIndexInLine = new int[beatmapData.beatmapLinesData.Count];
                }
                for (int i = 0; i < beatmapObjectCallbackData.nextObjectIndexInLine.Length; i++)
                {
                    beatmapObjectCallbackData.nextObjectIndexInLine[i] = 0;
                }
            }
        }

        /// <summary>
        /// Deactivate active pooled objects so they don't just chill in the back after unpausing
        /// </summary>
        private void DeactivateActiveObjects()
        {
            var notePool = GameNotePoolContainerAcc(ref _beatmapObjectManager);

            var bombPool = BombNotePoolContainerAcc(ref _beatmapObjectManager);

            var obstaclePool = ObstaclePoolContainerAcc(ref _beatmapObjectManager);

            foreach (var note in notePool.activeItems)
            {
                note.gameObject.SetActive(false);
            }

            foreach (var bomb in bombPool.activeItems)
            {
                bomb.gameObject.SetActive(false);
            }

            foreach (var obstacle in obstaclePool.activeItems)
            {
                obstacle.gameObject.SetActive(false);
            }
        }
    }
}