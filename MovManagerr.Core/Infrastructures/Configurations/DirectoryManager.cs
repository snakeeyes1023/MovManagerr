﻿namespace MovManagerr.Core.Infrastructures.Configurations
{
    public class DirectoryManager
    {
        public readonly string _BasePath;

        public DirectoryManager(string basePath)
        {
            _BasePath = basePath;
        }

        public virtual void CreateDirectory(string path)
        {
            var fullPath = Path.Combine(_BasePath, path);
            Directory.CreateDirectory(fullPath);
        }

        public virtual void DeleteDirectory(string path)
        {
            Directory.Delete(Path.Combine(_BasePath, path));
        }

        public void CreateDirectory(string path, string name)
        {
            var fullPath = Path.Combine(_BasePath, path, name);
            CreateDirectory(fullPath);
        }

        public DirectoryManager CreateSubInstance(string path)
        {
            return new DirectoryManager(Path.Combine(_BasePath, path));
        }

        public bool VerifyAccessibilty()
        {
            try
            {
                CreateDirectory($"TEST");
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
