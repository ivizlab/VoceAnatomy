using System;

namespace Assets.Scripts.Voice
{
    /// <summary>
    /// Plain bag of delegates that <see cref="VoiceCommandMatcher"/>'s rule table invokes via
    /// <see cref="VoiceCommandMatcher.Execute"/>. Built once by <see cref="VoiceCommands"/>
    /// (which has the scene/singleton access these delegates close over) and handed to the
    /// matcher's constructor -- this is what keeps VoiceCommandMatcher itself free of any
    /// dependency on scene state, so it can be constructed and matched against in isolation
    /// (e.g. in an edit-mode test with every field here left null/no-op).
    /// </summary>
    public class VoiceCommandActions
    {
        public Action RemoveSlice;
        public Action Clear;
        public Action ResetView;
        public Action ResetAll;

        public Action ShowMuscles, HideMuscles;
        public Action ShowSkeleton, HideSkeleton;
        public Action ShowNerves, HideNerves;
        public Action ShowOrgans, HideOrgans;
        public Action ShowBloodVessels, HideBloodVessels;
        public Action ShowLymph, HideLymph;
        public Action ShowJoints, HideJoints;

        public Action ShowHeart, ShowBrain, ShowLungs, ShowLiver, ShowStomach;

        public Action FocusOnLastShown;
        public Action ResetOrientation;
        public Action PanUp, PanDown;

        public Action<int> ZoomInByPercent, ZoomOutByPercent;
        public Action ZoomOut;

        public Action<int> RotateByDegrees;
        public Action SlowRotate, Rotate;

        public Action<int> SliceToPercent, SliceDeeperByPercent;
        public Action SliceBack, SlowSlice, SliceOn;

        public Action GestureOff, GestureLeap, GestureVideo;
    }
}
