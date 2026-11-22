//ModManagerClass.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Text.Json;

namespace IsaacLauncherGUI
{
    public class ModItem
    {
        public string FolderName { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public string WorkShopId { get; set; }
        public string WorkShopLink { get; set; }
        public bool IsEnabled { get; set; } = true;

        public override string ToString()
        {
            return DisplayName;
        }
    }

    class ModManager
    {
        public string GamePath { get; set; }
        public string ModsPath { get; set; }
        public string thumbPath { get; set; }
        public event EventHandler<ThumbnailDownloadedEventArgs> ThumbnailDownloaded;

        public ModManager(string gamePath)
        {
            thumbPath = Path.Combine(AppConfig.GetDataPath(), "thumbs");
            Directory.CreateDirectory(thumbPath);
            GamePath = gamePath;
            ModsPath = Path.Combine(GamePath, "mods");
        }

        public class ThumbnailDownloadedEventArgs : EventArgs
        {
            public string WorkshopId { get; }
            public string ImagePath { get; }
            public ThumbnailDownloadedEventArgs(string workshopId, string imagePath)
            {
                WorkshopId = workshopId;
                ImagePath = imagePath;
            }
        }

        private void OnThumbnailDownloaded(string workshopId, string imagePath)
        {
            ThumbnailDownloaded?.Invoke(this, new ThumbnailDownloadedEventArgs(workshopId, imagePath));
        }

