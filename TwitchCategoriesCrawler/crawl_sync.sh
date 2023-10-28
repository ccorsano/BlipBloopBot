echo "Starting crawler"
dotnet TwitchCategoriesCrawler.dll crawl -l=en
echo "Starting en sync"
dotnet TwitchCategoriesCrawler.dll sync -l=en -c
echo "Starting fr sync"
dotnet TwitchCategoriesCrawler.dll sync -l=fr -c
echo "Starting it sync"
dotnet TwitchCategoriesCrawler.dll sync -l=it -c
echo "Starting de sync"
dotnet TwitchCategoriesCrawler.dll sync -l=de -c
echo "Starting es sync"
dotnet TwitchCategoriesCrawler.dll sync -l=es -c
echo "Starting da sync"
dotnet TwitchCategoriesCrawler.dll sync -l=da -c
echo "Starting sv sync"
dotnet TwitchCategoriesCrawler.dll sync -l=sv -c
echo "Starting ru sync"
dotnet TwitchCategoriesCrawler.dll sync -l=ru -c