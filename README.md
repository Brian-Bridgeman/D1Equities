[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-22041afd0340ce965d47ae6ef1cefeee28c7c493a6346c4f15d667ab976d596c.svg)](https://classroom.github.com/a/TyBvFPPq)
# .net25-oop-group
Grupparbete för första kursen

### OBS! 

Hej Oscar, vi skulle vilja meddela att eftersom programmet funkar i samband med Amerikanska börsen, så skulle vi 
uppskatta om du kunde köra programmet efter klockan 15:30 och innan klockan 22:00 annars kommer den vara väldigt 
tråkig att använda.

### D1 Equities

Känner du att du vill kunna handla aktier utan risken att tappa dina livsbesparingar? Eller till och med kunna
öva på din investeringsstrategi innan du börjar handla på riktigt? Det är precis därför vi har skapat D1 equities,
ett program som låter dig handla fritt på den riktiga Amerikanska marknaden med låtsas pengar. När programmet körs 
kan du logga in med ditt egna konto och börja handla med ett kontosaldo på $10,000 så fort marknaden är öppen. 

### Krav för att komma igång

För att programmet ska kunna fungera så finns det några krav:

- .NET 8.0 eller senare.
- Windows som operativsystem
- .env med API credentials i samma mapp som den kompilerade .exe. Utan denna fungerar inte programmet.

## Installation

1. Kör nedan kommando i Windows Terminalen / CMD:
   ```bash
   git clone https://github.com/EduEdugrade/net25-kurs-1-grupparbete-d1.git && cd net25-kurs-1-grupparbete-d1\D1Equities.GUI && dotnet build && explorer bin\Debug\net8.0-windows
   ```

2. Lägg .env filen i mappen som öppnas

3. Kör `D1Equities.GUI.exe`


## Användning

`Login:` När du startar programmet kommer du se en login view där användarnamn och lösenord krävs. Som standard är det "a"
på båda fält och när du trycker på login knappen hamnar du i programmet. Användarnamnet och lösenordet går att ändra
på i "users.json" filen i repo mappen.

`Home:` När du är inloggad hamnar du inne på home view där du kan se den totala värdet på din portfölj samt din balans. Till
höger på det hittar du hur mycket du har gått upp eller ner på ditt konto, både i $ och %. Längst åt höger ser du hur 
många olika aktier du äger och på den nedre sektion hittar du en historik på alla dina köp och sälj. 

`Portfolio:` Nästa view är portfolio view, den visar alla aktier du äger samt information som ticker namn, antal du äger, 
medelpriset du har handlat varje aktie för, aktiens nuvarande pris och din profit/loss i $ för varje aktie du har handlat.

`Market:` Här kan du köpa dina aktier. Åt vänster hittar en lista på över 7000 olika aktier, du kan söka efter en specifik aktie
genom att skriva ticker eller företagsnamnet i sök fältet och sedan trycka på den du vill köpa för att komma till stock view. Åt höger 
hittar du en lista på dom 10 mest framgångsrika aktier dom senaste 24 timmar samt information om hur mycket dom har gått upp, bredvid 
den listan hittar du en lista på dom 10 mest aktiva stock dom senaste 24 timmar.

`Stocks:` När du har valt en aktie du vill handla kommer du att hamna på stock view. Här hittar du det aktuella priset i realtid, hur 
mycket den har gått upp sedan marknaden öppnade och en candlestick graph som visar aktiens prisutveckling i realtid. Du hittar en "Buy"
och "Sell" knapp längre ner, trycker du på buy så kommer du kunna välja hur många aktier av just den företag du vill köpa. Du kan se din 
nuvarande balans och din balans efter köp samt hur många aktier du redan äger. Trycker du på sälj knappen är det i princip samma sak fast
du kan nu sälja dina aktier istället. När du köper en aktie så hamnar den i din portfolio, notera att du kan även komma till stock view 
från portfolio view om du trycker på en aktie i listan. 

Nu har du all informationen du behöver för att komma igång och blir till nästa Warren Buffett, lycka till!



