using System;
using System.Security.Cryptography;

namespace TA.SharpTunnel.Crypto
{
    //TODO Add own AES algorythm because Microsoft -> NSA
    public class AES : SymmetricCipher
    {
        private AesCryptoServiceProvider _AES;
        private ICryptoTransform _Encryptor;
        private ICryptoTransform _Decryptor;

        public AES(byte[] IV, byte[] Key)
        {
            _AES = new AesCryptoServiceProvider();
            _AES.BlockSize = 128;
            _AES.Padding = PaddingMode.None;
            _AES.KeySize = Key.Length;
            _AES.Mode = CipherMode.CBC;

            _AES.IV = IV;
            _AES.Key = Key;

            _Encryptor = _AES.CreateEncryptor();
            _Decryptor = _AES.CreateDecryptor();
        }

        //ref for performance, since no copying is done
        private byte[] Transform(ICryptoTransform Transform, ref byte[] Data)
        {
            if (Data.Length != 16)
                throw new InvalidOperationException("Array length % 16 must be 0");

            byte[] oData = new byte[Data.Length];

            for (int i = 0; i < Data.Length; i += 16)
                Transform.TransformBlock(Data, i, 16, oData, i);

            return oData;
        }

        public override byte[] Encrypt(byte[] Data) { return Transform(_Encryptor, ref Data); }
        public override byte[] Decrypt(byte[] Data) { return Transform(_Decryptor, ref Data); }

        public override void Dispose()
        {
            _Encryptor.Dispose();
            _Decryptor.Dispose();
            _AES.Dispose();
        }
    }
}
