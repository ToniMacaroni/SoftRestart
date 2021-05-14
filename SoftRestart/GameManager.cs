using System;
using System.Collections.Generic;
using HMUI;
using IPA.Utilities;
using Polyglot;
using SiraUtil.Services;
using SiraUtil.Tools;
using SoftRestart.Services;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Zenject;
using Exception = System.Exception;
using Object = UnityEngine.Object;

namespace SoftRestart
{
    internal class GameManager : IInitializable
    {
        private readonly Bookmark _bookmark;
        private readonly BeatmapObjectHandler _beatmapObjectHandler;
        private readonly GameScoringHandler _gameScoringHandler;
        private readonly PauseHandler _pauseHandler;
        private readonly AudioTimeSyncController _audioTimeSyncController;
        private readonly PluginConfig _config;
        private readonly Submission _submission;
        private readonly SiraLog _log;

        private NoTransitionsButton _bookmarkButton;
        private readonly bool _enabled = true;

        public GameManager(
            Bookmark bookmark,
            BeatmapObjectHandler beatmapObjectHandler,
            GameScoringHandler gameScoringHandler,
            PauseHandler pauseHandler,
            AudioTimeSyncController audioTimeSyncController,
            PluginConfig config,
            Submission submission,
            SiraLog log, StandardLevelRestartController restartController)
        {
            if (SceneSetupDataAcc(ref restartController).gameMode == "Replay")
            {
                _enabled = false;
                return;
            }

            _bookmark = bookmark;
            _beatmapObjectHandler = beatmapObjectHandler;
            _gameScoringHandler = gameScoringHandler;
            _pauseHandler = pauseHandler;
            _audioTimeSyncController = audioTimeSyncController;
            _config = config;
            _submission = submission;
            _log = log;
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
            var buttonsTemplate = _pauseHandler.PauseMenuManager.GetField<LevelBar, PauseMenuManager>("_levelBar")
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
            _gameScoringHandler.ResetEnergy();

            _pauseHandler.Unpause(true);

            _beatmapObjectHandler.DeactivateActiveObjects();

            _gameScoringHandler.ResetScore();
        }

        private void OnRestartBookmarkClick()
        {
            _submission.DisableScoreSubmission("Soft Restart", "'Bookmark Restart' used");
            SeekTo(_bookmark.Time, true);
            _pauseHandler.Unpause();
            _beatmapObjectHandler.DeactivateActiveObjects();
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
            _beatmapObjectHandler.SeekTo(t + offset);
        }

        #region Accessors

        private static readonly FieldAccessor<StandardLevelRestartController, StandardLevelScenesTransitionSetupDataSO>.Accessor SceneSetupDataAcc =
            FieldAccessor<StandardLevelRestartController, StandardLevelScenesTransitionSetupDataSO>.GetAccessor("_standardLevelSceneSetupData");

        #endregion
    }
}