using AngleSharp;
using AngleSharp.Dom;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ToyruParsingAngeSharpTestTask
{
    class Parser
    {
        const string address = "https://www.toy.ru/catalog/boy_transport/";
        readonly IConfiguration config = Configuration.Default.WithDefaultLoader();

        public List<ToyInfo> Toys { get; set; } = new();

        private void WriteToCSV()
        {
            using (StreamWriter streamWriter = new("D:\\VERA\\.Programming\\C#\\project\\.Resources\\Toys.csv"))
            using (CsvWriter csvWriter = new(streamWriter, CultureInfo.InvariantCulture))
                csvWriter.WriteRecords(Toys);
        }

        private void Print(ToyInfo toy)
        {
            Console.WriteLine("------------------------------------------");
            Console.WriteLine($"Название {toy.Name}");
            Console.WriteLine($"Наличие {toy.Availability}");
            Console.WriteLine($"Хлебные крошки {toy.Breadcrumbs}");
            Console.WriteLine($"Регион {toy.RegionName}");
            Console.WriteLine($"Ссылка {toy.Link}");
            Console.WriteLine($"Цена {toy.Price}");
            Console.WriteLine($"Старая цена {toy.OldPrice}");
            Console.WriteLine($"Ссылки на картинки {toy.Images}");
            Console.WriteLine("------------------------------------------");
        }

        private async Task CreateEntityAsync(string toyLink, IBrowsingContext context)
        {
            ToyInfo myToy = new();
            var toy = context.OpenAsync(toyLink.ToString()).Result;

            myToy.Images = "";
            var images = toy.QuerySelectorAll("img.img-fluid").Where(i => i.GetAttribute("alt") == "");
            foreach (var img in images)
                myToy.Images += img.GetAttribute("src") + " | ";

            if (toy.QuerySelector("span.old-price") == null)
                myToy.OldPrice = "-";
            else
                myToy.OldPrice = toy.QuerySelector("span.old-price").TextContent;

            myToy.Breadcrumbs = "";
            var bread = toy.QuerySelector("nav.breadcrumb")
                 .Children
                 .Where(b => b?.ClassName != "breadcrumb-item active d-none");

            foreach (var crumb in bread)
                myToy.Breadcrumbs += crumb.TextContent + " > ";

            myToy.Name = toy.QuerySelector("h1.detail-name").TextContent;
            myToy.Price = toy.QuerySelector("span.price").TextContent;
            myToy.Link = toy.Url;
            myToy.RegionName = toy.QuerySelector("div.col-12.select-city-link").Children[1].TextContent.Trim();
            myToy.Availability = toy.QuerySelector("span.ok").TextContent;

            toy.Dispose();

            Toys.Add(myToy);

            await Task.Run(() => Print(myToy));
        }

        private async Task GetDocumentsLinkAsync(Task<IDocument> doc, IBrowsingContext context)
        {
            var res = doc.Result;
            Console.WriteLine(res.Title);

            var toysRef = res.QuerySelectorAll("meta").Where(m => m.GetAttribute("itemprop") == "url");

            foreach (var toy in toysRef)
            {
                string link = toy.GetAttribute("Content");
                await CreateEntityAsync(link, context);
            }

            res.Dispose();
        }

        public async Task<List<ToyInfo>> GetToys()
        {
            using var context = BrowsingContext.New(config);

            var document = await context.OpenAsync(address);

            var docs = new List<Task<IDocument>>() { context.OpenAsync(address) };

            var pagesCount = int.Parse(document.GetElementsByClassName("page-link")[7].TextContent);

            for (int i = 0; i < pagesCount; i++)
            {
                var res = docs[i].Result;

                if (i < pagesCount - 1)
                {
                    var link = "https://www.toy.ru"
                        + res.QuerySelectorAll("a.page-link")
                        .Where(a => a.TextContent == "След.")
                        .First()
                        .GetAttribute("Href");
                    docs.Add(context.OpenAsync(link));
                }

                await GetDocumentsLinkAsync(docs[i], context);

                res.Dispose();
            }

            WriteToCSV();

            return Toys;
        }
    }
}