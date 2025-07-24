using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;


namespace Desdinova
{
    public enum ScreenshotControllerRatio
    {
        Full,
        Squared,
        InstagramSquarePost,
        InstagramPortraitPost,
        InstagramLandscapePost,
        InstagramStoriesReels,
        TikTokSquarePost,
        TikTokPortraitPost,
        TikTokHalfPortraitPost,
        TikTokLandscapePost,
        FacebookPost,
        FacebookPageCoverImage,
        FacebookProfileCoverImage,
        FacebookGroupCoverImage,
        FacebookStories,
        LinkedinPost,
        LinkedinCoverBackground,
        XCoverImage,
        XPostImage,
        YoutubeCoverImage
    }

    public enum ScreenshotControllerPosition
    {
        LeftTop,
        LeftCenter,
        LeftBottom,
        CenterTop,
        CenterCenter,
        CenterBottom,
        RightTop,
        RightCenter,
        RightBottom
    }


    public enum ScreenshotControllerWatermarkPosition
    {
        LeftTop,
        LeftCenter,
        LeftBottom,
        CenterTop,
        CenterCenter,
        CenterBottom,
        RightTop,
        RightCenter,
        RightBottom
    }


    public class SocialNetworkScreenshotCreator : MonoBehaviour
    {
        [Header("Main Properties")]
        [Tooltip("Keyboard key to use to take the in-game screenshot.\nDefault: F1")]
        public KeyCode CaptureKey = KeyCode.F1;
        [Tooltip("Folder to store the screenshot files under project Assets/ folder. Will be created if not present.\nDefault: Screenshots/")]
        public string FolderPath = "Screenshots/";
        [Range(1, 4)]
        [Tooltip("Factor by which to increase resolution. Usefull for printing big images.\nDefault: 1")]
        public int ResolutionFactor = 1;
        [Tooltip("Crop position of the screenshot. Not used if TrackCamera & TrackObject are defined.\nDefault CenterCenter")]
        public ScreenshotControllerPosition Position = ScreenshotControllerPosition.CenterCenter;

        [Header("Track Object")]
        [Tooltip("Main camera to use for object's tracking.")]
        public Camera TrackCamera;
        [Tooltip("Game object to track. The screenshot always tries to keep the object in the center of the image, with defined aspect ratio.")]
        public GameObject TrackObject;

        [Header("Hide Properties")]
        [Tooltip("List of GameObjects to disable during screenshot phase (Ex: UI, text, player)")]
        public List<GameObject> HiddenObjects;

        [Header("Watermark Properties")]
        [Tooltip("Sprite image to use like a watermark on the screenshot.The sprite MUST be flagged with Read/write option.")]
        public Sprite Watermark;
        [Tooltip("Watermark alignment on the screenshot.\nDefault: CenterBottom")]
        public ScreenshotControllerWatermarkPosition WatermarkPosition = ScreenshotControllerWatermarkPosition.CenterBottom;
        [Range(1, 100)]
        [Tooltip("Percentage of the watermark size referred to screenshot size.\nDefault: 50")]
        public int WatermarkPercentage = 50;
        [Range(0, 1)]
        [Tooltip("Opacity value of the watermark. You can use .png sprite, transparency will be preserved.\nDefault: 0.5")]
        public float WatermarkOpacity = 0.5f;

        [Header("Ratios Properties")]
        [Tooltip("Optional screenshot custom ratio.\nEs: 1:2")]
        public string CustomRatio = string.Empty;
        [Tooltip("List of screenshot to take based on custom social platform specifications.\nMore ratios, more time to take screenshots.")]
        public List<ScreenshotControllerRatio> Ratios;

