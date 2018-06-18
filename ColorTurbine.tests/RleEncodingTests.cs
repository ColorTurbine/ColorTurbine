using System;
using System.Linq;
using ColorTurbine;
using Xunit;

namespace ColorTurbine.tests
{
    public class RleEncodingTests
    {
        [Fact]
        public void Compare_span_should_work()
        {
            byte[] a = new byte[] { 1, 2, 3, 4 };
            byte[] b = new byte[] { 1, 2, 3, 4 };
            byte[] d = new byte[] { 1, 2, 3, 5 };

            Assert.True(RlePixelEncoder.CompareSpan(a, a));
            Assert.True(RlePixelEncoder.CompareSpan(a, b));
            Assert.False(RlePixelEncoder.CompareSpan(a, d));
        }

        [Fact]
        public void RLE_encoding_arrays_should_work()
        {
            byte bytes_per_pixel = 4;
            byte[] buf = new byte[]
            {
                1, 1, 1, 1,
                1, 1, 1, 1,
                0, 0, 0, 0,
            };

            byte[] expected = new byte[]
            {
                bytes_per_pixel, // strip bytes per pixel
                2, // run count
                1, 1, 1, 1, // pixel data
                1, // next run
                0, 0, 0, 0, // next pixel
            };

            var outbuf = new byte[20];
            var ret = RlePixelEncoder.RleEncode(bytes_per_pixel, buf, out int len, outbuf);
            var ret_trimmed = new byte[len];
            Array.Copy(ret, ret_trimmed, len);

            Assert.Equal(expected.Length, len);
            Assert.Equal(bytes_per_pixel, expected[0]);
            Assert.Equal(expected, ret_trimmed);
        }

        [Fact]
        public void RLE_encoding_spans_should_work()
        {
            byte bytes_per_pixel = 4;
            byte[] buf = new byte[]
            {
                1, 1, 1, 1,
                1, 1, 1, 1,
                0, 0, 0, 0,
                0, 0, 0, 0,
            };

            Span<byte> expected_span = new byte[]
            {
                0, // new frame
                1, // RLE
                0, // unused
                0, // unused
                0, // pixel count high
                4, // pixel count low
                bytes_per_pixel, // strip bytes per pixel
                2, // run count
                1, 1, 1, 1, // pixel data
                2, // next run
                0, 0, 0, 0, // next pixel
            };

            Span<byte> raw_frame = buf;

            var compressed_output = new byte[20].AsSpan();
            var max_packet_size = 1000;

            var compressed_segments = RlePixelEncoder.RleEncode((byte)bytes_per_pixel, raw_frame, compressed_output, max_packet_size);

            Assert.Equal(1, compressed_segments.Count);
            var seg = compressed_segments.First();
            var slice = compressed_output.Slice(seg.start, seg.length);
            Assert.True(RlePixelEncoder.CompareSpan(expected_span, slice));
        }

        [Fact]
        public void RLE_encoding_spans_should_work_edge_case()
        {
            byte bytes_per_pixel = 4;
            byte[] buf = new byte[]
            {
                1, 1, 1, 1,
                1, 1, 1, 1,
                0, 0, 0, 0,
            };

            Span<byte> expected_span = new byte[]
            {
                0, // new frame
                1, // RLE
                0, // unused
                0, // unused
                0, // pixel count high
                3, // pixel count low
                bytes_per_pixel, // strip bytes per pixel
                2, // run count
                1, 1, 1, 1, // pixel data
                1, // next run
                0, 0, 0, 0, // next pixel
            };

            Span<byte> raw_frame = buf;

            var compressed_output = new byte[20].AsSpan();
            var max_packet_size = 1000;

            var compressed_segments = RlePixelEncoder.RleEncode((byte)bytes_per_pixel, raw_frame, compressed_output, max_packet_size);

            Assert.Equal(1, compressed_segments.Count);
            var seg = compressed_segments.First();
            var slice = compressed_output.Slice(seg.start, seg.length);
            Assert.True(RlePixelEncoder.CompareSpan(expected_span, slice));
        }

