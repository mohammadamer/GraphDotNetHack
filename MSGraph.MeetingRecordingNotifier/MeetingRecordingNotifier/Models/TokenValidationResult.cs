namespace MeetingRecordingNotifier.Models
{
    public class TokenValidationResult
    {
        // User ID
        public string UserId { get; private set; }

        // The extracted token - used to build user assertion
        // for OBO flow
        public string Token { get; private set; }

        public TokenValidationResult(string userId, string token)
        {
            UserId = userId;
            Token = token;
        }
    }
}
