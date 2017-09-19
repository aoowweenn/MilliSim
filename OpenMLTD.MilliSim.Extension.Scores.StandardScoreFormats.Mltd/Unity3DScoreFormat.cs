using System;
using System.Composition;
using JetBrains.Annotations;
using OpenMLTD.MilliSim.Core.Entities.Extending;

namespace OpenMLTD.MilliSim.Extension.Scores.StandardScoreFormats.Mltd {
    [Export(typeof(IScoreFormat))]
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
    public sealed class Unity3DScoreFormat : ScoreFormat {

        public override string PluginID => "plugin.score.mltd.unity3d";

        public override string PluginName => "MLTD Unit3D Score Format";

        public override string PluginDescription => "MLTD Unity3D score format reader and compiler factory.";

        public override string PluginAuthor => "OpenMLTD";

        public override Version PluginVersion => MyVersion;

        public override bool CanReadAsSource => true;

        public override bool CanReadAsCompiled => true;

        public override IScoreReader CreateReader() {
            return new Unity3DScoreReader();
        }

        public override IScoreCompiler CreateCompiler() {
            return new Unity3DScoreCompiler();
        }

        public override bool SupportsFileType(string fileName) {
            fileName = fileName.ToLowerInvariant();
            return fileName.EndsWith(".unity3d") || fileName.EndsWith(".unity3d.lz4");
        }

        public override string FormatDescription => "MLTD Unity3D Score File";

        private static readonly Version MyVersion = new Version(1, 0, 0, 0);

    }
}