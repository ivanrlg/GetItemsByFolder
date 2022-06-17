using Microsoft.Graph;
using System.IO;
using System.Threading.Tasks;

namespace GetItemsByFolder.Helper
{
    public class FileDownloader
    {
        public static async Task<byte[]> DownloadFile(GraphServiceClient _graphServiceClient, string UserID, string FileId)
        {
            IDriveItemContentRequest request = _graphServiceClient.Users[UserID].Drive.Items[FileId].Content.Request();
            Stream stream = await request.GetAsync();
            return ReadFully(stream);
        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}
