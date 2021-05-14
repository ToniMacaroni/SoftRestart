using System.Collections.Generic;
using IPA.Utilities;

namespace SoftRestart.Services
{
    internal class BeatmapObjectHandler
    {
        public BasicBeatmapObjectManager ObjectManager => _objectManager;

        private BeatmapObjectCallbackController _callbackController;
        private BasicBeatmapObjectManager _objectManager;

        public BeatmapObjectHandler(
            BeatmapObjectCallbackController callbackController,
            BasicBeatmapObjectManager objectManager)
        {
            _callbackController = callbackController;
            _objectManager = objectManager;
        }

        public void SeekTo(float t)
        {
            InitBeatmap();
            NextEventIndexAcc(ref _callbackController) = 0;
            SpawningStartTimeAcc(ref _callbackController) = t;
        }

        public void Resume()
        {
            _objectManager.HideAllBeatmapObjects(false);
            _objectManager.PauseAllBeatmapObjects(false);
        }

        /// <summary>
        /// Deactivate active pooled objects so they don't just chill in the back after unpausing
        /// </summary>
        public void DeactivateActiveObjects()
        {
            var notePool = GameNotePoolContainerAcc(ref _objectManager);

            var bombPool = BombNotePoolContainerAcc(ref _objectManager);

            var obstaclePool = ObstaclePoolContainerAcc(ref _objectManager);

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

        /// <summary>
        /// Rebase beatmap
        /// otherwise beatmapobjects only start spawing
        /// after the time we unpaused
        /// </summary>
        private void InitBeatmap()
        {
            var beatmapData = BeatmapDataAcc(ref _callbackController);
            var callbackDataList = BeatmapObjectCallbackDataAcc(ref _callbackController);

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

        #endregion
    }
}