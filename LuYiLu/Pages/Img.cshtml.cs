using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Threading;

namespace LuYiLu
{
    public class ImgModel : PageModel
    {
        public List<string> ImageUrls { get; set; }

        public string Title { get; set; }

        public void OnGet(string p, int mp = 20)
        {
            var cache = HttpContext.Session.GetString($"imgurl={p}&mp=${mp}");
            if (cache != null)
            {
                ImageUrls = cache.Split('\r').ToList();
                return;
            }

            var srcs = new List<string>();
            var htmlWeb = new HtmlWeb() { CaptureRedirect = true };
            var gb2312 = Encoding.GetEncoding("gb2312");

            var cts = new CancellationTokenSource();
            var po = new ParallelOptions()
            {
                CancellationToken = cts.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount,
            };

            try
            {
                var url = p;
                var totalImgCnt = -1;
                var dotIndex = url.LastIndexOf('.');
                var locker = new object();
                Parallel.For(0, mp, po, i =>
                {
                    string newUrl = url;
                    if (i > 0)
                    {
                        newUrl = url.Insert(dotIndex, $"_{i}");
                    }

                    var docTask = htmlWeb.LoadFromWebAsync(newUrl, gb2312, cts.Token);

                    HtmlDocument doc = null;
                    doc = docTask.Result;
                    lock (locker)
                    {
                        if (totalImgCnt == -1)
                        {
                            var titleNode = doc.DocumentNode.SelectSingleNode("//title");
                            var titleMatch = Regex.Match(titleNode.InnerText, @"\[(\d+)P\]");
                            if (titleMatch.Success)
                            {
                                totalImgCnt = int.Parse(titleMatch.Groups[1].Value);
                                Title = titleNode.InnerText;
                            }
                        }
                    }
                    var images = GetImages(doc);
                    lock (locker)
                    {
                        srcs.AddRange(images);
                        if (totalImgCnt != -1 && srcs.Count >= totalImgCnt)
                        {
                            cts.Cancel(true);
                        }
                    }
                });
            }
            catch (OperationCanceledException e)
            {
            }
            catch (AggregateException e)
            {

            }
            finally
            {
                cts.Dispose();
            }

            //var url = p;
            //var doc = await htmlWeb.LoadFromWebAsync(url, gb2312);

            //srcs.AddRange(GetImages(doc));

            //var titleNode = doc.DocumentNode.SelectSingleNode("//title");
            //var totalImgCnt = int.Parse(Regex.Match(titleNode.InnerText, @"\[(\d+)P\]").Groups[1].Value);
            //Title = titleNode.InnerText;

            //var dotIndex = url.LastIndexOf('.');
            //for (int i = 2; srcs.Count < totalImgCnt; i++)
            //{
            //    var newUrl = url.Insert(dotIndex, $"_{i}");
            //    srcs.AddRange(GetImages(await htmlWeb.LoadFromWebAsync(newUrl, gb2312)));
            //}

            ImageUrls = srcs;

            HttpContext.Session.SetString($"imgurl={p}&mp=${mp}", string.Join('\r', srcs));
        }

        private List<string> GetImages(HtmlDocument doc)
        {
            var imgNodes = doc.DocumentNode.SelectNodes("//*[contains(@class, 'article-content')]//img");
            var list = new List<string>();
            if (imgNodes != null)
            {
                for (int i = 0; i < imgNodes.Count; i++)
                {
                    var imgNode = imgNodes[i];
                    var src = imgNode.Attributes["src"].Value;
                    list.Add(src);
                }
            }
            return list;
        }
    }
}