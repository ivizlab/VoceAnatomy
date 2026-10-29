using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Scripting
{
    /// <summary>
    /// How a presenter advances a lesson: a click (or a Leap/video pinch, via PointerInput) on the model,
    /// or an advance key -- Right arrow, Page Down or Enter, which is what presentation clickers send.
    ///
    /// While a command waits for a click, click-to-select is paused (<see cref="HoldClicks"/>), so the
    /// advancing click doesn't also select a part. The same pause is what a future quiz command needs.
    /// <see cref="ReleaseAll"/> is called by the runner on stop, since a stopped coroutine never gets to
    /// release its own hold.
    /// </summary>
    public static class PresenterInput
    {
        private static int holds;
        private static bool raycastWasBlocked;

        public static bool AdvanceKeyPressed
        {
            get
            {
                var keyboard = Keyboard.current;
                if (keyboard == null || Shortcuts.UserIsWriting())
                    return false;
                return keyboard.rightArrowKey.wasPressedThisFrame || keyboard.pageDownKey.wasPressedThisFrame
                    || keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame;
            }
        }

        /// <summary>A click/pinch on the model -- not on UI such as the console or the app's panels.</summary>
        public static bool ClickedModel
        {
            get
            {
                if (!PointerInput.WasPressedThisFrame)
                    return false;
                if (PointerInput.IsHandTracking)
                    return !GestureUIInput.IsOverUI;
                return EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject();
            }
        }

        public static void HoldClicks()
        {
            if (holds++ == 0 && RaycastObject.instance != null)
            {
                raycastWasBlocked = RaycastObject.instance.raycastBlocked;
                RaycastObject.instance.raycastBlocked = true;
            }
        }

        public static void ReleaseClicks()
        {
            if (holds == 0)
                return;
            if (--holds == 0 && RaycastObject.instance != null)
                RaycastObject.instance.raycastBlocked = raycastWasBlocked;
        }

        public static void ReleaseAll()
        {
            if (holds > 0)
            {
                holds = 1;
                ReleaseClicks();
            }
        }
    }
}
