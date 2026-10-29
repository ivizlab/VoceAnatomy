using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Scripting
{
    /// <summary>
    /// Runs every command list -- console, voice or macro -- as one coroutine queue: each command's
    /// IEnumerator is waited on before the next starts, so a rotate finishes before a slice begins. A
    /// trailing &amp; starts a command and moves straight on. Errors are logged and that command is
    /// skipped; a failing command can never freeze the app.
    ///
    /// Interrupts: new live input replaces a running script (a macro, or a line with several commands)
    /// and stops its animations. A single command still animating (e.g. a lone "rotate") is left to
    /// finish, so live commands overlap as they always have. "queue ..." waits for whatever is running;
    /// "stop" cancels everything.
    /// </summary>
    public class CommandRunner : MonoBehaviour
    {
        public static CommandRunner Instance { get; private set; }

        /// <summary>Macros can run macros; this stops an accidental loop (a macro running itself).</summary>
        public const int MaxMacroDepth = 8;

        private readonly Queue<List<CommandInvocation>> queued = new Queue<List<CommandInvocation>>();
        private int activeRuns;
        private int activeScripts;

        public bool IsBusy => activeRuns > 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[CommandRunner] A second CommandRunner was found -- disabling this one.");
                enabled = false;
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        // Seconds after startup before checking every macro: part names are only fully known once the
        // app has loaded its translations (synonyms), which happens after the scene starts.
        private const float StartupCheckDelay = 3f;

        /// <summary>Checks every macro shortly after startup, so a broken line shows up in the console
        /// before a lesson rather than in the middle of one.</summary>
        private IEnumerator Start()
        {
            yield return new WaitForSecondsRealtime(StartupCheckDelay);
            if (PartResolver.IsReady)
                MacroCheck.CheckAll(reportClean: false);
        }

        /// <summary>Parses and runs text from a live source (console or voice). Parse errors are logged and
        /// those lines skipped; the rest still runs.</summary>
        public void Submit(string text, CommandSource source)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            string body = text.Trim();
            bool queue = false;
            if (body.StartsWith("queue ", StringComparison.OrdinalIgnoreCase))
            {
                queue = true;
                body = body.Substring("queue ".Length);
            }

            var parsed = CommandParser.Parse(body);
            foreach (var error in parsed.Errors)
                CommandLog.Error(error.ToString());
            if (parsed.Commands.Count == 0)
                return;
            MacroRecorder.Capture(parsed.Commands);

            if (queue && IsBusy)
            {
                queued.Enqueue(parsed.Commands);
                CommandLog.Info("queued: " + string.Join("; ", parsed.Commands));
                return;
            }

            if (activeScripts > 0)
            {
                CommandLog.Info("(interrupted running script)");
                Stop();
            }

            StartCoroutine(RunTopLevel(parsed.Commands, source));
        }

        /// <summary>Cancels every running and queued command and the animations they started.</summary>
        public void Stop()
        {
            StopAllCoroutines();
            queued.Clear();
            activeRuns = 0;
            activeScripts = 0;
            if (CameraController.instance != null)
                CameraController.instance.StopAnimations();
            if (CrossSections.Instance != null)
                CrossSections.Instance.StopAutoSlice();
            // A stopped "wait click" never reaches its own cleanup.
            PresenterInput.ReleaseAll();
            CaptionPanel.HideWaitHintIfShown();
        }

        private IEnumerator RunTopLevel(List<CommandInvocation> commands, CommandSource source)
        {
            bool script = commands.Count > 1 || commands.Any(c => c.Verb == "run");
            activeRuns++;
            if (script)
                activeScripts++;

            yield return RunList(commands, 0, source);
            while (queued.Count > 0)
                yield return RunList(queued.Dequeue(), 0, source);

            activeRuns--;
            if (script)
                activeScripts--;
        }

        /// <summary>Runs a parsed list in order. Also how a macro runs its contents (at depth + 1), so
        /// nested macros share this runner's stop/interrupt.</summary>
        public IEnumerator RunList(List<CommandInvocation> commands, int depth, CommandSource source)
        {
            string indent = new string(' ', depth * 2);
            foreach (var invocation in commands)
            {
                CommandLog.Echo(indent + invocation);
                var ctx = new CommandContext { Runner = this, Invocation = invocation, Depth = depth, Source = source };
                if (invocation.Parallel)
                    StartCoroutine(Guarded(ctx));
                else
                    yield return StartCoroutine(Guarded(ctx));
            }
        }

        // Steps a command's IEnumerator by hand so an exception inside it is caught and logged (a plain
        // try/catch can't wrap a yield), then moves on to the next command.
        private static IEnumerator Guarded(CommandContext ctx)
        {
            IEnumerator steps;
            try
            {
                steps = ctx.Invocation.Command.Execute(ctx);
            }
            catch (Exception e)
            {
                Fail(ctx, e);
                yield break;
            }
            if (steps == null)
                yield break;

            while (true)
            {
                object current;
                try
                {
                    if (!steps.MoveNext())
                        yield break;
                    current = steps.Current;
                }
                catch (Exception e)
                {
                    Fail(ctx, e);
                    yield break;
                }
                yield return current;
            }
        }

        private static void Fail(CommandContext ctx, Exception e)
        {
            CommandLog.Error($"{ctx.Invocation} failed: {e.Message}");
            Debug.LogException(e);
        }
    }
}
