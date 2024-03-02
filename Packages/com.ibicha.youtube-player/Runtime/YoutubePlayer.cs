using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;
namespace YoutubePlayer
{
    /// <summary>
    /// Downloads and plays Youtube videos a VideoPlayer component
    /// </summary>
    [RequireComponent(typeof(VideoPlayer))]
    public class YoutubePlayer : MonoBehaviour
    {
        public Text debugText;

        public RawImage thumbnailImage;
        public void StarLoadImg()
        {
            StartCoroutine(loadimg());
        }

        IEnumerator loadimg()
        {
            string thumbnailUrl = GetThumbnailUrl(youtubeUrl);
            if (!string.IsNullOrEmpty(thumbnailUrl))
            {
                yield return LoadThumbnail(thumbnailUrl);
            }
        }

        string GetThumbnailUrl(string videoUrl)
        {
            // Construa a URL da miniatura a partir da URL do vídeo
            // Exemplo: https://img.youtube.com/vi/VIDEO_ID/0.jpg
            // Você pode alterar o número para obter miniaturas de diferentes tamanhos.
            return "https://img.youtube.com/vi/" + GetYouTubeVideoId(videoUrl) + "/0.jpg";
        }

        string GetYouTubeVideoId(string videoUrl)
        {
            // Lógica para extrair o ID do vídeo do URL do YouTube
            // Implemente ou use uma biblioteca para fazer isso.
            // Exemplo simples para fins educacionais:
            int startIndex = videoUrl.IndexOf("v=") + 2;
            int length = videoUrl.Length - startIndex;
            return videoUrl.Substring(startIndex, length);
        }

