using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

namespace Spice86.Aeon.Emulator.Video.Rendering
{
    /// <summary>
    /// Renders 4-bit graphics to a bitmap.
    /// </summary>
    public sealed class GraphicsPresenter4 : Presenter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsPresenter4"/> class.
        /// </summary>
        /// <param name="dest">Pointer to destination bitmap.</param>
        /// <param name="videoMode">VideoMode instance describing the video mode.</param>
        public GraphicsPresenter4(VideoMode videoMode) : base(videoMode)
        {
        }

        /// <summary>
        /// Updates the bitmap to match the current state of the video RAM.
        /// </summary>
        protected override void DrawFrame(IntPtr destination)
        {
            int width = VideoMode.Width;
            int height = Math.Min(VideoMode.Height, VideoMode.LineCompare + 1);
            var palette = VideoMode.Palette;
            int stride = VideoMode.Stride;
            int horizontalPan = VideoMode.HorizontalPanning;
            int startOffset = VideoMode.StartOffset;

            int safeWidth = Math.Min(stride, width / 8);
            int bitPan = horizontalPan % 8;

            unsafe
            {
                uint* srcPtr = (uint*)VideoMode.VideoRam.ToPointer();
                uint* destPtr = (uint*)destination.ToPointer();

                fixed (byte* paletteMap = VideoMode.InternalPalette)
                {
                    int destStart = 0;

                    for (int split = 0; split < 2; split++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            int srcPos = (stride * y + startOffset + horizontalPan / 8) & 0xFFFF;
                            int destPos = width * y + destStart;

                            for (int i = bitPan; i < 8; i++)
                                destPtr[destPos++] = EgaToArgb(paletteMap[UnpackIndex(srcPtr[srcPos], 7 - i)]);

                            srcPos++;

                            for (int xb = 1; xb < safeWidth; xb++)
                            {
                                uint source = srcPtr[srcPos++ & 0xFFFF];
                                for (int i = 7; i >= 0; i--) {
                                    int index = UnpackIndex(source, i);
                                    byte ega = paletteMap[index];
                                    destPtr[destPos++] = EgaToArgb(ega);
                                }
                            }

                            srcPos &= 0xFFFF;

                            for (int i = 0; i < bitPan; i++)
                                destPtr[destPos++] = EgaToArgb(paletteMap[UnpackIndex(srcPtr[srcPos], 7 - i)]);
                        }

                        if (height < VideoMode.Height)
                        {
                            startOffset = 0;
                            height = VideoMode.Height - VideoMode.LineCompare - 1;
                            destStart = VideoMode.LineCompare * width;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint EgaToArgb(byte ega) {
            int red = 0b1010101 * (ega >> 1 & 2 | ega >> 5 & 1);
            int green = 0b1010101 * (ega & 2 | ega >> 4 & 1);
            int blue = 0b1010101 * (ega << 1 & 2 | ega >> 3 & 1);
            
            uint argb = (uint)(red << 16 | green << 8 | blue);
           
            return argb;
        }

        // it's important for this to get inlined
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int UnpackIndex(uint value, int index) {
            if (Bmi2.IsSupported)
                return (int)Bmi2.ParallelBitExtract(value, 0x01010101u << index);
            return (int)(((value & (1u << index)) >> index) | ((value & (0x100u << index)) >> (7 + index)) | ((value & (0x10000u << index)) >> (14 + index)) | ((value & (0x1000000u << index)) >> (21 + index)));
        }
    }
}
