using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace Quark
{
    public partial class QuarkUtility
    {
        #region MD5
        /// <summary>
        /// 计算字符串的MD5哈希值
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <returns>MD5哈希字符串</returns>
        public static string ComputeMD5(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
                
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                
                return sb.ToString();
            }
        }
        
        /// <summary>
        /// 计算文件的MD5哈希值
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>MD5哈希字符串</returns>
        public static string ComputeFileMD5(string filePath)
        {
            if (!File.Exists(filePath))
                return string.Empty;
                
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream stream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = md5.ComputeHash(stream);
                    
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < hashBytes.Length; i++)
                    {
                        sb.Append(hashBytes[i].ToString("x2"));
                    }
                    
                    return sb.ToString();
                }
            }
        }
        
        /// <summary>
        /// 计算字节数组的MD5哈希值
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <returns>MD5哈希字符串</returns>
        public static string ComputeBytesMD5(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return string.Empty;
                
            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(bytes);
                
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                
                return sb.ToString();
            }
        }
        
        /// <summary>
        /// 计算流的MD5哈希值
        /// </summary>
        /// <param name="stream">输入流</param>
        /// <returns>MD5哈希字符串</returns>
        public static string ComputeStreamMD5(Stream stream)
        {
            if (stream == null || !stream.CanRead)
                return string.Empty;
                
            long position = stream.Position;
            stream.Position = 0;
            
            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(stream);
                
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                
                stream.Position = position;
                return sb.ToString();
            }
        }
        #endregion
    }
}
