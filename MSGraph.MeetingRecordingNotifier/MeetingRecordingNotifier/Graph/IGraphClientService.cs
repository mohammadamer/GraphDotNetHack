
using Microsoft.Graph;

namespace MeetingRecordingNotifier.Graph
{
    public interface IGraphClientService
    {
        public GraphServiceClient? GetAppGraphClient();
    }
}