        private static SocialNetworkScreenshotCreator _instance;
        public static SocialNetworkScreenshotCreator SharedInstance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<SocialNetworkScreenshotCreator>();
                }
                return _instance;
            }
        }


        private void Update()
        {
            //Screenshot
            if (this.CaptureKey != KeyCode.None)
            {
                if (Input.GetKeyDown(this.CaptureKey))
                {
                    this.TakeScreenShot();
                }
            }
        }

        public void TakeScreenShot()
        {
            StartCoroutine(TakeScreenShot_Enumerator());
        }

        private IEnumerator TakeScreenShot_Enumerator()
        {
            List<bool> disabledStates = new List<bool>(Enumerable.Repeat(false, this.HiddenObjects.Count));
            for (int i = 0; i < this.HiddenObjects.Count; i++)
            {
                if (this.HiddenObjects[i] != null)
                {
                    disabledStates[i] = this.HiddenObjects[i].activeSelf;
                    this.HiddenObjects[i].SetActive(false);
                }
            }

            //https://gamedevbeginner.com/how-to-capture-the-screen-in-unity-3-methods/
            yield return new WaitForEndOfFrame();

            string filename = System.DateTime.Now.ToString().Replace("/", "-").Replace(" ", "_").Replace(":", "-");
            Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture(this.ResolutionFactor);

            foreach (ScreenshotControllerRatio ratio in Ratios)
            {
                string finalFilename = filename + "-" + ratio.ToString() + ".png";
                Texture2D texCropped;
                if (this.TrackCamera != null && this.TrackObject != null)
                {
                    texCropped = this.CropTextureWithAspectRatioAndPosition(tex, this.GetAspectRatio(ratio), this.TrackObject.transform.position, this.TrackCamera);
                }
                else
                {
                    texCropped = this.CropTextureWithAspectRatio(tex, this.GetAspectRatio(ratio), this.Position);
                }                
                this.SaveTextureToFile(texCropped, this.FolderPath, finalFilename);
            }

            if (!string.IsNullOrEmpty(this.CustomRatio))
            {
                string soloFilename = filename + "-CustomRatio.png";
                Texture2D soloTexCropped;
                if (this.TrackCamera != null && this.TrackObject != null)
                {
                    soloTexCropped = this.CropTextureWithAspectRatioAndPosition(tex, this.CustomRatio.Replace(".", ","), this.TrackObject.transform.position, this.TrackCamera);
                }
                else
                {
                    soloTexCropped = this.CropTextureWithAspectRatio(tex, this.CustomRatio.Replace(".", ","), this.Position);
                }
                this.SaveTextureToFile(soloTexCropped, this.FolderPath, soloFilename);
            }

            for (int i = 0; i < this.HiddenObjects.Count; i++)
            {
                if (this.HiddenObjects[i] != null)
                {
                    this.HiddenObjects[i].SetActive(disabledStates[i]);
                }
            }
        }

        private Texture2D CropTextureWithAspectRatio(Texture2D originalTexture, string aspectRatio, ScreenshotControllerPosition cropPosition)
        {
            // Ottieni il rapporto di aspetto (es: "16:9")
            string[] ratios = aspectRatio.Split(':');
            float aspectWidth = float.Parse(ratios[0], CultureInfo.GetCultureInfo("it-IT"));
            float aspectHeight = float.Parse(ratios[1], CultureInfo.GetCultureInfo("it-IT"));

            // Calcola la dimensione del ritaglio in base al rapporto di aspetto
            int originalWidth = originalTexture.width;
            int originalHeight = originalTexture.height;
            int targetWidth, targetHeight;

            // Mantieni la dimensione più grande e ridimensiona l'altra dimensione
            if (originalWidth / (float)originalHeight > aspectWidth / aspectHeight)
            {
                targetHeight = originalHeight;
                targetWidth = Mathf.RoundToInt(targetHeight * (aspectWidth / aspectHeight));
            }
            else
            {
                targetWidth = originalWidth;
                targetHeight = Mathf.RoundToInt(targetWidth * (aspectHeight / aspectWidth));
            }

            // Calcola gli offset per il ritaglio basato sulla posizione
            int startX = 0;
            int startY = 0;

            // Calcola l'offset orizzontale (X)
            switch (cropPosition)
            {
                case ScreenshotControllerPosition.LeftTop:
                case ScreenshotControllerPosition.LeftCenter:
                case ScreenshotControllerPosition.LeftBottom:
                    startX = 0;
                    break;

                case ScreenshotControllerPosition.CenterTop:
                case ScreenshotControllerPosition.CenterCenter:
                case ScreenshotControllerPosition.CenterBottom:
                    startX = (originalWidth - targetWidth) / 2;
                    break;

                case ScreenshotControllerPosition.RightTop:
                case ScreenshotControllerPosition.RightCenter:
                case ScreenshotControllerPosition.RightBottom:
                    startX = originalWidth - targetWidth;
                    break;
            }

            // Calcola l'offset verticale (Y)
            switch (cropPosition)
            {
                case ScreenshotControllerPosition.LeftTop:
                case ScreenshotControllerPosition.CenterTop:
                case ScreenshotControllerPosition.RightTop:
                    startY = originalHeight - targetHeight;
                    break;

                case ScreenshotControllerPosition.LeftCenter:
                case ScreenshotControllerPosition.CenterCenter:
                case ScreenshotControllerPosition.RightCenter:
                    startY = (originalHeight - targetHeight) / 2;
                    break;

                case ScreenshotControllerPosition.LeftBottom:
                case ScreenshotControllerPosition.CenterBottom:
                case ScreenshotControllerPosition.RightBottom:
                    startY = 0;
                    break;
            }

            // Ottieni i pixel dal ritaglio centrale
            UnityEngine.Color[] pixels = originalTexture.GetPixels(startX, startY, targetWidth, targetHeight);

            // Crea una nuova texture con il rapporto di aspetto desiderato
            Texture2D croppedTexture = new Texture2D(targetWidth, targetHeight);
            croppedTexture.SetPixels(pixels);
            croppedTexture.Apply();

            return croppedTexture;
        }


        private void SaveTextureToFile(Texture2D texture, string subfolderName, string fileName)
        {
            string mainProjectPath = Application.dataPath;

            //Se inizia con uno slash, lo rimuove, perchè lo mette dopo
            if (subfolderName.StartsWith('/'))
                subfolderName = subfolderName.Substring(1);

            if(subfolderName.EndsWith("/"))
                subfolderName = subfolderName.Substring(0, subfolderName.Length - 1);

            // Crea il percorso della sottocartella
            string subfolderPath = mainProjectPath + "/" + subfolderName;

            // Assicurati che la sottocartella esista, altrimenti la crea
            if (!Directory.Exists(subfolderPath))
            {
                Directory.CreateDirectory(subfolderPath);
            }

            //Texture con watermark positionata
            Texture2D finalTexture = texture;
            if (this.Watermark != null)
            {
                finalTexture = this.ApplyWatermarkWithPercentage(texture, this.Watermark.texture, this.WatermarkPercentage, this.WatermarkOpacity, this.WatermarkPosition);
            }

            // Converti la texture in un array di byte (formato PNG)
            byte[] bytes = finalTexture.EncodeToPNG();
            string fullPath = subfolderPath + "/" + fileName;

            // Scrivi i byte su file
            System.IO.File.WriteAllBytes(fullPath, bytes);
        }


        Texture2D ResizeTexture(Texture2D originalTexture, int newWidth, int newHeight)
        {
            Texture2D resizedTexture = new Texture2D(newWidth, newHeight);
            Color[] pixels = resizedTexture.GetPixels(0, 0, newWidth, newHeight);

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    // Calcola le coordinate nella texture originale
                    float u = x / (float)newWidth;
                    float v = y / (float)newHeight;
                    pixels[x + y * newWidth] = originalTexture.GetPixelBilinear(u, v);
                }
            }

            resizedTexture.SetPixels(pixels);
            resizedTexture.Apply();
            return resizedTexture;
        }


        Vector2 CalculateWatermarkPosition(Texture2D baseTexture, Texture2D watermarkTexture, ScreenshotControllerWatermarkPosition cropPosition)
        {
            int startX = 0, startY = 0;

            // Calcola l'offset orizzontale (X)
            switch (cropPosition)
            {
                case ScreenshotControllerWatermarkPosition.LeftTop:
                case ScreenshotControllerWatermarkPosition.LeftCenter:
                case ScreenshotControllerWatermarkPosition.LeftBottom:
                    startX = 0;
                    break;

                case ScreenshotControllerWatermarkPosition.CenterTop:
                case ScreenshotControllerWatermarkPosition.CenterCenter:
                case ScreenshotControllerWatermarkPosition.CenterBottom:
                    startX = (baseTexture.width - watermarkTexture.width) / 2;
                    break;

                case ScreenshotControllerWatermarkPosition.RightTop:
                case ScreenshotControllerWatermarkPosition.RightCenter:
                case ScreenshotControllerWatermarkPosition.RightBottom:
                    startX = baseTexture.width - watermarkTexture.width;
                    break;
            }

            // Calcola l'offset verticale (Y)
            switch (cropPosition)
            {
                case ScreenshotControllerWatermarkPosition.LeftTop:
                case ScreenshotControllerWatermarkPosition.CenterTop:
                case ScreenshotControllerWatermarkPosition.RightTop:
                    startY = baseTexture.height - watermarkTexture.height;
                    break;

                case ScreenshotControllerWatermarkPosition.LeftCenter:
                case ScreenshotControllerWatermarkPosition.CenterCenter:
                case ScreenshotControllerWatermarkPosition.RightCenter:
                    startY = (baseTexture.height - watermarkTexture.height) / 2;
                    break;

                case ScreenshotControllerWatermarkPosition.LeftBottom:
                case ScreenshotControllerWatermarkPosition.CenterBottom:
                case ScreenshotControllerWatermarkPosition.RightBottom:
                    startY = 0;
                    break;
            }

            return new Vector2(startX, startY);
        }



        Texture2D ApplyWatermarkWithPercentage(Texture2D baseTexture, Texture2D watermarkTexture, float percentage, float opacity, ScreenshotControllerWatermarkPosition position)
        {
            // Calcola la dimensione massima della filigrana in base alla percentuale
            int maxWatermarkWidth = Mathf.RoundToInt(baseTexture.width * (percentage / 100f));
            int maxWatermarkHeight = Mathf.RoundToInt(baseTexture.height * (percentage / 100f));

            // Mantieni il rapporto d'aspetto della filigrana
            float aspectRatio = (float)watermarkTexture.width / watermarkTexture.height;
            int newWatermarkWidth, newWatermarkHeight;

            if (watermarkTexture.width > watermarkTexture.height)
            {
                newWatermarkWidth = maxWatermarkWidth;
                newWatermarkHeight = Mathf.RoundToInt(maxWatermarkWidth / aspectRatio);
            }
            else
            {
                newWatermarkHeight = maxWatermarkHeight;
                newWatermarkWidth = Mathf.RoundToInt(maxWatermarkHeight * aspectRatio);
            }

            // Ridimensiona la filigrana
            Texture2D resizedWatermark = ResizeTexture(watermarkTexture, newWatermarkWidth, newWatermarkHeight);

            // Calcola la posizione della filigrana (vedi esempio precedente per posizionare)
            Vector2 positionOffset = CalculateWatermarkPosition(baseTexture, resizedWatermark, position);

            // Applica la filigrana ridimensionata sulla texture di base
            return ApplyWatermarkWithOpacity(baseTexture, resizedWatermark, positionOffset, opacity);
        }



        Texture2D ApplyWatermarkWithOpacity(Texture2D baseTexture, Texture2D watermarkTexture, Vector2 position, float opacity)
        {
            // Crea una copia della texture base per la modifica
            Texture2D resultTexture = new Texture2D(baseTexture.width, baseTexture.height, TextureFormat.RGBA32, false);
            resultTexture.SetPixels(baseTexture.GetPixels());

            // Applica la filigrana sopra la texture di base
            for (int x = 0; x < watermarkTexture.width; x++)
            {
                for (int y = 0; y < watermarkTexture.height; y++)
                {
                    // Calcola la posizione nella texture base
                    int resultX = (int)position.x + x;
                    int resultY = (int)position.y + y;

                    // Se siamo fuori dalla texture di base, saltiamo
                    if (resultX < 0 || resultX >= baseTexture.width || resultY < 0 || resultY >= baseTexture.height)
                        continue;

                    // Prendi il colore del pixel dalla filigrana
                    Color watermarkPixel = watermarkTexture.GetPixel(x, y);

                    // Se il pixel della filigrana ha trasparenza, regoliamo l'opacità
                    if (watermarkPixel.a > 0f)
                    {
                        // Moltiplica il valore alfa originale per l'opacità aggiuntiva
                        watermarkPixel.a *= opacity;

                        // Applica la filigrana sulla base usando l'alpha blend
                        Color basePixel = resultTexture.GetPixel(resultX, resultY);

                        // Blend tra la texture base e la filigrana, usando il valore alfa aggiornato
                        Color blendedColor = Color.Lerp(basePixel, watermarkPixel, watermarkPixel.a);
                        resultTexture.SetPixel(resultX, resultY, blendedColor);
                    }
                }
            }

            resultTexture.Apply(); // Applica i cambiamenti
            return resultTexture;
        }


        private Texture2D ApplyWatermark(Texture2D baseTexture, Texture2D watermarkTexture, Vector2 position)
        {
            // Crea una copia della texture base per la modifica
            Texture2D resultTexture = new Texture2D(baseTexture.width, baseTexture.height, TextureFormat.RGBA32, false);
            resultTexture.SetPixels(baseTexture.GetPixels());

            // Applica la filigrana sopra la texture di base
            for (int x = 0; x < watermarkTexture.width; x++)
            {
                for (int y = 0; y < watermarkTexture.height; y++)
                {
                    // Calcola la posizione nella texture base
                    int resultX = (int)position.x + x;
                    int resultY = (int)position.y + y;

                    // Se siamo fuori dalla texture di base, saltiamo
                    if (resultX < 0 || resultX >= baseTexture.width || resultY < 0 || resultY >= baseTexture.height)
                        continue;

                    // Prendi il colore del pixel dalla filigrana
                    Color watermarkPixel = watermarkTexture.GetPixel(x, y);

                    // Se la filigrana è trasparente, mantieni il colore della base
                    if (watermarkPixel.a > 0f)
                    {
                        // Applica la filigrana sulla base usando l'alpha blend
                        Color basePixel = resultTexture.GetPixel(resultX, resultY);
                        Color blendedColor = Color.Lerp(basePixel, watermarkPixel, watermarkPixel.a);
                        resultTexture.SetPixel(resultX, resultY, blendedColor);
                    }
                }
            }

            resultTexture.Apply(); // Applica i cambiamenti
            return resultTexture;
        }


        private string GetAspectRatio(ScreenshotControllerRatio ratio)
        {                       
            //DEFAULT
            if (ratio == ScreenshotControllerRatio.Full)
            {
                int width = Screen.width;
                int height = Screen.height;
                int gcd = this.GCD(width, height);
                int aspectWidth = width / gcd;
                int aspectHeight = height / gcd;
                return aspectWidth.ToString().Replace(".", ",") + ":" + aspectHeight.ToString().Replace(".", ",");
            }

            //SQUARED
            else if (ratio == ScreenshotControllerRatio.Squared)
            {
                return "1:1";
            }

            //INSTAGRAM
            else if (ratio == ScreenshotControllerRatio.InstagramSquarePost)
            {
                return "1:1";
            }
            else if (ratio == ScreenshotControllerRatio.InstagramPortraitPost)
            {
                return "4:5";
            }
            else if (ratio == ScreenshotControllerRatio.InstagramLandscapePost)
            {
                return "1,91:1";
            }
            else if (ratio == ScreenshotControllerRatio.InstagramStoriesReels)
            {
                return "9:16";
            }

            //TIKTOK
            else if (ratio == ScreenshotControllerRatio.TikTokSquarePost)
            {
                return "1:1";
            }
            else if (ratio == ScreenshotControllerRatio.TikTokPortraitPost)
            {
                return "9:16";
            }
            else if (ratio == ScreenshotControllerRatio.TikTokHalfPortraitPost)
            {
                return "4:16";
            }
            else if (ratio == ScreenshotControllerRatio.TikTokLandscapePost)
            {
                return "16:9";
            }

            //LINKEDIN
            else if (ratio == ScreenshotControllerRatio.LinkedinPost)
            {
                return "1,91:1";
            }
            else if (ratio == ScreenshotControllerRatio.LinkedinCoverBackground)
            {
                return "4:1";
            }

            //FACEBOOK
            else if (ratio == ScreenshotControllerRatio.FacebookPost)
            {
                return "1,91:1";
            }
            else if (ratio == ScreenshotControllerRatio.FacebookPageCoverImage)
            {
                return "2,63:1";
            }
            else if (ratio == ScreenshotControllerRatio.FacebookProfileCoverImage)
            {
                return "2,7:1";
            }
            else if (ratio == ScreenshotControllerRatio.FacebookGroupCoverImage)
            {
                return "1,78:1";
            }
            else if (ratio == ScreenshotControllerRatio.FacebookStories)
            {
                return "9:16";
            }

            //X
            else if (ratio == ScreenshotControllerRatio.XCoverImage)
            {
                return "3:1";
            }
            else if (ratio == ScreenshotControllerRatio.XPostImage)
            {
                return "16:9";
            }

            //YOUTUBE
            else if (ratio == ScreenshotControllerRatio.YoutubeCoverImage)
            {
                return "16:9";
            }




            return "1:1";
        }



        private int GCD(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }




        Texture2D CropTextureWithAspectRatioAndPosition(Texture2D originalTexture, string aspectRatio, Vector3 worldPosition, Camera camera)
        {
            // Ottieni il rapporto di aspetto (es: "16:9")
            string[] ratios = aspectRatio.Split(':');
            float aspectWidth = float.Parse(ratios[0]);
            float aspectHeight = float.Parse(ratios[1]);

            // Calcola la dimensione del ritaglio in base al rapporto di aspetto
            int originalWidth = originalTexture.width;
            int originalHeight = originalTexture.height;
            int targetWidth, targetHeight;

            // Mantieni la dimensione più grande e ridimensiona l'altra dimensione
            if (originalWidth / (float)originalHeight > aspectWidth / aspectHeight)
            {
                targetHeight = originalHeight;
                targetWidth = Mathf.RoundToInt(targetHeight * (aspectWidth / aspectHeight));
            }
            else
            {
                targetWidth = originalWidth;
                targetHeight = Mathf.RoundToInt(targetWidth * (aspectHeight / aspectWidth));
            }

            Debug.Log(targetWidth + "x" + targetHeight);

            // Converti la posizione 3D in coordinate 2D sullo schermo
            Vector3 screenPosition = camera.WorldToScreenPoint(worldPosition);
            // Converti in coordinate della texture (inverti l'asse Y perché la Y dello schermo parte dall'alto)
            screenPosition.y = Screen.height - screenPosition.y;

            // Converte la posizione dello schermo in coordinate della texture
            int centerX = Mathf.RoundToInt(screenPosition.x * (float)originalTexture.width / (float)Screen.width);
            int centerY = Mathf.RoundToInt(screenPosition.y * (float)originalTexture.height / (float)Screen.height);

          
            // Calcola i bordi del ritaglio partendo dal centro, ma correggi se necessario
            int startX = centerX - targetWidth / 2;
            int startY = centerY - targetHeight / 2;

            // Adatta i bordi se il ritaglio esce dalla texture originale
            if (startX < 0)
            {
                startX = 0;
            }
            if (startY < 0)
            {
                startY = 0;
            }
            if (startX + targetWidth > originalTexture.width)
            {
                startX = originalTexture.width - targetWidth;
            }
            if (startY + targetHeight > originalTexture.height)
            {
                startY = originalTexture.height - targetHeight;
            }

            // Crea la texture ritagliata
            Color[] pixels = originalTexture.GetPixels(startX, startY, targetWidth, targetHeight);
            Texture2D croppedTexture = new Texture2D(targetWidth, targetHeight);
            croppedTexture.SetPixels(pixels);
            croppedTexture.Apply();

            return croppedTexture;
        }





    }
}