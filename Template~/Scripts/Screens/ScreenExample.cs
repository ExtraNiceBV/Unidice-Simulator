using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using Unidice.SDK.Interfaces;
using Unidice.SDK.Unidice;
using UnityEngine;

namespace Template.Screens
{
    public class ScreenExample : ScreenBase
    {
        // An example player class, so we can set a text and an icon, and store an image sequence
        [Serializable]
        public class Player
        {
            public string name;
            public Sprite icon;
            internal ImageSequence sequence;
        }

        [SerializeField] private Player[] _players;
        [SerializeField] private TextMeshProUGUI _instructions;
        private IUnidice _unidice;
        public List<Texture2D> debugTextures = new List<Texture2D>();

        public override IEnumerable<ImageSequence> GetSequences()
        {
            // Generate 3 player icons
            foreach (var player in _players)
            {
                // Generate an image sequence from the icon and name. Image sequences are used to show graphics on the unidice.
                if(!player.sequence) player.sequence = GameManager.Instance.GeneratePlayerSequence(player.icon, player.name);
                yield return player.sequence;
            }
        }

        public override void Open()
        {
            base.Open();

            // Get a reference to the Unidice. This is an interface that links to the simulated or real die.
            _unidice = GameManager.Instance.Unidice;

            // Run some example code. We call ""Forget" on it, since we don't want to await the result in this case.
            ExampleCommandsSequence(cancellationToken.Token).Forget();
        }

        // UniTask is like a coroutine, but can return a value. The cancellationToken is used to abort it and has to be passed to every await function.
        private async UniTask ExampleCommandsSequence(CancellationToken cancellationToken)
        {
            _instructions.SetText("Loading...");

            // Clear the Unidice
            _unidice.Sides.Clear();

            // Wait until the game manager is done loading
            await GameManager.Instance.SyncSequences(cancellationToken);

            // Show the player icons on the die
            for (int i = 0; i < _players.Length; i++)
            {
                var player = _players[i];
                // SideLocal refers the sides of the die, relative to itself. So the rotation of the die is ignored.
                // "Each" lets us access the sides of the die by index (0-5).
                // Sides gives us access to the sides of the die. SetSide sets the side of the die to an image sequence.
                _unidice.Sides.SetSide(SideLocal.Each[i], player.sequence);
                // Enable tapping for the side and wait for the tap to happen
                _unidice.Sides.EnableTap(SideLocal.Each[i], _ => OnPlayerTapped(player));
            }

            Player selectedPlayer = null;

            // We call this inline function when one of the players is tapped.
            void OnPlayerTapped(Player player)
            {
                selectedPlayer = player;
                _instructions.SetText($"Player {player.name} selected.");
            }

            _instructions.SetText("Tap a player.");

            // Wait until a player has been selected
            await UniTask.WaitUntil(() => selectedPlayer != null, cancellationToken: cancellationToken);

            // Disable tapping again for all sides
            _unidice.Sides.DisableTap(SideLocal.All);


            // Wait 0.5 seconds
            await UniTask.Delay(TimeSpan.FromSeconds(0.5), cancellationToken: cancellationToken);

            // Draw the number 1 on top of the die (relative to the "world" / table).
            _unidice.Sides.SetSide(SideWorld.Top, GameManager.Instance.GetNumber(1));

            _instructions.SetText("Tap any side.");

            // Wait until any side of the die gets tapped.
            await _unidice.Sides.WaitForTapSequence(SideWorld.All, OnSideTapped, cancellationToken);

            // The inline function that gets called from the tap.
            void OnSideTapped(ISide local)
            {
                // Draw the number 2 on the tapped side of the die.
                _unidice.Sides.SetSide(local, GameManager.Instance.GetNumber(2));
                _instructions.SetText($"You've tapped the {local} side. Now turn the die.");
            }

            // Move the simulator die to the secret box
            _unidice.MoveToSecret(true);

            // Wait for die turned: OnRotated is an event. With GetAsyncEventHandler we can make it "awaitable". OnInvokeAsync is the function we then await. 
            // The code will not continue until OnRotated triggers.
            // Rotator gives us access to the die rotations.
            await _unidice.Rotator.OnRotated.GetAsyncEventHandler(cancellationToken).OnInvokeAsync();

            // Move the simulator die back to the center
            _unidice.MoveToSecret(false);

            _instructions.SetText("Roll the die.");

            // Run this loop forever
            while (true)
            {
                // Wait until the die starts rolling
                await _unidice.Rotator.OnStartedRolling.GetAsyncEventHandler(cancellationToken).OnInvokeAsync();
                // Create a temporary token
                var endOfRollToken = new CancellationTokenSource();
                // Run a task with this token
                DoStuffUntilRollEnds(endOfRollToken.Token).Forget();
                // Wait until the roll is done
                await _unidice.Rotator.OnRolled.GetAsyncEventHandler(cancellationToken).OnInvokeAsync();
                // Cancel the task we started
                endOfRollToken.Cancel();

                // Wait for a moment
                await UniTask.Delay(TimeSpan.FromSeconds(1.5), cancellationToken: cancellationToken);

                _instructions.SetText($"Roll again.");
            }
        }

        private async UniTask DoStuffUntilRollEnds(CancellationToken cancellationToken)
        {
            try
            {
                while (true)
                {
                    _instructions.SetText("Rolling...");
                    // Wait for next frame
                    await UniTask.Yield(cancellationToken);
                }
            }
            // When a cancellation token is used, it throws an OperationCanceledException.
            finally
            {
                // Get the sequence on top of the die
                var sequence = _unidice.Sides.GetSideSequence(SideWorld.Top);
                var text = sequence ? sequence.name : "nothing";
                _instructions.SetText($"You've rolled {text}.");
            }
        }
    }
}