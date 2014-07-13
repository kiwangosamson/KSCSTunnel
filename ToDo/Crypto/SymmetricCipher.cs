public abstract class SymmetricCipher : IDisposable
{
 public abstract byte[] Encrypt(byte[] Data);
 public abstract byte[] Decrypt(byte[] Data);

 public abstract void Dispose();
}
