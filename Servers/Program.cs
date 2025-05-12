using ModelContextProtocol.Server;
using System.ComponentModel;
using YoutubeExplode;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<YoutubeSubtitlesExtractor>();
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<YoutubeSubtitlesExtractor>();

var app = builder.Build();
app.MapMcp();
app.Run();


[McpServerToolType]
public sealed class YoutubeSubtitlesExtractor
{
    [McpServerTool, 
     Description("Extracts subtitles (full text) from the given youtube link, e.g. " +
                 "'https://www.youtube.com/watch?v=jNQXAC9IVRw&ab_channel=jawed' or 'https://youtube.com/watch?v=jNQXAC9IVRw', etc.")]
    public static async Task<string> ExtractYoutubeSubtitles(
        IMcpServer mcpServer,
        string youtubeLink)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(youtubeLink, nameof(youtubeLink));

        var youtube = new YoutubeClient();
        var manifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(youtubeLink);

        if (manifest.Tracks.Count == 0)
            return "No closed captions found for this video.";

        var captions = await youtube.Videos.ClosedCaptions.GetAsync(manifest.Tracks.First());
        return captions.Captions
            .Select(c => c.Text)
            .Aggregate((current, next) => current + "\n" + next);
    }
}