using UnityStudio.Serialization;
using UnityStudio.Serialization.Naming;

namespace OpenMLTD.MilliSim.Extension.Scores.StandardScoreFormats.Mltd.Serialization {
    [MonoBehavior(NamingConventionType = typeof(CamelCaseNamingConvention))]
    internal sealed class ScoreObject {

        [MonoBehaviorProperty(Name = "evts")]
        internal EventNoteData[] NoteEvents { get; set; }

        [MonoBehaviorProperty(Name = "ct")]
        internal EventConductorData[] ConductorEvents { get; set; }

        internal float[] JudgeRange { get; set; }

        internal float[] ScoreSpeed { get; set; }

        [MonoBehaviorProperty(Name = "BGM_offset")]
        internal float BgmOffset { get; set; }

    }
}
