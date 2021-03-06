using System;
using System.Diagnostics;
using OpenMLTD.MilliSim.Core;
using OpenMLTD.MilliSim.Extension.Components.CoreComponents.Configuration;
using OpenMLTD.MilliSim.Foundation;
using OpenMLTD.MilliSim.Graphics;

namespace OpenMLTD.MilliSim.Extension.Components.CoreComponents {
    [MilliSimPlugin(typeof(IComponentFactory))]
    public sealed class DebugOverlayFactory : ComponentFactory {

        public override string PluginID => "plugin.component_factory.debug_overlay";

        public override string PluginName => "DebugOverlay Component Factory";

        public override string PluginDescription => "DebugOverlay Component Factory";

        public override string PluginAuthor => "OpenMLTD";

        public override Version PluginVersion => MyVersion;

        public override IComponent CreateComponent(GameBase game, IComponentContainer parent) {
            var config = game.ConfigurationStore.Get<DebugOverlayConfig>();
            if (config.Data.Visible) {
                Trace.Assert(parent is IVisualContainer);
                var debug = new DebugOverlay((IVisualContainer)parent);
                debug.FillColor = config.Data.TextFill;
                debug.FontSize = config.Data.FontSize;
                return debug;
            } else {
                return null;
            }
        }

        private static readonly Version MyVersion = new Version(1, 0, 0, 0);

    }
}
