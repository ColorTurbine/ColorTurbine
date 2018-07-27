using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Newtonsoft.Json;

namespace ColorTurbine
{
    public class Point
    {
        public int x {get;}
        public int y {get;}

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class RGBWStrip2D : IStrip // Be a 'strip'
    {
        RGBWStrip _strip; // Contain an implemented strip (for low level - only RGBW for now - handling)
        public int width;
        public int height;
        bool mirroredX;

        public RGBWStrip2D(string name, IPAddress address, int width, int height, bool mirroredX = false)
        {
            this._strip = new RGBWStrip(name, address, width * height);

            this.width = width;
            this.height = height;
            this.mirroredX = mirroredX;
        }
        Dictionary<string, int[]> font = new Dictionary<string, int[]>
        {
            [" "] = new[] { 0x00, 0x00, 0x00, 0x00, 0x00 },
            ["!"] = new[] { 0x00, 0x00, 0x5F, 0x00, 0x00 },
            ["\""] = new[] { 0x00, 0x07, 0x00, 0x07, 0x00 },
            ["#"] = new[] { 0x14, 0x7F, 0x14, 0x7F, 0x14 },
            ["$"] = new[] { 0x24, 0x2A, 0x7F, 0x2A, 0x12 },
            ["%"] = new[] { 0x23, 0x13, 0x08, 0x64, 0x62 },
            ["&"] = new[] { 0x36, 0x49, 0x55, 0x22, 0x50 },
            ["'"] = new[] { 0x00, 0x05, 0x03, 0x00, 0x00 },
            ["("] = new[] { 0x00, 0x1C, 0x22, 0x41, 0x00 },
            [")"] = new[] { 0x00, 0x41, 0x22, 0x1C, 0x00 },
            ["*"] = new[] { 0x08, 0x2A, 0x1C, 0x2A, 0x08 },
            ["+"] = new[] { 0x08, 0x08, 0x3E, 0x08, 0x08 },
            [","] = new[] { 0x00, 0x50, 0x30, 0x00, 0x00 },
            ["-"] = new[] { 0x00, 0x00, 0x08, 0x08, 0x08 },
            ["."] = new[] { 0x00, 0x60, 0x60, 0x00, 0x00 },
            ["/"] = new[] { 0x20, 0x10, 0x08, 0x04, 0x02 },
            ["0"] = new[] { 0x3E, 0x51, 0x49, 0x45, 0x3E },
            ["1"] = new[] { 0x00, 0x42, 0x7F, 0x40, 0x00 },
            ["2"] = new[] { 0x42, 0x61, 0x51, 0x49, 0x46 },
            ["3"] = new[] { 0x21, 0x41, 0x45, 0x4B, 0x31 },
            ["4"] = new[] { 0x18, 0x14, 0x12, 0x7F, 0x10 },
            ["5"] = new[] { 0x27, 0x45, 0x45, 0x45, 0x39 },
            ["6"] = new[] { 0x3C, 0x4A, 0x49, 0x49, 0x30 },
            ["7"] = new[] { 0x01, 0x71, 0x09, 0x05, 0x03 },
            ["8"] = new[] { 0x36, 0x49, 0x49, 0x49, 0x36 },
            ["9"] = new[] { 0x06, 0x49, 0x49, 0x29, 0x1E },
            [":"] = new[] { 0x00, 0x36, 0x36, 0x00, 0x00 },
            [";"] = new[] { 0x00, 0x56, 0x36, 0x00, 0x00 },
            ["<"] = new[] { 0x00, 0x08, 0x14, 0x22, 0x41 },
            ["="] = new[] { 0x14, 0x14, 0x14, 0x14, 0x14 },
            [">"] = new[] { 0x41, 0x22, 0x14, 0x08, 0x00 },
            ["?"] = new[] { 0x02, 0x01, 0x51, 0x09, 0x06 },
            ["@"] = new[] { 0x32, 0x49, 0x79, 0x41, 0x3E },
            ["A"] = new[] { 0x7E, 0x11, 0x11, 0x11, 0x7E },
            ["B"] = new[] { 0x7F, 0x49, 0x49, 0x49, 0x36 },
            ["C"] = new[] { 0x3E, 0x41, 0x41, 0x41, 0x22 },
            ["D"] = new[] { 0x7F, 0x41, 0x41, 0x22, 0x1C },
            ["E"] = new[] { 0x7F, 0x49, 0x49, 0x49, 0x41 },
            ["F"] = new[] { 0x7F, 0x09, 0x09, 0x01, 0x01 },
            ["G"] = new[] { 0x3E, 0x41, 0x41, 0x51, 0x32 },
            ["H"] = new[] { 0x7F, 0x08, 0x08, 0x08, 0x7F },
            ["I"] = new[] { 0x00, 0x41, 0x7F, 0x41, 0x00 },
            ["J"] = new[] { 0x20, 0x40, 0x41, 0x3F, 0x01 },
            ["K"] = new[] { 0x7F, 0x08, 0x14, 0x22, 0x41 },
            ["L"] = new[] { 0x7F, 0x40, 0x40, 0x40, 0x40 },
            ["M"] = new[] { 0x7F, 0x02, 0x04, 0x02, 0x7F },
            ["N"] = new[] { 0x7F, 0x04, 0x08, 0x10, 0x7F },
            ["O"] = new[] { 0x3E, 0x41, 0x41, 0x41, 0x3E },
            ["P"] = new[] { 0x7F, 0x09, 0x09, 0x09, 0x06 },
            ["Q"] = new[] { 0x3E, 0x41, 0x51, 0x21, 0x5E },
            ["R"] = new[] { 0x7F, 0x09, 0x19, 0x29, 0x46 },
            ["S"] = new[] { 0x46, 0x49, 0x49, 0x49, 0x31 },
            ["T"] = new[] { 0x01, 0x01, 0x7F, 0x01, 0x01 },
            ["U"] = new[] { 0x3F, 0x40, 0x40, 0x40, 0x3F },
            ["V"] = new[] { 0x1F, 0x20, 0x40, 0x20, 0x1F },
            ["W"] = new[] { 0x7F, 0x20, 0x18, 0x20, 0x7F },
            ["X"] = new[] { 0x63, 0x14, 0x08, 0x14, 0x63 },
            ["Y"] = new[] { 0x03, 0x04, 0x78, 0x04, 0x03 },
            ["Z"] = new[] { 0x61, 0x51, 0x49, 0x45, 0x43 },
            ["["] = new[] { 0x00, 0x00, 0x7F, 0x41, 0x41 },
            ["\\"] = new[] { 0x02, 0x04, 0x08, 0x10, 0x20 },
            ["]"] = new[] { 0x41, 0x41, 0x7F, 0x00, 0x00 },
            ["^"] = new[] { 0x04, 0x02, 0x01, 0x02, 0x04 },
            ["_"] = new[] { 0x40, 0x40, 0x40, 0x40, 0x40 },
            ["`"] = new[] { 0x00, 0x01, 0x02, 0x04, 0x00 },
            ["a"] = new[] { 0x20, 0x54, 0x54, 0x54, 0x78 },
            ["b"] = new[] { 0x7F, 0x48, 0x44, 0x44, 0x38 },
            ["c"] = new[] { 0x38, 0x44, 0x44, 0x44, 0x20 },
            ["d"] = new[] { 0x38, 0x44, 0x44, 0x48, 0x7F },
            ["e"] = new[] { 0x38, 0x54, 0x54, 0x54, 0x18 },
            ["f"] = new[] { 0x08, 0x7E, 0x09, 0x01, 0x02 },
            ["g"] = new[] { 0x08, 0x14, 0x54, 0x54, 0x3C },
            ["h"] = new[] { 0x7F, 0x08, 0x04, 0x04, 0x78 },
            ["i"] = new[] { 0x00, 0x44, 0x7D, 0x40, 0x00 },
            ["j"] = new[] { 0x20, 0x40, 0x44, 0x3D, 0x00 },
            ["k"] = new[] { 0x00, 0x7F, 0x10, 0x28, 0x44 },
            ["l"] = new[] { 0x00, 0x41, 0x7F, 0x40, 0x00 },
            ["m"] = new[] { 0x7C, 0x04, 0x18, 0x04, 0x78 },
            ["n"] = new[] { 0x7C, 0x08, 0x04, 0x04, 0x78 },
            ["o"] = new[] { 0x38, 0x44, 0x44, 0x44, 0x38 },
            ["p"] = new[] { 0x7C, 0x14, 0x14, 0x14, 0x08 },
            ["q"] = new[] { 0x08, 0x14, 0x14, 0x18, 0x7C },
            ["r"] = new[] { 0x7C, 0x08, 0x04, 0x04, 0x08 },
            ["s"] = new[] { 0x48, 0x54, 0x54, 0x54, 0x20 },
            ["t"] = new[] { 0x04, 0x3F, 0x44, 0x40, 0x20 },
            ["u"] = new[] { 0x3C, 0x40, 0x40, 0x20, 0x7C },
            ["v"] = new[] { 0x1C, 0x20, 0x40, 0x20, 0x1C },
            ["w"] = new[] { 0x3C, 0x40, 0x30, 0x40, 0x3C },
            ["x"] = new[] { 0x44, 0x28, 0x10, 0x28, 0x44 },
            ["y"] = new[] { 0x0C, 0x50, 0x50, 0x50, 0x3C },
            ["z"] = new[] { 0x44, 0x64, 0x54, 0x4C, 0x44 },
            ["{"] = new[] { 0x00, 0x08, 0x36, 0x41, 0x00 },
            ["|"] = new[] { 0x00, 0x00, 0x7F, 0x00, 0x00 },
            ["}"] = new[] { 0x00, 0x41, 0x36, 0x08, 0x00 },
            ["->"] = new[] { 0x08, 0x08, 0x2A, 0x1C, 0x08 },
            ["<-"] = new[] { 0x08, 0x1C, 0x2A, 0x08, 0x08 }
        };

        public string Name { get => ((IStrip)_strip).Name; set => ((IStrip)_strip).Name = value; }

        public RGBWColor this[int index] { get => _strip[index]; set => _strip[index] = value; }
        public RGBWColor this[int x, int y] { get => Get(x, y); set => Plot(x, y, value); }

        private void BoundsCheck(int x, int y)
        {
            if (x >= width) {
                throw new IndexOutOfRangeException("x");
            }
            if (y >= height) {
                throw new IndexOutOfRangeException("y");
            }
        }

        private void Plot(int x, int y, RGBWColor c)
        {
            BoundsCheck(x, y);
            if(mirroredX) {
                x = (width - 1) - x;
            }
            if (y % 2 == 0)
            {
                _strip[y * width + x] = c;
            }
            else
            {
                _strip[y * width + (width - 1) - x] = c;
            }
        }
        private RGBWColor Get(int x, int y)
        {
            BoundsCheck(x, y);
            if(mirroredX) {
                x = (width - 1) - x;
            }
            if (y % 2 == 0)
            {
                return _strip[y * width + x];
            }
            else
            {
                return _strip[y * width + (width - 1) - x];
            }
        }

        private void RenderChar(Point location, int pos, string c, RGBWColor clr)
        {
            var glyph = font[c];
            const int fontwidth = 5;
            const int fontheight = 8;

            for (var y = 0; y < fontheight; y++)
            {
                for (var x = 0; x < fontwidth; x++)
                {
                    if ((glyph[fontwidth - 1 - x] >> (fontheight - y - 1) & 1) == 1)
                    {
                        // offset*6: 6 pixels between characters
                        Plot(location.x + x + pos * 6, location.y + y, clr);
                    }
                }
            }
        }

        public void RenderString(Point location, string str, RGBWColor clr)
        {
            int pos = str.Length - 1;
            foreach (char c in str)
            {
                RenderChar(location, pos, c.ToString(), clr);
                pos--;
            }
        }

        public void Clear()
        {
            _strip.Clear();
        }

        public void Send(bool force)
        {
            _strip.Send(force);
        }

        public bool dirty { get => _strip.dirty; set => _strip.dirty = value; }
        public int led_count { get => _strip.led_count; }
        public void Fill(int start, int end, RGBWColor c, FillType mode = FillType.Set)
        {
            ((IStrip)_strip).Fill(start, end, c, mode);
        }

        public void Fill(double start, double end, RGBWColor c, FillType mode = FillType.Set)
        {
            ((IStrip)_strip).Fill(start, end, c, mode);
        }

        public void DumpBuffer() => ((IStrip)_strip).DumpBuffer();
    }
}
