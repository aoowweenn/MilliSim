using System.Drawing;
using OpenMLTD.MilliSim.Graphics.Extensions;
using SharpDX.Direct2D1;

namespace OpenMLTD.MilliSim.Graphics.Drawing.Direct2D.Effects {
    public sealed class D2DShadowEffect : D2DEffect {

        public D2DShadowEffect(Effect effect)
            : base(effect) {
        }

        public D2DShadowEffect(RenderContext context)
            : this(context.RenderTarget.DeviceContext2D) {
        }

        public D2DShadowEffect(DeviceContext context)
            : base(new Effect(context, Effect.Shadow)) {
        }

        public Color Color {
            get {
                var rawColor = NativeEffect.GetColor4Value((int)ShadowProperties.Color);
                return rawColor.ToColor();
            }
            set {
                var rawColor = value.ToRC4();
                NativeEffect.SetValue((int)ShadowProperties.Color, rawColor);
            }
        }

        public float BlurStandardDeviation {
            get => NativeEffect.GetFloatValue((int)ShadowProperties.BlurStandardDeviation);
            set => NativeEffect.SetValue((int)ShadowProperties.BlurStandardDeviation, value);
        }

        public ShadowOptimization Optimization {
            get => NativeEffect.GetEnumValue<ShadowOptimization>((int)ShadowProperties.Optimization);
            set => NativeEffect.SetEnumValue((int)ShadowProperties.Optimization, value);
        }

    }
}
