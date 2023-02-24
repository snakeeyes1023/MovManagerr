using System.Collections;
using System.Text;

namespace MovManagerr.Core.Downloaders.M3U
{
    public class M3uChunkReadable : IEnumerable<MediaM3u>
    {
        #region Events
        public event Action<MediaM3u, string> OnContentFounded;
        public event Action OnContentRead;
        #endregion


        private readonly List<MediaM3u> _chunks;
        private readonly StringBuilder _pendingChunk;

        public M3uChunkReadable()
        {
            _chunks = new List<MediaM3u>();
            _pendingChunk = new StringBuilder();
        }

        public IEnumerator<MediaM3u> GetEnumerator()
        {
            return _chunks.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _chunks.GetEnumerator();
        }

        /// <summary>
        /// Adds the specified bytes.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        public void Add(byte[] bytes, string source)
        {
            _pendingChunk.Append(Encoding.UTF8.GetString(bytes));

            Proceeded(source);
        }

        /// <summary>
        /// Proceededs this instance.
        /// </summary>
        public void Proceeded(string source)
        {
            // Sépare le chunk en attente par \n
            string[] chunks = _pendingChunk.ToString().Split('\n');

            // Si le dernier chunk n'est pas vide, on le garde en attente
            string pendingChunk = string.Empty;
            if (!chunks.Last().EndsWith("\r"))
            {
                pendingChunk = chunks.Last();
                chunks = chunks.Take(chunks.Length - 1).ToArray();
            }

            // Parcoure chaque chunk
            foreach (string chunk in chunks)
            {
                // Si le chunk est vide, ignore-le
                if (string.IsNullOrEmpty(chunk) || chunk.StartsWith("#EXTM3U"))
                {
                    continue;
                }
                else
                {
                    string cleanChunk = chunk.Replace("\r", string.Empty);

                    // Si le chunk commence par #EXTINF, crée un nouvel objet Extinf et l'ajoute à l'objet Extm3u actuel
                    if (cleanChunk.StartsWith("#EXTINF"))
                    {
                        var extinf = new MediaM3u(cleanChunk);
                        _chunks.Add(extinf);
                    }
                    // Si le chunk ne commence pas par #EXTM3U ni #EXTINF, c'est une URL de fichier, l'ajoute à l'objet Extinf actuel
                    else
                    {
                        _chunks.Last().SetUrl(cleanChunk);
                        OnContentFounded?.Invoke(_chunks.Last(), source);

                        //Remove it from the list cuase it's already read
                        _chunks.RemoveAt(_chunks.Count - 1);
                    }
                }
            }

            // Efface le chunk en attente
            _pendingChunk.Clear();

            if (!string.IsNullOrEmpty(pendingChunk))
            {
                _pendingChunk.Append(pendingChunk);
            }

            OnContentRead?.Invoke();
        }
    }
}