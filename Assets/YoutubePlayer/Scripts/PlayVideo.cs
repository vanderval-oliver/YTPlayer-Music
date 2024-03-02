using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace YoutubePlayer
{
    public class PlayVideo : MonoBehaviour
    {
        public VideoPlayer videoPlayer;
        public Sprite pauseImage;
        public Sprite resumeImage;
        public Image Thisimage;
        Button m_Button;
        public YoutubePlayer youtubePlayer; // Referência ao seu script YoutubePlayer
        public InputField urlInputField;

        public rotation disc;

        void Awake()
        {
            m_Button = GetComponent<Button>();
            m_Button.interactable = videoPlayer.isPrepared;
            videoPlayer.prepareCompleted += VideoPlayerOnPrepareCompleted;
        }

        void VideoPlayerOnPrepareCompleted(VideoPlayer source)
        {
            m_Button.interactable = videoPlayer.isPrepared;
            disc.play();
            //  youtubePlayer.StarLoadImg();
        }

        public void PasteFromClipboard()
        {
            urlInputField.text = GUIUtility.systemCopyBuffer;
        }

        public void PlayButtonClicked()
        {
            // Obtém o URL do campo de entrada
            string videoUrl = urlInputField.text;
            string cleanUrl = YoutubeUrlParser.RemoveQueryString(videoUrl);
            // Verifica se o URL não está vazio
            if (!string.IsNullOrEmpty(cleanUrl))
            {
                // Atribui o URL ao script YoutubePlayer
                youtubePlayer.youtubeUrl = cleanUrl;

                // Chama a função para reproduzir o vídeo
                _ = youtubePlayer.PlayVideoAsync();
            }
            else
            {
                Debug.LogError("URL do vídeo está vazio!");
            }
        }

        public void Play()
        {
            if (videoPlayer.isPaused)
            {
                videoPlayer.Play();
                Thisimage.sprite = pauseImage;
                disc.play();
            }
            else
            {
                videoPlayer.Pause();
                Thisimage.sprite = resumeImage;
                disc.stop();
            }
        }

        /*/  void OnDestroy()
          {
              videoPlayer.prepareCompleted -= VideoPlayerOnPrepareCompleted;
          }
      /*/
        public class YoutubeUrlParser
        {
            public static string RemoveQueryString(string youtubeUrl)
            {
                try
                {
                    int index = youtubeUrl.IndexOf('&');
                    if (index != -1)
                    {
                        // Remove o caractere '&' e tudo o que vem depois
                        return youtubeUrl.Substring(0, index);
                    }
                    else
                    {
                        // Se não houver '&', retorne a URL original
                        return youtubeUrl;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Error parsing URL: " + e.Message);
                    return youtubeUrl; // Retorna a URL original se houver um erro
                }
            }
        }
    }
}
