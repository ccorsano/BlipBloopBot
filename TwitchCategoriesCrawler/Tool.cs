using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchCategoriesCrawler
{
    [Command]
    [Subcommand(
        typeof(TwitchCategoriesCrawlerCommand),
        typeof(TwitchCategoriesSynchronizationService),
        typeof(ExportCommand))]
    public class Tool
    {
        protected int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return 1;
        }
    }
}
