using System;
using Unity.Collections;
using Unity.Netcode;

namespace Game.Riddles.FilesRiddle
{
    [Serializable]
    public struct FileInfoEntry : INetworkSerializable, IEquatable<FileInfoEntry>
    {
        public FixedString64Bytes fileName;
        public bool isMalicious;
        public long fileSize;
        public FixedString64Bytes creationDate;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref fileName);
            serializer.SerializeValue(ref isMalicious);
            serializer.SerializeValue(ref fileSize);
            serializer.SerializeValue(ref creationDate);
        }

        public bool Equals(FileInfoEntry other)
        {
            return fileName.Equals(other.fileName) && isMalicious == other.isMalicious && fileSize == other.fileSize && creationDate.Equals(other.creationDate);
        }

        public override bool Equals(object obj)
        {
            return obj is FileInfoEntry other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(fileName, isMalicious, fileSize, creationDate);
        }
    }
}