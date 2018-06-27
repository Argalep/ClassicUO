﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ClassicUO.Network
{
    public sealed class Packet : PacketBase
    {
        private readonly byte[] _data;
        private readonly int _len;

        public Packet(byte[] data, int length)
        {
            _data = data; _len = length; IsDynamic = PacketsTable.GetPacketLength(ID) < 0;
        }


        protected override byte this[int index]
        {
            get
            {
                if (index < 0 || index >= Length)
                    throw new ArgumentOutOfRangeException("index");
                return _data[index];
            }
            set
            {
                if (index < 0 || index >= Length)
                    throw new ArgumentOutOfRangeException("index");
                _data[index] = value; IsChanged = true;
            }
        }

        public override int Length => _len;
        public bool IsChanged { get; private set; }
        public bool Filter { get; set; }

        public override byte[] ToArray() => _data;
        public void MoveToData() => Seek(IsDynamic ? 3 : 1);

        protected override void EnsureSize(int length)
        {
            if (length < 0 || Position + length > Length)
                throw new ArgumentOutOfRangeException("length");
        }

        public byte ReadByte()
        {
            EnsureSize(1);
            return this[Position++];
        }

        public sbyte ReadSByte() => (sbyte)ReadByte();

        public bool ReadBool() => ReadByte() != 0;

        public ushort ReadUShort()
        {
            EnsureSize(2);
            return (ushort)((ReadByte() << 8) | ReadByte());
        }

        public uint ReadUInt()
        {
            EnsureSize(4);
            return (uint)((ReadByte() << 24) | (ReadByte() << 16) | (ReadByte() << 8) | ReadByte());
        }

        public string ReadASCII()
        {
            EnsureSize(1);
            StringBuilder sb = new StringBuilder();
            char c;

            while ((c = (char)ReadByte()) != '\0')
                sb.Append(c);
            return sb.ToString();
        }

        public string ReadASCII(int length)
        {
            EnsureSize(length);
            StringBuilder sb = new StringBuilder(length);
            char c;

            for (int i = 0; i < length; i++)
            {
                c = (char)ReadByte();
                if (c != '\0')
                    sb.Append(c);
            }

            return sb.ToString();
        }

        public string ReadUnicode()
        {
            EnsureSize(2);
            StringBuilder sb = new StringBuilder();
            char c;

            while ((c = (char)ReadUShort()) != '\0')
                sb.Append(c);

            return sb.ToString();
        }

        public string ReadUnicode(int length)
        {
            EnsureSize(length);
            StringBuilder sb = new StringBuilder(length);
            char c;
            for (int i = 0; i < length; i++)
            {
                c = (char)ReadUShort();
                if (c != '\0')
                    sb.Append(c);
            }

            return sb.ToString();
        }

        public string ReadUnicodeReversed(int length)
        {
            EnsureSize(length);
            length /= 2;

            StringBuilder sb = new StringBuilder(length);
            char c;

            for (int i = 0; i < length; i++)
            {
                c = (char)ReadUShortReversed();
                if (c != '\0')
                    sb.Append(c);
            }

            return sb.ToString();
        }

        public ushort ReadUShortReversed() => (ushort)(ReadByte() | ReadByte() << 8);
    }
}