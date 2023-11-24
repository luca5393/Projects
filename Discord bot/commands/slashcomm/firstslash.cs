using Discord_bot.profilesystem;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Discord_bot.commands.slashcomm
{
    public class firstslash : ApplicationCommandModule

    {
        public string path = @"C:\Users\luca5393\source\repos\Discord bot\Discord bot\profilesystem\UserInfo.json";
        [SlashCommand("donate", "Donate money to another user")]

        public async Task donatecommand(InteractionContext ctx, [Option("Amount", "The Bet amount")] long amo, [Option("User", "The User that you want to give the money to")] DiscordUser giveuser)
        {
            string username = ctx.User.Username;

            var coinsstore = new coinsstore();
            var doesexist = coinsstore.Userexists(username);
            var reciverexist = coinsstore.Userexists(giveuser.Username);

            if (doesexist && reciverexist)
            {
                int amount = Convert.ToInt32(amo);
                var user = coinsstore.GetUser(username);
                var reciver = coinsstore.GetUser(giveuser.Username);
                if (amount < 0)
                {
                    coinsstore.quickemblem(true, ctx,
                        "You cant give minus",
                        "",
                        DiscordColor.Red);
                }
                else
                {
                    if (user.Coins >= amount)
                    {
                        coinsstore.Useraddremovecoins(user.Username, -amount);
                        coinsstore.Useraddremovecoins(reciver.Username, amount);

                        coinsstore.quickemblem(true, ctx,
                        $"{user.Username} donated {amount} to {reciver.Username}",
                        "",
                        DiscordColor.Azure);
                    }
                    else
                    {
                        coinsstore.quickemblem(true, ctx,
                        "You don't have enough money",
                        "",
                        DiscordColor.Red);

                    }
                }
            }
            else
            {
                coinsstore.quickemblem(true, ctx,
                        "Please make a profile first with /profile",
                        "(Both users have to have a profile)",
                        DiscordColor.Yellow);
            }
        }

        [SlashCommand("profile", "See your profile")]
        public async Task ProfileCommand(InteractionContext ctx, [Option("User", "The User You want to show the profile of")] DiscordUser? giveuser = null)
        {
            if (giveuser == null)
            {
                giveuser = ctx.User;
            }


            var userDetails = new DUser()
            {
                Username = giveuser.Username,
                Coins = 0,
                avatarurl = giveuser.AvatarUrl
            };

            var coinsstore = new coinsstore();
            var doesexist = coinsstore.Userexists(giveuser.Username);

            if (!doesexist)
            {

                var isstored = coinsstore.Userdetails(userDetails);
                if (isstored)
                {

                    var pulleduser = coinsstore.GetUser(giveuser.Username);
                    var embed = new DiscordEmbedBuilder()
                        .WithColor(DiscordColor.Azure)
                        .WithTitle(pulleduser.Username + "'s Profile")
                        .WithThumbnail(pulleduser.avatarurl)
                        .AddField("Coins", pulleduser.Coins.ToString());

                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                }
                else
                {
                    coinsstore.quickemblem(true, ctx,
                        "Something went wrong storing your data",
                        "",
                        DiscordColor.Red);
                }

            }
            else
            {
                var pulleduser = coinsstore.GetUser(giveuser.Username);
                var embed = new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Azure)
                    .WithTitle(pulleduser.Username + "'s Profile")
                    .WithThumbnail(pulleduser.avatarurl)
                    .AddField("Coins", pulleduser.Coins.ToString());

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));

            }

        }


        [SlashCommand("beg", "Try to beg for coins")]
        [SlashCooldown(1, 60, SlashCooldownBucketType.User)]
        public async Task coincommand(InteractionContext ctx)
        {

            string username = ctx.User.Username;



            var coinsstore = new coinsstore();
            var doesexist = coinsstore.Userexists(username);

            if (doesexist)
            {
                Random rnd = new Random();
                string story = "";
                switch (rnd.Next(1, 6))
                {
                    case 1:
                        coinsstore.Useraddremovecoins(username, 100);
                        story = $"A man gave {username} 100 coins";
                        break;
                    case 5:
                        coinsstore.Useraddremovecoins(username, 50);
                        story = $"A man gave {username} 50 coins";
                        break;
                    case 2:
                    case 4:
                        coinsstore.Useraddremovecoins(username, 0);
                        story = $"People just walk by and {username} didnt get any coins";
                        break;
                    case 3:
                        var user = coinsstore.GetUser(username);
                        if (user.Coins >= 100)
                        {
                            coinsstore.Useraddremovecoins(username, -100);
                            story = $"A man stole 100 coins from {username}";
                        }
                        else if (user.Coins <= 0)
                        {
                            coinsstore.Useraddremovecoins(username, 10);
                            story = $"A man tried to steal 100 coins from {username}, but because {username} has 0 coins, the man gave him 10 coins instead.";
                        }
                        else
                        {
                            int amountstole = user.Coins;
                            coinsstore.Useraddremovecoins(username, amountstole);
                            story = $"A man stole {amountstole} from {username}";
                        }
                        break;
                }
                coinsstore.quickemblem(true, ctx,
                        story,
                        "",
                        DiscordColor.Azure);
            }
            else
            {
                coinsstore.quickemblem(true, ctx,
                        "Please make a profile first with /profile",
                        "",
                        DiscordColor.Yellow);
            }

        }

        [SlashCommand("coinflip", "Have a 50/50 chance to win or lose")]
        public async Task coinflipcommand(InteractionContext ctx, [Option("Bet", "The Bet amount")] long bet)
        {

            string username = ctx.User.Username;

            var coinsstore = new coinsstore();
            var doesexist = coinsstore.Userexists(username);

            if (doesexist)
            {
                int betamount = Convert.ToInt32(bet);
                var user = coinsstore.GetUser(username);
                if (user.Coins >= betamount)
                {
                    Random rnd = new Random();
                    var embed = new DiscordEmbedBuilder();

                    switch (rnd.Next(1, 3))
                    {
                        case 1:
                            coinsstore.quickemblem(true, ctx,
                                "You Won",
                                "",
                                DiscordColor.Green);
                            coinsstore.Useraddremovecoins(username, betamount);
                            break;
                        case 2:
                            coinsstore.Useraddremovecoins(username, -betamount);
                            coinsstore.quickemblem(true, ctx,
                                "You Lost",
                                "",
                                DiscordColor.Red);
                            break;
                    }
                }
                else
                {
                    coinsstore.quickemblem(true, ctx,
                            "You dont have enough money",
                            "",
                            DiscordColor.Red);
                }
            }
            else
            {
                coinsstore.quickemblem(true, ctx,
                    "Please make a profile first with /profile",
                    "",
                    DiscordColor.Yellow);
            }
        }

        [SlashCommand("loan", "Loan money from another user")]

        public async Task loancommand(
            InteractionContext ctx,
            [Option("Amount", "The loan amount")] long amount,
            [Option("User", "The User that you want to loan the money from")] DiscordUser lender,
            [Option("time", "The time before you have to pay the loan back (in minutes)")] long timeBeforePayback,
            [Option("payback", "The % of money you have to pay back extra when paying back the loan")] long paybackPercentage)
        {
            string username = ctx.User.Username;

            var coinsstore = new coinsstore();
            var doesexist = coinsstore.Userexists(username);
            var reciverexist = coinsstore.Userexists(lender.Username);

            
                
            

            if (doesexist && reciverexist || doesexist && lender.IsBot)
            {
                if (username == lender.Username)
                {
                    coinsstore.quickemblem(true, ctx,
                        "You cant loan from yourself",
                        "",
                        DiscordColor.Red);
                }
                else
                {
                    int intamount = Convert.ToInt32(amount);
                    int time = Convert.ToInt32(timeBeforePayback);
                    int procentpb = Convert.ToInt32(paybackPercentage);
                    var user = coinsstore.GetUser(username);

                    var reciver = new DUser()
                    {
                        Username = lender.Username,
                        Coins = 100000
                    };
                    if (!lender.IsBot)
                    {
                        reciver = coinsstore.GetUser(lender.Username);
                    }
                    
                    if (time < 0)
                    {
                        coinsstore.quickemblem(true, ctx,
                            "You cant go in minus time",
                            "",
                            DiscordColor.Red);

                    }
                    if (procentpb < 0)
                    {

                        coinsstore.quickemblem(true, ctx,
                            "You Cant have a minus payback %",
                            "",
                            DiscordColor.Red);
                    }
                    if (amount < 0)
                    {
                        coinsstore.quickemblem(true, ctx,
                            "You Cant loan minus",
                            "",
                            DiscordColor.Red);

                    }
                    else
                    {
                        if (amount > reciver.Coins)
                        {
                            coinsstore.quickemblem(true, ctx,
                            $"{reciver.Username} does not have enough money",
                            "",
                            DiscordColor.Red);

                        }
                        else
                        {

                            if (coinsstore.loanexists(user, reciver))
                            {
                                coinsstore.loanrequest(user, reciver, time * 60, intamount, procentpb);
                                
                                var embed = new DiscordEmbedBuilder()
                                    .WithColor(DiscordColor.Azure)
                                    .WithTitle($"{user.Username} want to loan {amount} from {reciver.Username}")
                                    .AddField("Details", $"With the payback time of {time} minutes and a payback % of {procentpb}");


                                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                                if (lender.IsBot)
                                {
                                    if (time * 60 <= 60*60 && procentpb >= 20)
                                    {
                                        coinsstore.Useraddremovecoins(user.Username, intamount);
                                        
                                        var botloan = coinsstore.Getloan(user, reciver);
                                        coinsstore.loanend(botloan, ctx, lender);
                                        coinsstore.quickemblem(false, ctx,
                                                    "Loan successful",
                                                    "",
                                                    DiscordColor.Green);
                                    }
                                    else
                                    {
                                        coinsstore.quickemblem(false, ctx,
                                                    "The Bot Declined",
                                                    "The bot wont accept loans longer than 1 hour and with a payback % of less than 20%",
                                                    DiscordColor.Red);
                                    }


                                }
                            }
                            else
                            {
                                coinsstore.quickemblem(true, ctx,
                                $"Loan already exists",
                                "",
                                DiscordColor.Red);

                            }
                        }
                    }
                }
            }
            else
            {
                coinsstore.quickemblem(true, ctx,
                            "Please make a profile first with /profile (Both users have to have a profile)",
                            "",
                            DiscordColor.Yellow);

            }
        }

        [SlashCommand("accept", "Loan money to another user")]
        public async Task acceptcommand(InteractionContext ctx, [Option("User", "The User")] DiscordUser lender)
        {
            string username = ctx.User.Username;

            var coinsstore = new coinsstore();
            var user = coinsstore.GetUser(username);
            var lenderduuser = coinsstore.GetUser(lender.Username);
            var doesexist = coinsstore.loanexists(lenderduuser, user);
            if (!doesexist)
            {
                Loan loan = coinsstore.Getloan(lenderduuser, user);
                if (user.Coins >= loan.Coins)
                {
                    coinsstore.Useraddremovecoins(lenderduuser.Username, loan.Coins);
                    coinsstore.Useraddremovecoins(user.Username, -loan.Coins);
                    coinsstore.loanend(loan, ctx, lender);
                    coinsstore.quickemblem(true, ctx,
                                "Loan successful",
                                "",
                                DiscordColor.Green);
                }
                else
                {
                    coinsstore.quickemblem(true, ctx,
                            "Not enough money to accept the offer",
                            "",
                            DiscordColor.Red);
                }
            }
            else
            {
                coinsstore.quickemblem(true, ctx,
                            "Loan dosent exist",
                            "",
                            DiscordColor.Red);
            }

        }

        [SlashCommand("cancel", "Cancel your Loan")]
        public async Task cancelcommand(InteractionContext ctx, [Option("User", "The User that you want to loan the money from")] DiscordUser banker)
        {
            string username = ctx.User.Username;

            var coinsstore = new coinsstore();
            var user = coinsstore.GetUser(username);

            var bankeruser = new DUser()
            {
                Username = banker.Username,
                Coins = 100000
            };
            if (!banker.IsBot)
            {
                bankeruser = coinsstore.GetUser(banker.Username);
            }
            var doesexist = coinsstore.loanexists(user, bankeruser);
            if (!doesexist)
            {

                Loan loan = coinsstore.Getloan(user, bankeruser);
                if (loan.active == false)
                {

                    var json = File.ReadAllText(path);
                    var jsonObj = JObject.Parse(json);


                    var loans = jsonObj["loans"].ToObject<List<Loan>>();

                    Loan item = loan;

                    int indexToRemove = loans.FindIndex(loan =>
                        loan.Loaner.Username == item.Loaner.Username &&
                        loan.Banker.Username == item.Banker.Username &&
                        loan.Coins == item.Coins &&
                        loan.Time == item.Time &&
                        loan.Payback == item.Payback &&
                        loan.active == item.active);

                    if (indexToRemove != -1)
                    {
                        loans.RemoveAt(indexToRemove);
                    }

                    jsonObj["loans"] = JArray.FromObject(loans);
                    File.WriteAllText(path, jsonObj.ToString());

                    coinsstore.quickemblem(true, ctx,
                            "Loan successfully cancelled",
                            "",
                            DiscordColor.Green);
                }
                else
                {
                    coinsstore.quickemblem(true, ctx,
                    "You cant cancel a loan that is started",
                    "",
                    DiscordColor.Red);
                }
            }
            else
            {
                coinsstore.quickemblem(true, ctx,
                            "Loan dosent exist",
                            "",
                            DiscordColor.Red);
            }
        }

        [SlashCommand("work", "Try to beg for coins")]
        [SlashCooldown(1, 60 * 30, SlashCooldownBucketType.User)]
        public async Task workcommand(InteractionContext ctx)
        {

            string username = ctx.User.Username;



            var coinsstore = new coinsstore();
            var doesexist = coinsstore.Userexists(username);

            if (doesexist)
            {
                Random rnd = new Random();
                string story = "";
                switch (rnd.Next(1, 6))
                {
                    case 1:
                        coinsstore.Useraddremovecoins(username, 5000);
                        story = $"{username} went to work, made the company a lot of money and got 5000 coins";
                        break;
                    case 2:
                    case 5:
                        coinsstore.Useraddremovecoins(username, 1000);
                        story = $"{username} went to work, worked and got 1000 coins";
                        break;
                    case 4:
                        coinsstore.Useraddremovecoins(username, 500);
                        story = $"{username} went to work, did a little work, and got 500 coins";
                        break;
                    case 3:
                        coinsstore.Useraddremovecoins(username, 100);
                        story = $"{username} went to work, played games the whole time, and got 100 coins.";
                        ;


                        break;
                }
                coinsstore.quickemblem(true, ctx,
                        story,
                        "",
                        DiscordColor.Azure);
            }
            else
            {
                coinsstore.quickemblem(true, ctx,
                        "Please make a profile first with /profile",
                        "",
                        DiscordColor.Yellow);
            }

        }
        [SlashCommand("buy", "Cancel your Loan")]
        public async Task buycommand(InteractionContext ctx, [Choice("Kick (100000)", "kick")]
                                                             [Choice("Ban (10000000)", "ban")]
                                                             [Option("item", "The item that ")] string item
                                                            , [Option("User", "The User that you want use your item on")] DiscordUser enemyuser)
        {
            var coinsstore = new coinsstore();
            var username = ctx.User.Username;
            var member = (DiscordMember)enemyuser;
            var user = coinsstore.GetUser(username);
            switch (item)
            {
                case "kick":
                    if (user.Coins >= 100000)
                    {
                        coinsstore.Useraddremovecoins(username, -100000);
                        await member.RemoveAsync($"{username} used 100000 coins to kick you");
                        coinsstore.quickemblem(true, ctx,
                        $"{username} kicked {member.Username} by buying kick from /buy",
                        "",
                        DiscordColor.Red);
                    }
                    else
                    {
                        coinsstore.quickemblem(true, ctx,
                        "Not enough coins",
                        "",
                        DiscordColor.Red);
                    }
                    break;
                case "ban":
                    if (user.Coins >= 10000000)
                    {
                        coinsstore.Useraddremovecoins(username, -10000000);

                        await ctx.Guild.BanMemberAsync(member, 0, $"{username} used 10000000 coins to ban you");

                        coinsstore.quickemblem(true, ctx,
                        $"{username} banned {member.Username} by buying ban from /buy",
                        "",
                        DiscordColor.Red);
                    }
                    else
                    {
                        coinsstore.quickemblem(true, ctx,
                        "Not enough coins",
                        "",
                        DiscordColor.Red);
                    }
                    break;


                default:
                    coinsstore.quickemblem(true, ctx,
                        "Cant find item",
                        "",
                        DiscordColor.Red);
                    break;
            }
        }

        [SlashCommand("help", "Get help with commands")]
        public async Task helpcommand(InteractionContext ctx)
        {
            var embed = new DiscordEmbedBuilder()
                        .WithColor(DiscordColor.Azure)
                        .WithTitle($"Help")
                        .AddField("/help", "This command gets a list of all commands of lucas bot")
                        .AddField("/profile", "This command creates and shows a profile")
                        .AddField("/beg", "This command lets you begs for coins")
                        .AddField("/work", "This command lets you work for coins")
                        .AddField("/coinflip", "This command allows you to coinflip you money for double or nothing")
                        .AddField("/donate", "This command allows you to donate to another user")
                        .AddField("/buy", "With this command you can buy things.")
                        .AddField("/loan", "This command allows you to ask for a loan from another player")
                        .AddField("/cancel", "This command lets you take back your request for a loan")
                        .AddField("/accept", "This command lets you accept a loan request from another player")
                        .AddField("/startup", "With this command you can start your own business (You can own 1 of each company).")
                        .AddField("/upgrade", "This command allows you upgrade your business")
                        .AddField("/dailyupdate", "This command lets you get your company update (gives you money from your businesses)")
                        .AddField("/company", "This command lets you get the stats for a company")

                        ;

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        [SlashCommand("startup", "Startup your own company")]
        public async Task startupommand(InteractionContext ctx, [
                                                                    Choice("Hotdog_stand_(1000)", "Hotdog stand"),
                                                                    Choice("Food_Truck_(10000)", "Food Truck"),
                                                                    Choice("Coffee_Shop_(50000)", "Coffee Shop"),
                                                                    Choice("Bookstore_(100000)", "Bookstore"),
                                                                    Choice("Fashion_Boutique_(150000)", "Fashion Boutique"),
                                                                    Choice("IT_Business_(200000)", "IT Business"),
                                                                    Choice("Tech_Store_(300000)", "Tech Store"),
                                                                    Choice("Restaurant_(500000)", "Restaurant"),
                                                                    Choice("Fitness_Center_(750000)", "Fitness Center"),
                                                                    Choice("Car_Dealership_(1000000)", "Car Dealership"),
                                                                    Choice("Supermarket_(1500000)", "Supermarket"),
                                                                    Choice("Art_Gallery_(2000000)", "Art Gallery"),
                                                                    Choice("Hotel_Chain_(3000000)", "Hotel Chain"),
                                                                    Choice("Theme_Park_(5000000)", "Theme Park"),
                                                                    Choice("Space_Tourism_Agency_(10000000)", "Space Tourism Agency")
                                                                ]
                                                             [Option("shop_type", "The shop type")] string bus)
        {
            var coinsstore = new coinsstore();
            var username = ctx.User.Username;
            var user = coinsstore.GetUser(username);

            int price = 0;

            switch (bus)
            {
                case "Hotdog stand":
                    price = 1000;
                    break;
                case "Food Truck":
                    price = 10000;
                    break;
                case "Coffee Shop":
                    price = 50000;
                    break;
                case "Bookstore":
                    price = 100000;
                    break;
                case "Fashion Boutique":
                    price = 150000;
                    break;
                case "IT Business":
                    price = 200000;
                    break;
                case "Tech Store":
                    price = 300000;
                    break;
                case "Restaurant":
                    price = 500000;
                    break;
                case "Fitness Center":
                    price = 750000;
                    break;
                case "Car Dealership":
                    price = 1000000;
                    break;
                case "Supermarket":
                    price = 1500000;
                    break;
                case "Art Gallery":
                    price = 2000000;
                    break;
                case "Hotel Chain":
                    price = 3000000;
                    break;
                case "Theme Park":
                    price = 5000000;
                    break;
                case "Space Tourism Agency":
                    price = 10000000;
                    break;

                default:
                    coinsstore.quickemblem(true, ctx,
                        "Cant find business",
                        "",
                        DiscordColor.Red);
                    return;

            }

            if (user.Coins >= price)
            {

                if (coinsstore.companyexists(user.Username, bus))
                {
                    coinsstore.quickemblem(true, ctx,
                    "Company already exists",
                    "You cant create two companies of the same type.",
                    DiscordColor.Red);
                }
                else
                {
                    coinsstore.Useraddremovecoins(username, -price);
                    try
                    {
                        var json = File.ReadAllText(path);
                        var jsonObj = JObject.Parse(json);


                        var companies = jsonObj["company"].ToObject<List<Company>>();


                        Company company = new Company()
                        {
                            owner = user,
                            workers = 1,
                            bustype = bus,
                            coingeneration = price / 2,
                            operatingcost = price / 5,
                        };


                        companies.Add(company);

                        jsonObj["company"] = JArray.FromObject(companies);
                        File.WriteAllText(path, jsonObj.ToString());
                        coinsstore.quickemblem(true, ctx,
                            $"You have just made a {bus} company",
                            $"it has a basic generation of {price / 2} with a operating cost of {price / 5}. You can upgrade it with /upgrade",
                            DiscordColor.Green);
                    }
                    catch (Exception)
                    { }
                }

            }
            else
            {
                coinsstore.quickemblem(true, ctx,
                "Not enough coins",
                "",
                DiscordColor.Red);
            }
        }
        [SlashCommand("upgrade", "Upgrade your company")]
        public async Task upgradecommand(InteractionContext ctx, [
                                                                    Choice("Hotdog_stand_(1000)", "Hotdog stand"),
                                                                    Choice("Food_Truck_(10000)", "Food Truck"),
                                                                    Choice("Coffee_Shop_(50000)", "Coffee Shop"),
                                                                    Choice("Bookstore_(100000)", "Bookstore"),
                                                                    Choice("Fashion_Boutique_(150000)", "Fashion Boutique"),
                                                                    Choice("IT_Business_(200000)", "IT Business"),
                                                                    Choice("Tech_Store_(300000)", "Tech Store"),
                                                                    Choice("Restaurant_(500000)", "Restaurant"),
                                                                    Choice("Fitness_Center_(750000)", "Fitness Center"),
                                                                    Choice("Car_Dealership_(1000000)", "Car Dealership"),
                                                                    Choice("Supermarket_(1500000)", "Supermarket"),
                                                                    Choice("Art_Gallery_(2000000)", "Art Gallery"),
                                                                    Choice("Hotel_Chain_(3000000)", "Hotel Chain"),
                                                                    Choice("Theme_Park_(5000000)", "Theme Park"),
                                                                    Choice("Space_Tourism_Agency_(10000000)", "Space Tourism Agency")
                                                                ]
                                                             [Option("shop_type", "The shop type you want to upgrade")] string bus,

                                                            [Choice("Workers", "worker")]
                                                             [Choice("basic_generation", "generation")]
                                                             [Choice("operating_cost", "operating")]
                                                             [Option("upgrade_type", "The upgrade you want to give your company")] string upgradetype)
        {
            var coinsstore = new coinsstore();
            var username = ctx.User.Username;
            var user = coinsstore.GetUser(username);
            
            if (coinsstore.companyexists(username, bus))
            {
                var comp = coinsstore.Getcompany(username, bus);
                switch (upgradetype)
                {
                    case "worker":
                        if (comp.workers <= 10)
                        {
                            if (user.Coins - ((int)Math.Round((double)comp.workers * 100 * ((double)comp.coingeneration / 1000))) >= 0)
                            {
                                coinsstore.Useraddremovecoins(username, -(int)Math.Round((double)comp.workers * 100 * ((double)comp.coingeneration / 1000)));
                                coinsstore.companyupgrade(comp, upgradetype);
                                comp = coinsstore.Getcompany(username, bus);
                                coinsstore.quickemblem(true, ctx,
                                "Upgrade success",
                                $"The next upgrade costs {(int)Math.Round((double)comp.workers * 100 * ((double)comp.coingeneration / 1000))} coins",
                                DiscordColor.Azure);
                            }
                            else
                            {
                                coinsstore.quickemblem(true, ctx,
                                "You dont have enogh coins for this upgrade",
                                $"The next upgrade costs {(int)Math.Round((double)comp.workers * 100 * ((double)comp.coingeneration / 1000))} coins",
                                DiscordColor.Red);
                            }
                        }
                        else
                        {
                            coinsstore.quickemblem(true, ctx,
                            "You have max of this upgrade",
                            "",
                            DiscordColor.Red);
                        }
                        break;
                    case "generation":
                        if (comp.coingenerationupgrade <= 9)
                        {
                            if (user.Coins - (comp.coingeneration * 2) >= 0)
                            {
                                coinsstore.Useraddremovecoins(username, -(comp.coingeneration * 2));
                                coinsstore.companyupgrade(comp, upgradetype);
                                comp = coinsstore.Getcompany(username, bus);
                                coinsstore.quickemblem(true, ctx,
                                "Upgrade success",
                                $"The next upgrade costs {comp.coingeneration *2} coins",
                                DiscordColor.Azure);
                            }
                            else
                            {
                                coinsstore.quickemblem(true, ctx,
                                "You dont have enogh coins for this upgrade",
                                $"The next upgrade costs {comp.coingeneration * 2} coins",
                                DiscordColor.Red);
                            }
                        }
                        else
                        {
                            coinsstore.quickemblem(true, ctx,
                            "You have max of this upgrade",
                            "",
                            DiscordColor.Red);
                        }
                        break;
                    case "operating":
                        if (comp.operationupgrades <= 9)    
                        {
                            if (user.Coins - ((int)Math.Round((double)comp.operatingcost * 2 * ((double)comp.coingeneration / 10000))) >= 0)
                            {
                                coinsstore.Useraddremovecoins(username, -(int)Math.Round((double)comp.operatingcost * 2 * ((double)comp.coingeneration / 10000)));
                                coinsstore.companyupgrade(comp, upgradetype);
                                comp = coinsstore.Getcompany(username, bus);
                                coinsstore.quickemblem(true, ctx,
                                "Upgrade success",
                                $"The next upgrade costs {(int)Math.Round((double)comp.operatingcost * 2 * ((double)comp.coingeneration / 10000))} coins",
                                DiscordColor.Azure);
                            }
                            else
                            {
                                coinsstore.quickemblem(true, ctx,
                                "You dont have enogh coins for this upgrade",
                                $"The next upgrade costs {(int)Math.Round((double)comp.operatingcost * 2 * ((double)comp.coingeneration / 10000))} coins",
                                DiscordColor.Red);
                            }
                        }
                        else
                        {
                            coinsstore.quickemblem(true, ctx,
                            "You have max of this upgrade",
                            "",
                            DiscordColor.Red);
                        }
                        break;
                }
                

            }
            else
            {
                coinsstore.quickemblem(true, ctx,
                "You dont own that company",
                "/startup to buy a company (you can own 1 of each)",
                DiscordColor.Red);
            }
        }

        [SlashCommand("dailyupdate", "Get the daily update for you companies")]
        [SlashCooldown(3, 60 * 60*24, SlashCooldownBucketType.User)]
        public async Task dalyupdatecommand(InteractionContext ctx)
        {
            var coinsstore = new coinsstore();
            string username = ctx.User.Username;

            
            int totalmoney = 0;
            int totalcost = 0;
            var embed = new DiscordEmbedBuilder()
                        .WithColor(DiscordColor.Azure)
                        .WithTitle($"{username}'s Daily Update");
            using (StreamReader sr = new StreamReader(@"C:\Users\luca5393\source\repos\Discord bot\Discord bot\profilesystem\UserInfo.json"))
            {
                string json = sr.ReadToEnd();
                jsonfile userget = JsonConvert.DeserializeObject<jsonfile>(json);

                foreach (var comp in userget.company)
                {
                    if (comp.owner.Username == username)
                    {
                        int cost = comp.operatingcost + (comp.operatingcost / 10 * comp.workers);
                        int money = (int)Math.Round(comp.coingeneration * comp.workers * (new Random().NextDouble() * (2 - 0.8) + 0.8));
                        embed.AddField(comp.bustype, $"Money: {money} {Environment.NewLine} Cost: {cost} {Environment.NewLine} Profit: {money - cost}");
                        totalmoney += money;
                        totalcost += cost;

                    }
                }
               
            }
            embed.AddField("Total", $"Money: {totalmoney} {Environment.NewLine} Cost: {totalcost} {Environment.NewLine} Profit: {totalmoney - totalcost}");
            coinsstore.Useraddremovecoins(username, totalmoney-totalcost);
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }
        [SlashCommand("company", "Get the stats for a company")]
        public async Task businesscommand(InteractionContext ctx, [
                                                                    Choice("Hotdog_stand_(1000)", "Hotdog stand"),
                                                                    Choice("Food_Truck_(10000)", "Food Truck"),
                                                                    Choice("Coffee_Shop_(50000)", "Coffee Shop"),
                                                                    Choice("Bookstore_(100000)", "Bookstore"),
                                                                    Choice("Fashion_Boutique_(150000)", "Fashion Boutique"),
                                                                    Choice("IT_Business_(200000)", "IT Business"),
                                                                    Choice("Tech_Store_(300000)", "Tech Store"),
                                                                    Choice("Restaurant_(500000)", "Restaurant"),
                                                                    Choice("Fitness_Center_(750000)", "Fitness Center"),
                                                                    Choice("Car_Dealership_(1000000)", "Car Dealership"),
                                                                    Choice("Supermarket_(1500000)", "Supermarket"),
                                                                    Choice("Art_Gallery_(2000000)", "Art Gallery"),
                                                                    Choice("Hotel_Chain_(3000000)", "Hotel Chain"),
                                                                    Choice("Theme_Park_(5000000)", "Theme Park"),
                                                                    Choice("Space_Tourism_Agency_(10000000)", "Space Tourism Agency")
                                                                ]
                                                             [Option("shop_type", "The shop type you want to upgrade")] string bus
                                                            , [Option("User", "The User You want to show the profile of")] DiscordUser? giveuser = null)
        {
            if (giveuser == null)
            {
                giveuser = ctx.User;
            }

            var coinsstore = new coinsstore();
            var doesexist = coinsstore.companyexists(giveuser.Username, bus);

            if (doesexist)
            {
                var pulleduser = coinsstore.Getcompany(giveuser.Username, bus);
                var embed = new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Azure)
                    .WithTitle(pulleduser.owner.Username + "'s Profile " + pulleduser.bustype)
                    .AddField("Workers", $"{pulleduser.workers} / 11 {Environment.NewLine} Next upgrade cost: {(int)Math.Round((double)pulleduser.workers * 100 * ((double)pulleduser.coingeneration / 1000))}")  // Her er fix til round
                    .AddField("Coingeneration", $"{pulleduser.coingenerationupgrade} /10 {Environment.NewLine} Next upgrade cost: {pulleduser.coingeneration*2}")
                    .AddField("Operatingcost", $"{pulleduser.operationupgrades} / 10  {Environment.NewLine} Next upgrade cost: {(int)Math.Round((double)pulleduser.operatingcost * 2 * ((double)pulleduser.coingeneration / 10000))}");
                    


                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));

            }
            else
            {
                coinsstore.quickemblem(true, ctx,
                "That company dont exist",
                "/startup to buy a company (you can own 1 of each)",
                DiscordColor.Red);
            }

        }
    }


}
