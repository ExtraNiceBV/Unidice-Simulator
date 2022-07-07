using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Template.Generators;
using Template.Screens;
using Unidice.SDK.Interfaces;
using Unidice.SDK.Unidice;
using Unidice.SDK.Utilities;
using UnityEngine;

namespace Template
{
    public class GameManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private NumberGenerator numberGenerator;
        [SerializeField] private PlayerImageGenerator playerImageGenerator;
        [SerializeField] private GameObject modalBackground;

        private readonly List<IScreen> _activeScreens = new List<IScreen>();

        public static GameManager Instance { get; private set; }

        public bool AllSequencesLoaded { get; private set; }
        public IUnidice Unidice { get; private set; }
        private ScreenBase ActiveScreen => _activeScreens.OfType<ScreenBase>().LastOrDefault();

        public void Awake()
        {
            Instance = this;
        }

        public async UniTask Start()
        {
            // We (probably) don't need high FPS at this point
            FPSManager.FPS = TargetFPS.Low;

            Unidice = UnidiceSDK.GetUnidice();

            // Make sure all screens are hidden
            foreach (var screen in FindObjectsOfType<ScreenBase>().OrderBy(s => s.transform.GetSiblingIndex()))
            {
                screen.gameObject.SetActive(false);
            }

            // Register to all screens
            foreach (var screen in FindObjectsOfType<ScreenBase>(true))
            {
                screen.OnOpen.AddListener(OnScreenOpened);
                screen.OnClose.AddListener(OnScreenClosed);
            }

            // Register to all modal screens
            foreach (var screen in FindObjectsOfType<ModalScreenBase>(true))
            {
                screen.OnOpen.AddListener(OnScreenOpened);
                screen.OnClose.AddListener(OnScreenClosed);
            }

            // Make sure current scene is open (use a reference for this later)
            FindObjectOfType<ScreenBase>(true).Open();

            await LoadGameData();
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
        }

