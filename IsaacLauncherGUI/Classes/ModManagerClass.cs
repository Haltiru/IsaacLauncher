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

namespace IsaacLauncherGUI
{
    public class ModItem
    {
        public string FolderName { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
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

        public ModManager(string gamePath)
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            thumbPath = Path.Combine(appData, "IsaacLauncherGUI", "thumbs");
            if (!Directory.Exists(thumbPath))
            {
                Directory.CreateDirectory(thumbPath);
            }
            GamePath = gamePath;
            ModsPath = Path.Combine(GamePath, "mods");
        }

        private string[] GetInstalledMods()
        {
            try
            {
                if (!Directory.Exists(ModsPath))
                {
                    Directory.CreateDirectory(ModsPath);
                }
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
                            if (nameElement != null && !string.IsNullOrWhiteSpace(nameElement.Value))
                            {
                                modDisplayName = System.Net.WebUtility.HtmlDecode(nameElement.Value).Trim();
                            }
                            var descriptionElement = doc.Descendants("description").FirstOrDefault();
                            if (descriptionElement != null && !string.IsNullOrWhiteSpace(descriptionElement.Value))
                            {
                                string rawDesc = descriptionElement.Value;
                                string decodedDesc = System.Net.WebUtility.HtmlDecode(rawDesc);
                                Description = decodedDesc.Replace("\r\n", "\n").Replace("\n", Environment.NewLine).Trim();
                            }
                            var idelement = doc.Descendants("id").FirstOrDefault();
                            if (idelement != null && !string.IsNullOrWhiteSpace(idelement.Value))
                            {
                                WorkshopId = idelement.Value;
                            }

                            if (!string.IsNullOrEmpty(WorkshopId))
                            {
                                string localThumbPath = "";
                                foreach (var ext in new[] { ".jpg", ".png", ".gif" })
                                {
                                    string pathTest = Path.Combine(thumbPath, $"{WorkshopId}{ext}");
                                    if (File.Exists(pathTest))
                                    {
                                        localThumbPath = pathTest;
                                        break;
                                    }
                                }

                                if (!string.IsNullOrEmpty(localThumbPath))
                                {
                                    ImagePath = localThumbPath;
                                }
                                else
                                {
                                    missingThumbnailIds.Add(WorkshopId);
                                }
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
                        WorkShopLink = string.IsNullOrEmpty(WorkshopId) ? "" : $"https://steamcommunity.com/sharedfiles/filedetails/?id={WorkshopId}",
                        IsEnabled = !File.Exists(disabledMod)
                    };
                    modItems.Add(modItem);
                }
                if (missingThumbnailIds.Count > 0)
                {
                    Task.Run(() => DownloadMissingThumbnailsAsync(missingThumbnailIds));
                }

                return modItems.ToArray();
            }
            catch
            {
                MessageBox.Show("Error sorting mods. Please check your game path and permissions.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new ModItem[] { };
            }
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

                    for (int j = 0; j < chunk.Count; j++)
                    {
                        formContent[$"publishedfileids[{j}]"] = chunk[j];
                    }

                    try
                    {
                        var response = await client.PostAsync("https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/", new FormUrlEncodedContent(formContent));
                        if (response.IsSuccessStatusCode)
                        {
                            string jsonResponse = await response.Content.ReadAsStringAsync();

                            // Itt kiszedjük a preview_url-eket a JSON-ből regex-szel vagy egyszerű string kereséssel, 
                            // hogy ne kelljen nehéz JSON csomagokat beemelni, vagy simán letöltjük a megadott URL-ről.
                            foreach (var id in chunk)
                            {
                                // Egyszerű keresés a válaszban az adott ID-hoz tartozó URL-re
                                string marker = $"\"publishedfileid\":\"{id}\"";
                                int index = jsonResponse.IndexOf(marker);
                                if (index != -1)
                                {
                                    int urlIndex = jsonResponse.IndexOf("\"preview_url\":\"", index);
                                    if (urlIndex != -1)
                                    {
                                        int start = urlIndex + 15;
                                        int end = jsonResponse.IndexOf("\"", start);
                                        if (end != -1)
                                        {
                                            string previewUrl = jsonResponse.Substring(start, end - start).Replace("\\/", "/");

                                            // Letöltjük magát a képet
                                            byte[] imageBytes = await client.GetByteArrayAsync(previewUrl);

                                            // Kigyűjtjük a kiterjesztést az URL-ből (alapból .jpg, ha más nincs)
                                            string extension = ".jpg";
                                            if (previewUrl.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) extension = ".png";
                                            else if (previewUrl.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)) extension = ".gif";

                                            string savePath = Path.Combine(thumbPath, $"{id}{extension}");
                                            File.WriteAllBytes(savePath, imageBytes);
                                        }
                                    }
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
                    if (modState)
                    {
                        // Enable the mod
                        if (File.Exists(disabledModPath))
                        {
                            File.Delete(disabledModPath);
                        }
                    }
                    else
                    {
                        // Disable the mod
                        File.Create(disabledModPath).Dispose();
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