        private string[] GetInstalledMods()
        {
            try
            {
                if (!Directory.Exists(ModsPath)) Directory.CreateDirectory(ModsPath);
                return Directory.GetDirectories(ModsPath).Select(Path.GetFileName).ToArray();
            }
            catch
            {
                MessageBox.Show("Error accessing the mods directory. Please check your game path and permissions.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new string[] { };
            }
        }

        public ModItem[] SortMods()
        {
            NormalizeThumbnailCache();
            foreach (string filePath in Directory.GetFiles(thumbPath, "*.webp"))
            {
                ConvertWebpToPNG(filePath);
            }

            List<ModItem> modItems = new List<ModItem>();
            List<string> missingThumbnailIds = new List<string>();

            try
            {
                foreach (var mod in GetInstalledMods())
                {
                    string modFolderName = Path.Combine(ModsPath, mod);
                    string disabledMod = Path.Combine(modFolderName, "disable.it");
                    string xmlPath = Path.Combine(modFolderName, "metadata.xml");
                    string modDisplayName = mod;
                    string Description = "";
                    string ImagePath = "";
                    string WorkshopId = "";

                    if (File.Exists(xmlPath))
                    {
                        try
                        {
                            var doc = XDocument.Load(xmlPath);
                            var nameElement = doc.Descendants("name").FirstOrDefault();
                            if (nameElement != null && !string.IsNullOrWhiteSpace(nameElement.Value)) modDisplayName = System.Net.WebUtility.HtmlDecode(nameElement.Value).Trim();
                            var descriptionElement = doc.Descendants("description").FirstOrDefault();
                            if (descriptionElement != null && !string.IsNullOrWhiteSpace(descriptionElement.Value))
                            {
                                string rawDesc = descriptionElement.Value;
                                string decodedDesc = System.Net.WebUtility.HtmlDecode(rawDesc);
                                Description = decodedDesc.Replace("\r\n", "\n").Replace("\n", Environment.NewLine).Trim();
                            }
                            var idelement = doc.Descendants("id").FirstOrDefault();
                            if (idelement != null && !string.IsNullOrWhiteSpace(idelement.Value)) WorkshopId = idelement.Value;

                            if (!string.IsNullOrEmpty(WorkshopId))
                            {
                                string localThumbPath = "";
                                foreach (var ext in new[] { ".jpg", ".png", ".gif"})
                                {
                                    string pathTest = Path.Combine(thumbPath, $"{WorkshopId}{ext}");
                                    if (File.Exists(pathTest))
                                    {
                                        localThumbPath = pathTest;
                                        break;
                                    }
                                }
                                if (!string.IsNullOrEmpty(localThumbPath)) ImagePath = localThumbPath;
                                else missingThumbnailIds.Add(WorkshopId);
                            }
                        }
                        catch
                        {
                        }
                    }

                    ModItem modItem = new ModItem
                    {
                        FolderName = mod,
                        DisplayName = modDisplayName,
                        Description = Description,
                        ImagePath = ImagePath,
                        WorkShopId = WorkshopId,
                        WorkShopLink = string.IsNullOrEmpty(WorkshopId) ? "" : $"https://steamcommunity.com/sharedfiles/filedetails/?id={WorkshopId}",
                        IsEnabled = !File.Exists(disabledMod)
                    };
                    modItems.Add(modItem);
                }
                if (missingThumbnailIds.Count > 0) Task.Run(() => DownloadMissingThumbnailsAsync(missingThumbnailIds));
                return modItems.ToArray();
            }
            catch
            {
                MessageBox.Show("Error sorting mods. Please check your game path and permissions.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new ModItem[] { };
            }
        }

        private string DetectImageExtension(byte[] bytes)
        {
            if (bytes.Length >= 12 && Encoding.ASCII.GetString(bytes, 0, 4) == "RIFF" &&Encoding.ASCII.GetString(bytes, 8, 4) == "WEBP") return ".webp";
            if (bytes.Length >= 8 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47 &&bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A) return ".png";
            if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF) return ".jpg";
            if (bytes.Length >= 6 && bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x38 && (bytes[4] == 0x37 || bytes[4] == 0x39) && bytes[5] == 0x61) return ".gif";
            return null;
        }

        // A háttérben futó Steam API letöltő motor
        private async Task DownloadMissingThumbnailsAsync(List<string> workshopIds)
        {
            using (HttpClient client = new HttpClient())
            {
                // A Steam egyszerre több ID-t is tud fogadni, de maradjunk a biztonságos csomagoknál (pl. 50-esével)
                for (int i = 0; i < workshopIds.Count; i += 50)
                {
                    var chunk = workshopIds.Skip(i).Take(50).ToList();
                    var formContent = new Dictionary<string, string>();
                    formContent["itemcount"] = chunk.Count.ToString();

                    for (int j = 0; j < chunk.Count; j++) formContent[$"publishedfileids[{j}]"] = chunk[j];

                    try
                    {
                        var response = await client.PostAsync("https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/", new FormUrlEncodedContent(formContent));
                        if (response.IsSuccessStatusCode)
                        {
                            string jsonResponse = await response.Content.ReadAsStringAsync();
                            using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
                            {
                                JsonElement details = doc.RootElement.GetProperty("response").GetProperty("publishedfiledetails");

                                foreach (JsonElement mod in details.EnumerateArray())
                                {
                                    if (!mod.TryGetProperty("result", out JsonElement result) ||result.GetInt32() != 1) continue;
                                    if (!mod.TryGetProperty("publishedfileid", out JsonElement idElement) || idElement.ValueKind != JsonValueKind.String) continue;
                                    if (!mod.TryGetProperty("preview_url", out JsonElement urlElement) || urlElement.ValueKind != JsonValueKind.String) continue;

                                    string id = idElement.GetString();
                                    string previewUrl = urlElement.GetString();

                                    if (string.IsNullOrWhiteSpace(previewUrl)) continue;

                                    byte[] imageBytes = await client.GetByteArrayAsync(previewUrl);

                                    // Kigyűjtjük a kiterjesztést az URL-ből (alapból .jpg, ha más nincs)
                                    string extension = DetectImageExtension(imageBytes);
                                    if (extension == null) continue;

                                    string savePath = Path.Combine(thumbPath, $"{id}{extension}");
                                    File.WriteAllBytes(savePath, imageBytes);
                                    OnThumbnailDownloaded(id, savePath);
                                }
                            }
                        }        
                        
                    }
                    catch
                    {
                        // Ha hálózati hiba van háttérben, csendben elnyeljük, hogy ne dobáljon fel popup ablakokat a játéktér közepén
                    }
                }
            }
        }

        private void NormalizeThumbnailCache()
        {
            foreach (string filePath in Directory.GetFiles(thumbPath))
            {
                try
                {
                    byte[] imageBytes = File.ReadAllBytes(filePath);
                    string actualExtension = DetectImageExtension(imageBytes);

                    if (actualExtension == null) continue;

                    string currentExtension = Path.GetExtension(filePath);
                    string newPath = Path.ChangeExtension(filePath, actualExtension);

                    if (!string.Equals(currentExtension, actualExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        if (File.Exists(newPath)) continue;
                        File.Move(filePath, newPath);
                    }
                }
                catch
                {
                    // Ha a fájl olvasása vagy írása közben hiba történik, csendben elnyeljük, hogy ne dobjon fel popup ablakokat
                }
            }
        }

        private void ConvertWebpToPNG(string webpPath)
        {
            string pngPath = Path.ChangeExtension(webpPath, ".png");
            if (File.Exists(pngPath)) return;

            try
            {
                using (var stream = File.OpenRead(webpPath))
                {
                    var decoder = System.Windows.Media.Imaging.BitmapDecoder.Create(
                        stream,
                        System.Windows.Media.Imaging.BitmapCreateOptions.PreservePixelFormat,
                        System.Windows.Media.Imaging.BitmapCacheOption.OnLoad
                    );

                    var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                    encoder.Frames.Add(decoder.Frames[0]);

                    

                    using (var output = File.Create(pngPath))
                    {
                        encoder.Save(output);
                    }
                }
            }
            catch
            {
                // Ha a konverzió közben hiba történik, csendben elnyeljük, hogy ne dobjon fel popup ablakokat
            }
        }

        public void ToggleModState(ModItem mod)
        {
            string modFolderPath = Path.Combine(ModsPath, mod.FolderName);
            string disabledModPath = Path.Combine(modFolderPath, "disable.it");
            try
            {
                if (mod.IsEnabled)
                {
                    // Disable the mod
                    File.Create(disabledModPath).Dispose();
                    mod.IsEnabled = false;
                }
                else
                {
                    // Enable the mod
                    if (File.Exists(disabledModPath))
                    {
                        File.Delete(disabledModPath);
                        mod.IsEnabled = true;
                    }
                }
            }
            catch
            {
                MessageBox.Show("Error toggling mod state. Please check your game path and permissions.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ToggleAllMods(ModItem[] mods)
        {
            try
            {
                foreach (var mod in mods)
                {
                    string modFolderPath = Path.Combine(ModsPath, mod.FolderName);
                    string disabledModPath = Path.Combine(modFolderPath, "disable.it");
                    bool modState = mod.IsEnabled;
                    if (modState) {
                        if (File.Exists(disabledModPath)) File.Delete(disabledModPath);
                        else File.Create(disabledModPath).Dispose();
                    }
                }
            }
            catch
            {
                MessageBox.Show("Error toggling all mods. Please check your game path and permissions.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
