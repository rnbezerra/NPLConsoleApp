using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Crawler
{
    public static class DicioCrawler
    {
        public static void Crawl()
        {
            //search word
            //    http://www.dicio.com.br/pesquisa.php?q={word}

            //if page contains {word} within "//*[@id="content"]/h1"
            //    search tag
            //        "//*[@id="content"]/p[@class='adicional']/b[1]"
            //            or
            //        "Classe gramatical" within "<p class="adicional">"
            //else
            //    if page contains "Busca por {word}"
	
        }
    }
}
