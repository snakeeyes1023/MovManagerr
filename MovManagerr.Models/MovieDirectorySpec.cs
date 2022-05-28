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
        public MovieDirectorySpec(int key) 
        { 
            this.DirectoryInfo = null;
            this.Movie = null;
            this.Id = key;
            this.Gb = string.Empty;
            this.FullPath = string.Empty;
            this.FileName = string.Empty;
        }
        
        public MovieDirectorySpec(MovieDirectory DirectoryInfo, SearchMovie movie, long size, string path, string fileName)
        {
            this.DirectoryInfo = DirectoryInfo;
            this.Movie = movie;
            this.Id = movie.Id;          
            this.Gb = size.GbRepresentation();
            this.FullPath = path;
            this.FileName = fileName;
        }

        public int Id { get; set; }
        public MovieDirectory DirectoryInfo { get; set; }
        public SearchMovie? Movie { get; set; }
        
        public string Gb { get; }
        public string FullPath { get; }
        public string FileName { get; }

        public bool IsWebRip()
        {
            return FileName.Split("WEBRip").Length > 1;
        }
    }

    public static class FileExtension
    {
        public static string GbRepresentation(this long lenght)
        {
            return $"{Math.Round(lenght / 1024f / 1024f / 1024f, 2)} GB";
        }
    }
}


