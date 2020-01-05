using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace LuYiLu.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public string PageHtml { get; set; }

        public async Task OnGetAsync(string p)
        {
            if (string.IsNullOrEmpty(p)) return;

            var url = p;

            var doc = await new HtmlWeb().LoadFromWebAsync(url, Encoding.GetEncoding("gb2312"));

            foreach (var script in doc.DocumentNode.SelectNodes("//script"))
            {
                script.Remove();
            }

            PageHtml = doc.DocumentNode.OuterHtml;
        }
    }
}
