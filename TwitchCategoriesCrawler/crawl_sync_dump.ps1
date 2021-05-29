dotnet run -c Release -- crawl -l=en
dotnet run -c Release -- sync -l=en -c
dotnet run -c Release -- sync -l=fr -c
dotnet run -c Release -- sync -l=it -c
dotnet run -c Release -- sync -l=de -c
dotnet run -c Release -- sync -l=es -c
dotnet run -c Release -- sync -l=da -c
dotnet run -c Release -- sync -l=sv -c
dotnet run -c Release -- sync -l=ru -c
rm gamedb.csv
dotnet run -c Release -- export -o="gamedb.csv"
rm gamedb_en.csv
dotnet run -c Release -- export -l=en -o="gamedb_en.csv"
rm gamedb_fr.csv
dotnet run -c Release -- export -l=fr -o="gamedb_fr.csv"
rm gamedb_it.csv
dotnet run -c Release -- export -l=it -o="gamedb_it.csv"
rm gamedb_de.csv
dotnet run -c Release -- export -l=de -o="gamedb_de.csv"
rm gamedb_es.csv
dotnet run -c Release -- export -l=es -o="gamedb_es.csv"