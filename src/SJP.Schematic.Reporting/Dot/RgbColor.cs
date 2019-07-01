﻿using SJP.Schematic.Core.Extensions;
using System;
using System.ComponentModel;

namespace SJP.Schematic.Reporting.Dot
{
    public sealed class RgbColor : IEquatable<RgbColor>
    {
        public RgbColor(string hex)
        {
            if (hex.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(hex));

            // validate hex string
            var hexOnly = hex.StartsWith("#") ? hex.Substring(1) : hex;
            var r = Convert.ToByte(hexOnly.Substring(0, 2), 16);
            var g = Convert.ToByte(hexOnly.Substring(2, 2), 16);
            var b = Convert.ToByte(hexOnly.Substring(4, 2), 16);

            _hex = ToRgbHex(r, g, b);
            _hashCode = _hex.GetHashCode();
        }

        private static string ToRgbHex(byte red, byte green, byte blue)
        {
            var r = red.ToString("X2");
            var g = green.ToString("X2");
            var b = blue.ToString("X2");

            return string.Concat("#", r, g, b);
        }

        public override string ToString() => _hex;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => _hashCode;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as RgbColor);
        }

        public bool Equals(RgbColor other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return _hex == other.ToString();
        }

        private readonly string _hex;
        private readonly int _hashCode;
    }
}
