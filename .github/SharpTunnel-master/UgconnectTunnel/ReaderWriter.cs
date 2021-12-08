using System;
using System.IO;

namespace TA.UgconnectTunnel
{
    public class ReaderWriter : IDisposable
    {
        public bool DisposeStreams { get; set; }
        public readonly Stream ReadStream;
        public readonly Stream WriteStream;

        private byte[] _SingleByteBuffer = new byte[1];

        private void CheckEndianness()
        {
            if (!BitConverter.IsLittleEndian)
                throw new Exception("BigEndian isn't supported");
        }

        public ReaderWriter(Stream Stream)
        {
            CheckEndianness();
            DisposeStreams = false;
            ReadStream = Stream;
            WriteStream = Stream;
        }

        public ReaderWriter(Stream ReadStream, Stream WriteStream)
        {
            CheckEndianness();
            DisposeStreams = false;
            this.ReadStream = ReadStream;
            this.WriteStream = WriteStream;
        }

        public void Dispose()
        {
            if (DisposeStreams)
            {
                ReadStream.Dispose();
                WriteStream.Dispose();
            }
        }

        public void Flush() { WriteStream.Flush(); }

        public void Write(byte Byte)
        {
            _SingleByteBuffer[0] = Byte;
            WriteStream.Write(_SingleByteBuffer, 0, 1);
        }

        public byte ReadByte()
        {
            int ibyte = ReadStream.ReadByte();
            if (ibyte < 0)
                throw new EndOfStreamException();
            else return (byte)ibyte;
        }

        public void Write(byte[] Bytes, bool LengthPrefix)
        {
            if (LengthPrefix)
                Write((uint)Bytes.Length);
            WriteStream.Write(Bytes, 0, Bytes.Length);
        }

        public byte[] ReadBytes() { return ReadBytes(ReadUInt()); }
        public byte[] ReadBytes(uint Length)
        {
            byte[] buffer = new byte[Length];
            if (Length > 0)
            {
                int total_read = 0;
                while (true)
                {
                    int read = ReadStream.Read(buffer, total_read, buffer.Length - total_read);
                    if (read > 0)
                    {
                        total_read += read;
                        if (total_read == Length)
                            return buffer;
                    }
                    else throw new EndOfStreamException();
                }
            }
            else return buffer;
        }

        public void Write(ushort UShort)
        {
            Write((byte)(UShort >> 8));
            Write((byte)(UShort & 0xFF));
        }

        public ushort ReadUShort()
        {
            byte higherByte = ReadByte();
            return (ushort)(higherByte << 8 | ReadByte());
        }

        public void Write(uint UInt)
        {
            Write((ushort)(UInt >> 16));
            Write((ushort)(UInt & 0xFFFF));
        }

        public uint ReadUInt()
        {
            ushort higherUShort = ReadUShort();
            return (uint)(higherUShort << 16 | ReadUShort());
        }

        public void Write(ulong ULong)
        {
            Write((uint)(ULong >> 32));
            Write((uint)(ULong & 0xFFFFFFFF));
        }

        public ulong ReadULong()
        {
            //higherUInt must be ulong, or "<< 32" will not work
            //maybe because int is used internally?
            ulong higherUInt = ReadUInt();
            return (ulong)(higherUInt << 32 | ReadUInt());
        }

        public void Write(char Char) { unchecked { Write((ushort)Char); } }

        public char ReadChar() { unchecked { return (char)ReadUShort(); } }

        public void Write(string String, bool LengthPrefix)
        {
            if (LengthPrefix)
                Write((ushort)String.Length);
            for (ushort s = 0; s < String.Length; s++)
                Write(String[s]);
        }

        public string ReadString() { return ReadString(ReadUShort()); }
        public string ReadString(ushort Length)
        {
            char[] chars = new char[Length];
            for (ushort s = 0; s < Length; s++)
                chars[s] = ReadChar();
            return new string(chars);
        }

        public void Write(bool Bool) { Write((byte)(Bool ? 1 : 0)); }

        public bool ReadBool() { return ReadByte() > 0; }

        public void Write(int Int) { unchecked { Write((uint)Int); } }

        public int ReadInt() { unchecked { return (int)ReadUInt(); } }

        public void Write(long Long) { unchecked { Write((ulong)Long); } }

        public long ReadLong() { unchecked { return (long)ReadULong(); } }
    }
}
