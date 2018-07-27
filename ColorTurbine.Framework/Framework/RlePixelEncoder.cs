using System;
using System.Collections.Generic;

namespace ColorTurbine
{
    public class RlePixelEncoder
    {
        public static byte[] RleEncode(byte itemSize, byte[] buf, out int len, byte[] retbuf)
        {
            byte count = 0;
            int itemIdx = 0;

            retbuf[0] = (byte)itemSize;
            int retIdx = 1;

            for (int i = 0; i < buf.Length; i += itemSize)
            {
                if (buf[i] == buf[itemIdx] &&
                   buf[i + 1] == buf[itemIdx + 1] &&
                   buf[i + 2] == buf[itemIdx + 2] &&
                   (itemSize == 4 && buf[i + 3] == buf[itemIdx + 3]) &&
                   count < 254)
                {
                    count++;
                }
                else
                {
                    retbuf[retIdx++] = (byte)count;
                    Array.Copy(buf, itemIdx, retbuf, retIdx, itemSize);
                    retIdx += itemSize;

                    count = 0;
                    itemIdx = i;
                }
            }

            retbuf[retIdx++] = (byte)(count + 1);
            Array.Copy(buf, itemIdx, retbuf, retIdx, itemSize);
            retIdx += itemSize;

            len = retIdx;
            return retbuf;
        }

        public class SpanInfo
        {
            public int start;
            public int length;
            public override string ToString()
            {
                return $"{start} - {length}";
            }
        }

        public static bool CompareSpan(Span<byte> a, Span<byte> b)
        {
            if (a.Length != b.Length)
            {
                throw new ArgumentException("Spans must be same length");
            }

            for (int i = 0; i < a.Length; i++)
            {
                if(a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static List<SpanInfo> RleEncode(byte itemSize, Span<byte> source, Span<byte> dest, int increment)
        {
            byte count = 0;
            int itemIdx = 0;
            var retlist = new List<SpanInfo>();
            int lastSlice = 0;

            var itemCount = source.Length / itemSize;
            dest[0] = 0; // new frame
            dest[1] = 1; // RLE
            dest[2] = 0; // unused
            dest[3] = 0; // unused
            dest[4] = (byte)((itemCount >> 8) & 0xFF);
            dest[5] = (byte)(itemCount & 0xFF);
            dest[6] = (byte)itemSize;
            int retIdx = 7;

            for (int i = 0; i < source.Length; i += itemSize)
            {
                if (CompareSpan(source.Slice(i, itemSize),
                                source.Slice(itemIdx, itemSize)) &&
                   count <= 254)
                {
                    count++;
                }
                else
                {
                    // End last run
                    dest[retIdx++] = (byte)count;
                    dest[retIdx++] = source[itemIdx];
                    dest[retIdx++] = source[itemIdx + 1];
                    dest[retIdx++] = source[itemIdx + 2];
                    if (itemSize == 4)
                    {
                        dest[retIdx++] = source[itemIdx + 3];
                    }

                    // Start next run
                    count = 1;
                    itemIdx = i;

                    if (retIdx + itemSize + 1 - lastSlice >= increment)
                    {
                        var currentPixel = i / itemSize;
                        retlist.Add(new SpanInfo {start = lastSlice, length = retIdx - lastSlice });
                        lastSlice = retIdx;
                        dest[retIdx++] = 1; // existing frame
                        dest[retIdx++] = 1; // rle
                        dest[retIdx++] = 0; // unused
                        dest[retIdx++] = 0; // unused
                        dest[retIdx++] = (byte)((currentPixel >> 8) & 0xFF); // pixel high
                        dest[retIdx++] = (byte)(currentPixel & 0xFF); // pixel low
                        dest[retIdx++] = (byte)itemSize;
                    }
                }
            }

            // Dump final run
            dest[retIdx++] = (byte)(count);
            dest[retIdx++] = source[itemIdx];
            dest[retIdx++] = source[itemIdx + 1];
            dest[retIdx++] = source[itemIdx + 2];
            if (itemSize == 4)
            {
                dest[retIdx++] = source[itemIdx + 3];
            }

            retlist.Add(new SpanInfo { start = lastSlice, length = retIdx - lastSlice});
            return retlist;
        }
    }
}