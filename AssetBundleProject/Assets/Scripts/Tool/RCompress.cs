using UnityEngine;
using System.Collections;
using System.IO;
using System;
using ICSharpCode.SharpZipLib.GZip;

public class RCompress{
    #region
    /// <summary>
    /// 使用LZMA算法压缩文件
    /// </summary>
    /// <param name="inFile"></param>
    /// <param name="outFile"></param>
    public static void CompressFileLZMA(string inFile,string outFile)
    {
        SevenZip.Compression.LZMA.Encoder coder = new SevenZip.Compression.LZMA.Encoder();
        using(FileStream input = new FileStream(inFile, FileMode.Open))
        {
            using(FileStream output = new FileStream(outFile, FileMode.Create))
            {
                // Write the encoder properties
                coder.WriteCoderProperties(output);

                // Write the decompressed file size.
                output.Write(BitConverter.GetBytes(input.Length), 0, 8);

                // Encode the file
                coder.Code(input, output, input.Length, - 1, null);
                output.Flush();
                output.Close();
                input.Close();
            }
        }
    }

    /// <summary>
    /// 使用LZMA算法解压文件
    /// </summary>
    /// <param name="inFile"></param>
    /// <param name="outFile"></param>
    public static void DecompressFileLZMA(string inFile,string outFile)
    {
        SevenZip.Compression.LZMA.Decoder coder = new SevenZip.Compression.LZMA.Decoder();
        using(FileStream input = new FileStream(inFile, FileMode.Open))
        {
            using(FileStream output = new FileStream(outFile, FileMode.Create))
            {
                // Read the decoder properties
                byte[] properties = new byte[5];
                input.Read(properties, 0, 5);

                // Read in the decompress file size
                byte[] fileLenthBytes = new byte[8];
                input.Read(fileLenthBytes, 0, 8);
                long fileLength = BitConverter.ToInt64(fileLenthBytes, 0);

                // Decompress the file.
                coder.SetDecoderProperties(properties);
                coder.Code(input, output, input.Length, fileLength, null);
                output.Flush();
                output.Close();
                input.Close();
            }
        }
    }
    #endregion

    #region GZip
    public static void CompressFileGZip(string inFile,string outFile)
    {
        using(FileStream input = new FileStream(inFile, FileMode.Open))
        {
            using(FileStream output = new FileStream(inFile, FileMode.Create))
            {
                var outStream = new GZipOutputStream(output);

                // Read
                byte[] data = new byte[input.Length];
                input.Read(data, 0, data.Length);

                // Write
                outStream.Write(data, 0, data.Length);

                outStream.Flush();
                outStream.Finish();
                output.Flush();
                output.Close();
                input.Flush();
                input.Close();
            }
        }
    }

    public static void DecompressFileGZip(string inFile, string outFile)
    {
        SevenZip.Compression.LZMA.Decoder coder = new SevenZip.Compression.LZMA.Decoder();
        using (FileStream input = new FileStream(inFile, FileMode.Open))
        {
            using (FileStream output = new FileStream(outFile, FileMode.Create))
            {
                var inputStream = new GZipInputStream(input);

                byte[] data = new byte[input.Length];
                input.Read(data, 0, data.Length);

                output.Write(data, 0, data.Length);

                inputStream.Flush();
                output.Flush();
                output.Close();
                input.Flush();
                input.Close();
            }
        }
    }
    #endregion

}
