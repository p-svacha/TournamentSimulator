using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using UnityEngine;

public static class FlagImageScraper
{
    public static void DownloadAllFlagImages()
    {
        string imageUrlBaseSmall = "https://cdn-csd.swisstxt.ch/images/geo/country/flag/medium/";
        string imageUrlBaseBig = "https://cdn-csd.swisstxt.ch/images/geo/country/flag/large/";

        string targetPathSmall = "C:/Workspace/Unity/TournamentSimulator/Assets/Resources/Icons/Flags/48x32/";
        string targetPathBig = "C:/Workspace/Unity/TournamentSimulator/Assets/Resources/Icons/Flags/180x120/";

        foreach (Country c in Database.AllCountries)
        {
            string imageUrlSmall = imageUrlBaseSmall + c.FifaCode + ".png";
            DownloadImage(imageUrlSmall, targetPathSmall);

            string imageUrlBig = imageUrlBaseBig + c.FifaCode + ".png";
            DownloadImage(imageUrlBig, targetPathBig);
        }
    }

    public static void DownloadImage(string imageUrl, string directoryPath)
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                // Send a GET request to the URL
                HttpResponseMessage response = client.GetAsync(imageUrl).Result;

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    // Get the content as a byte array
                    byte[] imageBytes = response.Content.ReadAsByteArrayAsync().Result;

                    // Extract the filename from the URL
                    string fileName = Path.GetFileName(imageUrl);

                    // Construct the full file path
                    string filePath = Path.Combine(directoryPath, fileName);

                    // Write the bytes to a file
                    File.WriteAllBytes(filePath, imageBytes);

                    Debug.Log($"Image downloaded successfully to: {filePath}");
                }
                else
                {
                    Debug.LogError($"Failed to download image. Status code: {response.StatusCode}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"An error occurred: {ex.Message}");
            }
        }
    }
}
