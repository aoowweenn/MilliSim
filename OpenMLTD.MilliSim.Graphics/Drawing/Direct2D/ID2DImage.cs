using System;
using SharpDX.Direct2D1;

namespace OpenMLTD.MilliSim.Graphics.Drawing.Direct2D {
    public interface ID2DImage : IDisposable {

        Image NativeImage { get; }

    }
}
