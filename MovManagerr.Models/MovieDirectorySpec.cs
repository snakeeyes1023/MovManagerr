using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMDbLib.Objects.Search;

namespace MovManagerr.Models
{
    public class MovieDirectorySpec
    {
        public MovieDirectorySpec(MovieDirectory DirectoryInfo, SearchMovie movie)
        {
            this.DirectoryInfo = DirectoryInfo;
            this.Movie = movie;
            this.Id = movie.Id;

            GetFileInfo();
            if (File != null)
            {
                Gb = File.GbRepresentation();
                FullPath = File.FullName;
                FileName = File.Name;
            }

        }

        public int Id { get; set; }
        public MovieDirectory DirectoryInfo { get; set; }

        private FileInfo File;

        public SearchMovie? Movie { get; set; }
        public string Gb { get; }
        public string FullPath { get; }
        public string FileName { get; }

        public bool IsWebRip()
        {
            return FileName.Split("WEBRip").Length > 1;
        }

        public void GetFileInfo()
        {
            var files = Directory.GetFiles(DirectoryInfo.Path, "*.*", SearchOption.TopDirectoryOnly);
            if (files.Count() == 1)
            {
                File = new FileInfo(files[0]);
                return;
            }
            else if(files.Count() == 0)
            {
                return;
            }
            var biggestFilePath = files.OrderByDescending(f => new FileInfo(f).Length).FirstOrDefault();

            if (!string.IsNullOrEmpty(biggestFilePath))
            {
                File = new FileInfo(biggestFilePath);
            }
        }
    }

    public static class FileExtension
    {
        public static string GbRepresentation(this FileInfo fileInfo)
        {
            return $"{Math.Round(fileInfo.Length / 1024f / 1024f / 1024f, 2)} GB";
        }

        public static string GetFileName(this FileInfo fileInfo)
        {
            return String.IsNullOrEmpty(fileInfo.GetFileName()) ? "Unknown" : fileInfo.GetFileName();
        }
    }
}


