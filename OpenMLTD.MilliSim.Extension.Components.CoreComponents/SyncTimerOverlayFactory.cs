using System;
using System.Diagnostics;
using OpenMLTD.MilliSim.Core;
using OpenMLTD.MilliSim.Extension.Components.CoreComponents.Configuration;
using OpenMLTD.MilliSim.Foundation;
using OpenMLTD.MilliSim.Graphics;

namespace OpenMLTD.MilliSim.Extension.Components.CoreComponents {
    [MilliSimPlugin(typeof(IComponentFactory))]
    public sealed class SyncTimerOverlayFactory : ComponentFactory {

        public override string PluginID => "plugin.component_factory.sync_timer_overlay";

        public override string PluginName => "SyncTimerOverlay Component Factory";

        public override string PluginDescription => "SyncTimerOverlay Component Factory";

        public override string PluginAuthor => "OpenMLTD";

        public override Version PluginVersion => MyVersion;

        public override IComponent CreateComponent(GameBase game, IComponentContainer parent) {
            var config = game.ConfigurationStore.Get<SyncTimerOverlayConfig>();
            if (config.Data.Visible) {
                Trace.Assert(parent is IVisualContainer);
                var syncTimerOverlay = new SyncTimerOverlay((IVisualContainer)parent);
                syncTimerOverlay.FillColor = config.Data.TextFill;
                syncTimerOverlay.FontSize = config.Data.FontSize;
                return syncTimerOverlay;
            } else {
                return null;
            }
        }

        private static readonly Version MyVersion = new Version(1, 0, 0, 0);

    }
}
