namespace AzureEmu
{
    public interface IBlobServiceEngine
    {
        byte[] GetBlob(string containerName, string blobName);
        bool ContainsBlob(string containerName, string blobName);
        void PutBlob(string containerName, string blobName, byte[] content);
        bool ContainsContainer(string containerName);
    }
}