        [Fact]
        public void RLE_encoding_spans_should_work_longer()
        {
            byte bytes_per_pixel = 4;
            byte[] buf = new byte[]
            {
                1, 1, 1, 1,
                1, 1, 1, 1,
                0, 0, 0, 0,
                1, 1, 1, 2,
                1, 1, 1, 2,
                1, 1, 1, 2,
                0, 0, 1, 0,
            };

            Span<byte> expected = new byte[]
            {
                0, // new frame
                1, // RLE
                0, // unused
                0, // unused
                0, // pixel count high
                7, // pixel count low
                bytes_per_pixel, // strip bytes per pixel
                2, // run count
                1, 1, 1, 1, // pixel data
                1, // next run
                0, 0, 0, 0, // next pixel
                3,
                1, 1, 1, 2,
                1,
                0, 0, 1, 0,
            };

            Span<byte> raw_frame = buf;

            var compressed_output = new byte[expected.Length * 2].AsSpan();
            var max_packet_size = 1000;

            var compressed_segments = RlePixelEncoder.RleEncode((byte)bytes_per_pixel, raw_frame, compressed_output, max_packet_size);

            Assert.Equal(1, compressed_segments.Count);

            var seg = compressed_segments.First();
            Assert.Equal(0, seg.start);
            Assert.Equal(expected.Length, seg.length);
            var slice = compressed_output.Slice(seg.start, seg.length);
            Assert.True(RlePixelEncoder.CompareSpan(expected, slice));
        }

        [Fact]
        public void RLE_encoding_spans_should_work_RGB()
        {
            byte bytes_per_pixel = 3;
            byte[] buf = new byte[]
            {
                1, 1, 1,
                1, 1, 1,
                0, 0, 0,
                1, 1, 1,
                1, 1, 1,
                1, 1, 1,
                0, 0, 1,
            };

            Span<byte> expected = new byte[]
            {
                0, // new frame
                1, // RLE
                0, // unused
                0, // unused
                0, // pixel count high
                7, // pixel count low
                bytes_per_pixel, // strip bytes per pixel
                2, // run count
                1, 1, 1, // pixel data
                1, // next run
                0, 0, 0, // next pixel
                3,
                1, 1, 1,
                1,
                0, 0, 1,
            };

            Span<byte> raw_frame = buf;

            var compressed_output = new byte[expected.Length * 2].AsSpan();
            var max_packet_size = 1000;

            var compressed_segments = RlePixelEncoder.RleEncode((byte)bytes_per_pixel, raw_frame, compressed_output, max_packet_size);

            Assert.Equal(1, compressed_segments.Count);

            var seg = compressed_segments.First();
            Assert.Equal(0, seg.start);
            Assert.Equal(expected.Length, seg.length);
            var slice = compressed_output.Slice(seg.start, seg.length);
            Assert.True(RlePixelEncoder.CompareSpan(expected, slice));
        }

        [Fact]
        public void RLE_encoding_spans_should_work_when_split()
        {
            byte bytes_per_pixel = 4;
            byte[] buf = new byte[]
            {
                // Segment 1
                1, 1, 1, 1,
                1, 1, 1, 1,
                0, 0, 0, 0,
                
                // Segment 2
                1, 1, 1, 2,
                1, 1, 1, 2,
                1, 1, 1, 2,
                0, 0, 1, 0,
            };

            Span<byte> expected1 = new byte[]
            {
                0, // new frame
                1, // RLE
                0, // unused
                0, // unused
                0, // pixel count high
                7, // pixel count low
                bytes_per_pixel, // strip bytes per pixel
                2, // run count
                1, 1, 1, 1, // pixel data
                1, // next run
                0, 0, 0, 0, // next pixel
            };

            Span<byte> expected2 = new byte[]
            {
                1, // existing
                1, // RLE
                0, // unused
                0, // unused
                0, // pixel index high
                3, // pixel index low
                bytes_per_pixel, // strip bytes per pixel
                3,
                1, 1, 1, 2,
                1,
                0, 0, 1, 0,
            };

            Span<byte> raw_frame = buf;

            var compressed_output = new byte[(expected1.Length + expected2.Length) * 2].AsSpan();
            var max_packet_size = 17 + 4; // Force the splits shown above

            var compressed_segments = RlePixelEncoder.RleEncode((byte)bytes_per_pixel, raw_frame, compressed_output, max_packet_size);

            // Check segment count
            Assert.Equal(2, compressed_segments.Count);

            var seg1 = compressed_segments[0];
            var seg2 = compressed_segments[1];

            // Check segment list
            Assert.Equal(0, seg1.start);
            Assert.Equal(expected1.Length, seg1.length);

            Assert.Equal(17, seg2.start);
            Assert.Equal(expected2.Length, seg2.length);

            // Check contents
            var slice1 = compressed_output.Slice(seg1.start, seg1.length);
            Assert.True(RlePixelEncoder.CompareSpan(expected1, slice1));

            var slice2 = compressed_output.Slice(seg2.start, seg2.length);
            Assert.True(RlePixelEncoder.CompareSpan(expected2, slice2));
        }
    }
}
