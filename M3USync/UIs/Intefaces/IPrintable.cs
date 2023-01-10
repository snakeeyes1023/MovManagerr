using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using M3USync.Data;
using M3USync.Data.Abstracts;

namespace M3USync.Models.Intefaces
{
    public interface IExportable<T> where T : class
    {
        string Export(IExporter<T> export);
    }

    public interface IExporter<T>
    {
        string Export(T obj);
    }

    public class HtmlContentExporter : IExporter<Content>
    {
        public HtmlCardBuilder ConvertToCard(Content obj)
        {
            return new HtmlCardBuilder()
                .Title(obj.Name)
                .Img(obj.Poster)
                .AddTags(obj.Tags);
        }

        public string Export(Content obj)
        {
            var card = ConvertToCard(obj);

            if (obj is Movie movie && movie.TmdbMovie != null)
            {
                card = card.Description(movie.TmdbMovie.Overview)
                    .Title(movie.TmdbMovie.Title);
            }
            return card
                .Build();
        }
    }

    public class HtmlCardBuilder
    {
        private string img;
        private string title;
        private string description;
        private IEnumerable<string> tags = Enumerable.Empty<string>();


        public HtmlCardBuilder Title(string newBody)
        {
            title = newBody;
            return this;
        }

        public HtmlCardBuilder Description(string newDescription)
        {
            description = newDescription;
            return this;
        }

        public HtmlCardBuilder Img(string newImg)
        {
            img = newImg;
            return this;
        }

        
        public HtmlCardBuilder AddTags(params string[] newTags)
        {
            tags = tags.Concat(newTags);
            return this;
        }

        public HtmlCardBuilder AddTags(IEnumerable<string> newTags)
        {
            tags = tags.Concat(newTags);
            return this;
        }

        public string Build()
        {
            var tagHtml = string.Join("", tags.Select(t => $"<span class='badge badge-primary px-3 py-2 my-2'>{t}</span>"));
            return $@"<div class='card'>
            <img class='card-img-top' src='{img}' alt='Card image cap'>
            <div class='card-body'>
                <h5 class='card-title'>{title}</h5>
                <p class='card-text'>{description}</p>
                {tagHtml}
            </div>
            </div>";
        }
    }
}
