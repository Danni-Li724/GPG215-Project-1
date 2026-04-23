using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

// copied from gpg214 project 1 :)
    public static class StreamingAssets
    {
        public static IEnumerator LoadBytes(string relativePath, Action<byte[]> onDone)
        {
            string fullPath = System.IO.Path.Combine(Application.streamingAssetsPath, relativePath);

            using (UnityWebRequest req = UnityWebRequest.Get(fullPath))
            {
                yield return req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"StreamingAssets LoadBytes failed: {relativePath} | {req.error}");
                    onDone?.Invoke(null);
                    yield break;
                }

                onDone?.Invoke(req.downloadHandler.data);
            }
        }

        public static IEnumerator LoadText(string relativePath, Action<string> onDone)
        {
            yield return LoadBytes(relativePath, bytes =>
            {
                if (bytes == null)
                {
                    onDone?.Invoke(null);
                    return;
                }

                onDone?.Invoke(System.Text.Encoding.UTF8.GetString(bytes));
            });
        }

        public static IEnumerator LoadSprite(string relativePath, float pixelsPerUnit, Action<Sprite> onDone)
        {
            yield return LoadBytes(relativePath, bytes =>
            {
                if (bytes == null)
                {
                    onDone?.Invoke(null);
                    return;
                }

                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!tex.LoadImage(bytes))
                {
                    Debug.LogWarning($"StreamingAssets LoadSprite failed: {relativePath}");
                    onDone?.Invoke(null);
                    return;
                }

                Rect rect = new Rect(0, 0, tex.width, tex.height);
                Vector2 pivot = new Vector2(0.5f, 0.5f);

                Sprite spr = Sprite.Create(tex, rect, pivot, pixelsPerUnit);
                onDone?.Invoke(spr);
            });
        }
    }