        IEnumerator LoadThumbnail(string thumbnailUrl)
        {
            // Modifique aqui para adicionar um cabeçalho User-Agent à solicitação HTTP
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(thumbnailUrl))
            {
                www.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                    thumbnailImage.texture = texture;
                }
                else
                {
                    Debug.LogError("Error loading thumbnail: " + www.error);
                }
            }
        }

        public enum Cli
        {
            YoutubeDl,
            YtDlp,
        }

        // The needed fields to play a video
        static readonly string[] k_PlayFields = { "url" };

        // The needed fields to download a video
        static readonly string[] k_DownloadFields = { "title", "_filename" , "ext", "url" };

        /// <summary>
        /// Youtube url (e.g. https://www.youtube.com/watch?v=VIDEO_ID)
        /// </summary>
        public string youtubeUrl;

        /// <summary>
        /// Specify whether to use 360 configuration
        /// </summary>
        public bool is360Video;

        /// <summary>
        /// The cli tool to be used (youtube-dl | yt-dlp)
        /// </summary>
        public Cli cli = Cli.YtDlp;

        /// <summary>
        /// VideoPlayer component associated with the current YoutubePlayer instance
        /// </summary>
        public VideoPlayer VideoPlayer { get; private set; }

        void Awake()
        {
            VideoPlayer = GetComponent<VideoPlayer>();
        }

        async void OnEnable()
        {
            if (VideoPlayer.playOnAwake)
            {
                try
                {
                    await PlayVideoAsync();
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error playing video: " + ex.Message);
                    debugText.text = ex.Message;
                }
            }
        }

        /// <summary>
        /// Triggers a request to youtube-dl to parse the webpage for the raw video url
        /// </summary>
        /// <param name="videoUrl">Youtube url (e.g. https://www.youtube.com/watch?v=VIDEO_ID)</param>
        /// <param name="options">Options for downloading the raw video</param>
        /// <param name="cancellationToken">A CancellationToken used to cancel the current async task</param>
        /// <returns>A Task to await</returns>
        public static async Task<string> GetRawVideoUrlAsync(string videoUrl, YoutubeDlOptions options = null, CancellationToken cancellationToken = default)
        {
            options = options ?? YoutubeDlOptions.Default;
            var metaData = await YoutubeDl.GetVideoMetaDataAsync<YoutubeVideoMetaData>(videoUrl, options, k_PlayFields, cancellationToken);
            return metaData.Url;
        }

        /// <summary>
        /// Triggers a request to youtube-dl to parse the webpage for the raw video url
        /// </summary>
        /// <param name="videoUrl">Youtube url (e.g. https://www.youtube.com/watch?v=VIDEO_ID)</param>
        /// <param name="cli">The cli tool that will be used to download the video or parse its raw url</param>
        /// <param name="options">Options for downloading the raw video</param>
        /// <param name="cancellationToken">A CancellationToken used to cancel the current async task</param>
        /// <returns>A Task to await</returns>
        public static async Task<string> GetRawVideoUrlAsync(string videoUrl, YoutubeDlCli cli, YoutubeDlOptions options = null, CancellationToken cancellationToken = default)
        {
            options = options ?? YoutubeDlOptions.Default;
            var metaData = await YoutubeDl.GetVideoMetaDataAsync<YoutubeVideoMetaData>(videoUrl, options, k_PlayFields, cli, cancellationToken);
            return metaData.Url;
        }

        /// <summary>
        /// Prepare the video for playing. This includes a web request to youtube-dl, as well as preparing/warming up
        /// the VideoPlayer.
        /// </summary>
        /// <param name="videoUrl">Youtube url (e.g. https://www.youtube.com/watch?v=VIDEO_ID)</param>
        /// <param name="options">Options for downloading the raw video</param>
        /// <param name="cancellationToken">A CancellationToken used to cancel the current async task</param>
        /// <returns>A Task to await</returns>
        public async Task PrepareVideoAsync(string videoUrl = null, YoutubeDlOptions options = null, CancellationToken cancellationToken = default)
        {
            videoUrl = videoUrl ?? youtubeUrl;
            options = options ?? (is360Video ? YoutubeDlOptions.Three60 : YoutubeDlOptions.Default);
            var downloader = GetCli(cli);
            var rawUrl = await GetRawVideoUrlAsync(videoUrl, downloader, options, cancellationToken);

            VideoPlayer.source = VideoSource.Url;

            //Resetting the same url restarts the video...
            if (VideoPlayer.url != rawUrl)
                VideoPlayer.url = rawUrl;

            youtubeUrl = videoUrl;
            VideoPlayer.sendFrameReadyEvents = false;

            await VideoPlayer.PrepareAsync(cancellationToken);
        }

        /// <summary>
        /// Play the youtube video in the attached Video Player component.
        /// </summary>
        /// <param name="videoUrl">Youtube url (e.g. https://www.youtube.com/watch?v=VIDEO_ID)</param>
        /// <param name="options">Options for downloading the raw video</param>
        /// <param name="cancellationToken">A CancellationToken used to cancel the current async task</param>
        /// <returns>A Task to await</returns>
        public async Task PlayVideoAsync(string videoUrl = null, YoutubeDlOptions options = null, CancellationToken cancellationToken = default)
        {
            options = options ?? (is360Video ? YoutubeDlOptions.Three60 : YoutubeDlOptions.Default);
            await PrepareVideoAsync(videoUrl, options, cancellationToken);
            VideoPlayer.Play();
        }

        /// <summary>
        /// Download a youtube video to a destination folder
        /// </summary>
        /// <param name="destinationFolder">A folder to create the file in</param>
        /// <param name="videoUrl">Youtube url (e.g. https://www.youtube.com/watch?v=VIDEO_ID)</param>
        /// <param name="cancellationToken">A CancellationToken used to cancel the current async task</param>
        /// <returns>Returns the path to the file where the video was downloaded</returns>
        public async Task<string> DownloadVideoAsync(string destinationFolder = null, string videoUrl = null, CancellationToken cancellationToken = default)
        {
            videoUrl = videoUrl ?? youtubeUrl;
            var downloader = GetCli(cli);
            var video = await YoutubeDl.GetVideoMetaDataAsync<YoutubeVideoMetaData>(videoUrl, YoutubeDlOptions.Default,
                k_DownloadFields, downloader, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var fileName = GetVideoFileName(video);

            var filePath = fileName;
            if (!string.IsNullOrEmpty(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
                filePath = Path.Combine(destinationFolder, fileName);
            }

            await YoutubeDownloader.DownloadAsync(video, filePath, cancellationToken);
            return filePath;
        }

        static string GetVideoFileName(YoutubeVideoMetaData video)
        {
            if (!string.IsNullOrWhiteSpace(video.FileName))
            {
                return video.FileName;
            }

            var fileName = $"{video.Title}.{video.Extension}";

            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var invalidChar in invalidChars)
            {
                fileName = fileName.Replace(invalidChar.ToString(), "_");
            }

            return fileName;
        }

        static YoutubeDlCli GetCli(Cli cli)
        {
            switch (cli)
            {
                case Cli.YoutubeDl:
                    return YoutubeDlCli.YoutubeDl;
                case Cli.YtDlp:
                    return YoutubeDlCli.YtDlp;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cli), cli, "value of cli not supported");
            }
        }
    }
}
