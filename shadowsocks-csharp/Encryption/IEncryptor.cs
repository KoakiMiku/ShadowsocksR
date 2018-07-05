﻿using System;

namespace ShadowsocksR.Encryption
{
    public interface IEncryptor : IDisposable
    {
        bool SetIV(byte[] iv);
        void Encrypt(byte[] buf, int length, byte[] outbuf, out int outlength);
        void Decrypt(byte[] buf, int length, byte[] outbuf, out int outlength);
        void ResetEncrypt();
        void ResetDecrypt();
        byte[] getIV();
        byte[] getKey();
        EncryptorInfo getInfo();
    }
}
