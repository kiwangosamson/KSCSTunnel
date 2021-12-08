using System;
using System.Security.Cryptography;

namespace TA.UgconnectTunnel.Encryption
{
    //TODO Add own TripleDES algorythm because Microsoft -> NSA
    public class TripleDES : SymmetricCipher
    {
        private TripleDESCryptoServiceProvider _3DES;
        private ICryptoTransform _Encryptor;
        private ICryptoTransform _Decryptor;

        public TripleDES(byte[] IV, byte[] Key)
        {
            _3DES = new TripleDESCryptoServiceProvider();
            _3DES.BlockSize = 64;
            _3DES.Padding = PaddingMode.None;
            _3DES.KeySize = Key.Length;
            _3DES.Mode = CipherMode.CBC;

            _3DES.IV = IV;
            _3DES.Key = Key;

            _Encryptor = _3DES.CreateEncryptor();
            _Decryptor = _3DES.CreateDecryptor();
        }

        //ref for performance, since no copying is done
        private byte[] Transform(ICryptoTransform Transform, ref byte[] Data)
        {
            if (Data.Length != 8)
                throw new InvalidOperationException("Array length % 8 must be 0");

            byte[] oData = new byte[Data.Length];

            for (int i = 0; i < Data.Length; i += 8)
                Transform.TransformBlock(Data, i, 8, oData, i);

            return oData;
        }

        public override byte[] Encrypt(byte[] Data) { return Transform(_Encryptor, ref Data); }
        public override byte[] Decrypt(byte[] Data) { return Transform(_Decryptor, ref Data); }

        public override void Dispose()
        {
            _Encryptor.Dispose();
            _Decryptor.Dispose();
            _3DES.Dispose();
        }
    }
}