        public void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) SaveGameData();
        }

        public void OnApplicationQuit()
        {
            SaveGameData();
        }

        /// <summary>
        /// Save whatever needs saving
        /// </summary>
        private void SaveGameData()
        {
        }

        private async UniTask LoadGameData()
        {
            Debug.Log("Waiting for data to load...");
            var dataLoaded = false;
            // Load whatever data the game needs

            // Simulated here with a delay
            Invoker.Invoke(() => dataLoaded = true, 1, DelayType.Realtime);

            await UniTask.WaitUntil(() => dataLoaded, cancellationToken: this.GetCancellationTokenOnDestroy());

            Debug.Log($"...data loaded.");

            OnGameDataLoaded();
        }

        private void OnGameDataLoaded()
        {
            SyncSequences(this.GetCancellationTokenOnDestroy()).ContinueWith(() => OpenFirstScreen(this.GetCancellationTokenOnDestroy())).Forget();
        }

        private static async UniTask OpenFirstScreen(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return;
            FindObjectOfType<ScreenBase>(true).Open();
        }


        public async UniTask SyncSequences(CancellationToken cancellationToken)
        {
            static void ShowProgress(float p)
            {
                // Show loading progress
                Debug.Log($"Loading {p:P0}...");
            }

            // In the simulator loading is very fast. On a real Unidice this can take much longer, so make sure you always await loading before showing images on the die and preload images when possible.
            await LoadSequences(Progress.Create<float>(ShowProgress), GetSequences(), cancellationToken);
        }

        private static void OpenModalScreen()
        {
			// Continue with whatever code you want to run when things are loaded here on in the example screen
            FindObjectOfType<ModalScreenBase>(true).Open();
        }

        /// <summary>
        /// Load all images to Unidice
        /// </summary>
        private async UniTask LoadSequences(IProgress<float> progress, IEnumerable<ImageSequence> sequences, CancellationToken cancellationToken)
        {
            AllSequencesLoaded = false;
            if (Unidice.IsValid)
            {
                progress.Report(0);
                var list = sequences.ToList();
                await Unidice.Images.LoadImagesSequence(list, progress, cancellationToken);
                progress.Report(1);
                AllSequencesLoaded = true;
            }
        }

        /// <summary>
        /// Should return all sequences that need to be loaded to continue.
        /// </summary>
        private IEnumerable<ImageSequence> GetSequences()
        {
            // Generate 10 numbers (they are cached, so just generate them again when you need them)
            foreach (var sequence in numberGenerator.GetSequences(0, 10)) yield return sequence;

            // Get the sequences from all screens (you'll probably want to limit this to the current/next screen)
            foreach (var sequence in FindObjectsOfType<ScreenBase>(true).SelectMany(s => s.GetSequences())) yield return sequence;
            foreach (var sequence in FindObjectsOfType<ModalScreenBase>(true).SelectMany(s => s.GetSequences())) yield return sequence;
        }


        /// <summary>
        /// Function to chain multiple tasks together and combine their reported progress.
        /// </summary>
        private static async Task ChainTasks(IProgress<float> progress, CancellationToken cancellationToken, params Func<IProgress<float>, CancellationToken, UniTask>[] tasks)
        {
            for (var i = 0; i < tasks.Length; i++) await tasks[i](Progress.Create<float>(p => progress.Report((i + p) / tasks.Length)), cancellationToken);

            progress.Report(1);
        }

        /// <summary>
        /// Get the sequence for a specific number, so it can be shown on the die later.
        /// </summary>
        public ImageSequence GetNumber(int number)
        {
            return numberGenerator.GetNumber(number);
        }

        /// <summary>
        /// Called when a screen is opened. Disables any screens that are already open. If it is modal, then modal background is enabled.
        /// </summary>
        public void OnScreenOpened(IScreen screen)
        {
            // Modal screens are treated differently than regular screens; they can be stacked and modalBackground gets toggled
            if (screen is ModalScreenBase)
            {
                modalBackground.gameObject.SetActive(true);

                // Block all other modal screens
                foreach (var modalScreen in _activeScreens.OfType<ModalScreenBase>()) modalScreen.MakeNonInteractable(true);
            }
            else
            {
                foreach (var s in _activeScreens.OfType<ScreenBase>())
                {
                    if (screen.Equals(s)) continue;
                    // Close currently active screen. But don't run the code inside this loop.
                    Invoker.Invoke(() => s.Close(), 0, DelayType.Realtime);
                    s.gameObject.SetActive(false); // So we can't click through the role choice screen
                }
            }

            _activeScreens.Add(screen);
        }

        /// <summary>
        /// Called when a screen is closed and makes sure the modal background is set correctly, and the topmost screen is activated.
        /// </summary>
        public void OnScreenClosed(IScreen screen)
        {
            _activeScreens.Remove(screen);
        
            // Disable modal background when all modal dialogs are closed
            modalBackground.gameObject.SetActive(_activeScreens.OfType<ModalScreenBase>().Any());

            // Enable the top other modal screen
            var topModalScreen = _activeScreens.OfType<ModalScreenBase>().LastOrDefault();
            if (topModalScreen) topModalScreen.MakeNonInteractable(false);

            // Restore top screen
            var topScreen = ActiveScreen;
            if (topScreen) topScreen.gameObject.SetActive(true);
        }

        /// <summary>
        /// Create an image sequence from a sprite and text using an image generator. You can customize the generator(s) any way you like.
        /// </summary>
        public ImageSequence GeneratePlayerSequence(Sprite icon, string labelText)
        {
            // Sequences are scriptable objects and you can also create and store them like that from Unity if they are not dynamic.
            // Sequences are animations, so if there is only one frame, that's the only element in the array. Here we use the image generator to create the texture we want.
            return ImageSequence.Create(labelText, new[] { playerImageGenerator.CreateTexture(icon, labelText) });
        }
    }
}