using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// アセットバンドル情報
/// </summary>
public class AssetBundleInfo : IBinary
{
    [JsonProperty("assetBundleName")]
    public string assetBundleName { get; set; }

    [JsonProperty("crc")]
    public uint crc { get; set; }

    [JsonProperty("dependencies")]
    public string[] dependencies { get; set; }

    [JsonProperty("fileSize")]
    public long fileSize { get; set; }

    void IBinary.Write(System.IO.BinaryWriter writer)
    {
        writer.Write(this.assetBundleName);
        writer.Write(this.crc);
        writer.Write(this.dependencies.Length);
        for (int i = 0; i < this.dependencies.Length; i++)
        {
            writer.Write(this.dependencies[i]);
        }
        writer.Write(this.fileSize);
    }

    void IBinary.Read(System.IO.BinaryReader reader)
    {
        this.assetBundleName = reader.ReadString();
        this.crc = reader.ReadUInt32();
        this.dependencies = new string[reader.ReadInt32()];
        for (int i = 0; i < this.dependencies.Length; i++)
        {
            this.dependencies[i] = reader.ReadString();
        }
        this.fileSize = reader.ReadInt64();
    }
}
