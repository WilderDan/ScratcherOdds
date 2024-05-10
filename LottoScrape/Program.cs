using Microsoft.Playwright;
using LottoScrape;
using System.Text.Json;
using System.IO;
using System.Globalization;

if(args.Length < 1)
{
    Console.WriteLine("Missing filename to save JSON");
    System.Environment.Exit(1);
}

var filename = args[0];


List<ScratcherInfo> scratcherInfoList = new List<ScratcherInfo>();

const string BaseUrl = "https://www.molottery.com";
const string ActiveScratcherUrl  = BaseUrl + "/scratchers/?type=all";

using var playwright = await Playwright.CreateAsync();
await using var browser = await playwright.Chromium.LaunchAsync();
var rootPage = await browser.NewPageAsync();
await rootPage.GotoAsync(ActiveScratcherUrl);
var page = await browser.NewPageAsync();

// Change to selector?
var xpathSelector = "//html/body/div[1]/main/div/div[2]/div/div/div[4]/div";
var scratcherLocator = rootPage.Locator(xpathSelector).Locator("> *");

// Change to count activeScratchers
var count = await scratcherLocator.CountAsync();
for (int i = 0; i < count; i++)
{
    var scratcherDataLocator = scratcherLocator.Nth(i).Locator("> *");
    var href = await scratcherDataLocator.Nth(4).GetAttributeAsync("href");
    var scratcherNumber = href.Split("/")[2];
    Console.WriteLine(href);

    await page.GotoAsync(BaseUrl + href);

    var name = await page.Locator("#block-molottery-content > div > div:nth-child(1) > div > h1").InnerTextAsync();
    var ticketPrice = await page.Locator("#scratchers-single-info-standard > div > div:nth-child(4) > div.scratchers-single-info__body").InnerTextAsync();
    var averageChances = await page.Locator("#scratchers-single-info-standard > div > div:nth-child(6) > div.scratchers-single-info__body").InnerTextAsync();
    var imageUrl = await page.Locator("#block-molottery-content > div > div:nth-child(1) > div > div.scratchers-single__columns > div.scratchers-single__right > a > img").GetAttributeAsync("src");
    var startDate = await page.Locator("#scratchers-single-info-standard > div > div:nth-child(1) > div.scratchers-single-info__body").InnerTextAsync();
    var endDate = await page.Locator("#scratchers-single-info-standard > div > div:nth-child(2) > div.scratchers-single-info__body").InnerTextAsync();
    var expireDate = await page.Locator("#scratchers-single-info-standard > div > div:nth-child(3) > div.scratchers-single-info__body").InnerTextAsync();
    var topPrize = await page.Locator("#scratchers-single-info-standard > div > div:nth-child(5) > div.scratchers-single-info__body").InnerTextAsync();
    
    List<PrizeRow> prizeTable = new List<PrizeRow>();

    var trLocator = page.Locator("#block-molottery-content > div > div:nth-child(2) > div > div.col-xl-8.col-lg-10.col-md-12.col-sm-12.offset-xl-2.offset-lg-1 > table > tbody").Locator("> *");
    var rowCount = await trLocator.CountAsync();
    for (int j = 0; j < rowCount; j++)
    {
        var tdLocator = trLocator.Nth(j).Locator("> *");

        var prizeLevel = await tdLocator.Nth(0).TextContentAsync();
        var totalPrizes = await tdLocator.Nth(1).TextContentAsync();
        var unclaimedPrizes = await tdLocator.Nth(2).TextContentAsync();

        prizeTable.Add(new PrizeRow(
            Int32.Parse(prizeLevel.Substring(1), NumberStyles.AllowThousands), 
            Int32.Parse(totalPrizes, NumberStyles.AllowThousands), 
            Int32.Parse(unclaimedPrizes, NumberStyles.AllowThousands)));
    }

    scratcherInfoList.Add(new ScratcherInfo
    {
        Name = name,
        OneInChanceWin = Double.Parse(averageChances.Split(" ")[2]),
        TicketPrice = Int32.Parse(ticketPrice.Substring(1)),
        PrizeTable = prizeTable,
        ImageUrl = BaseUrl + imageUrl,
        StartDate = startDate,
        EndDate = endDate,
        ExpireDate = expireDate,
        TopPrize = Int32.Parse(topPrize.Substring(1), NumberStyles.AllowThousands),
        PageUrl = BaseUrl + href,
        //ImageUrl = $"https://www.molottery.com/sites/default/files/scratchers/tile/{scratcherNumber}.png"
    });
}

//string jsonString = JsonSerializer.Serialize(scratcherInfoList);
string jsonString = JsonSerializer.Serialize(new { TimeStamp = DateTime.Now.ToString(), Data = scratcherInfoList });

using (StreamWriter sw = File.CreateText(filename))
{
    sw.Write(jsonString);
}